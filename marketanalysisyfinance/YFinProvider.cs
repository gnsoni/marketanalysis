// ----------------------------------------------------------------------------
// Owner   : gnsoni
// Project : MarketAnalysis
// Licence : MIT - see LICENSE in the repository root
// ----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace MarketAnalysis.YFinance
{
    using MarketAnalysis.YFinance.Entities;
    using MarketAnalysis.YFinance.Entities.Search;
    using System.Text.Json;
    using System.Web;

    /*
     * GSoni - Get data for symbol from Yahoo finance.
     *
     * The API takes Unix date time stamp.
     *
     * Yahoo finance api find listing
     *  URL: https://query1.finance.yahoo.com/v1/finance/search?q=<listingName>
     *
     *
     * Yahoo finance api usage.
     *      Today: https://query1.finance.yahoo.com/v8/finance/chart/GC=F?interval=1d
     *      Daily Month:https://query2.finance.yahoo.com/v8/finance/chart/GC=F?interval=1d&period1=1651968000&period2=1686182400
     *
     */

    public class YFinProvider
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        
        public YFinProvider(int cacheCapacity = 100, int cacheDurationMinutes = 30)
        {
            _searchCacheCapacity = cacheCapacity;
            _searchCacheDuration = TimeSpan.FromMinutes(cacheDurationMinutes);
        }

        private readonly int _searchCacheCapacity;
        private readonly TimeSpan _searchCacheDuration;
        private static readonly object SearchCacheLock = new();
        private static readonly Dictionary<string, CachedSearch> SearchCache = new();
        private static readonly LinkedList<string> SearchCacheOrder = new();

        private sealed class CachedSearch
        {
            public CachedSearch(string response, DateTimeOffset expiresAt, LinkedListNode<string> orderNode)
            {
                Response = response;
                ExpiresAt = expiresAt;
                OrderNode = orderNode;
            }

            public string Response { get; }
            public DateTimeOffset ExpiresAt { get; }
            public LinkedListNode<string> OrderNode { get; }
        }

        public DateTime ConvertFromUnixTimestamp(double timestamp)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return origin.AddSeconds(timestamp);
        }

        public double ConvertToUnixTimestamp(DateTime reftime)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return (TimeSpan.FromTicks(reftime.Date.Ticks - origin.Ticks)).TotalSeconds;
        }

        private String yCookie = string.Empty;

        public async Task<YSearchResult> Search(string symbol)
        {
            string cacheKey = symbol ?? string.Empty;
            bool cacheMiss = !TryGetCachedSearchResponse(cacheKey, out string resData);
            if (cacheMiss)
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
                _httpClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36");

                var response = await _httpClient.GetAsync($"https://query1.finance.yahoo.com/v1/finance/search?q={symbol}");

                response.EnsureSuccessStatusCode();

                resData = await response.Content.ReadAsStringAsync();

                resData = resData.Replace("quotes", "yItems").Replace("isYahooFinance", "isYFinance");
            }

            var result = JsonSerializer.Deserialize<YSearchResult>(resData)
                ?? throw new JsonException("Yahoo Finance returned an empty search result.");
            if (cacheMiss)
                CacheSearchResponse(cacheKey, resData);

            return result;
        }

        private bool TryGetCachedSearchResponse(string symbol, out string response)
        {
            lock (SearchCacheLock)
            {
                RemoveExpiredSearchResponses(DateTimeOffset.UtcNow);

                if (SearchCache.TryGetValue(symbol, out CachedSearch? cachedSearch))
                {
                    SearchCacheOrder.Remove(cachedSearch.OrderNode);
                    SearchCacheOrder.AddLast(cachedSearch.OrderNode);
                    response = cachedSearch.Response;
                    return true;
                }
            }

            response = string.Empty;
            return false;
        }

        private void CacheSearchResponse(string symbol, string response)
        {
            lock (SearchCacheLock)
            {
                var now = DateTimeOffset.UtcNow;
                RemoveExpiredSearchResponses(now);

                RemoveSearchResponse(symbol);

                while (SearchCache.Count >= _searchCacheCapacity)
                    RemoveSearchResponse(SearchCacheOrder.First!.Value);

                var orderNode = SearchCacheOrder.AddLast(symbol);
                SearchCache.Add(
                    symbol,
                    new CachedSearch(response, now.Add(_searchCacheDuration), orderNode));
            }
        }

        private static void RemoveExpiredSearchResponses(DateTimeOffset now)
        {
            while (SearchCacheOrder.First is { } first
                && SearchCache[first.Value].ExpiresAt <= now)
            {
                RemoveSearchResponse(first.Value);
            }
        }

        private static void RemoveSearchResponse(string symbol)
        {
            if (SearchCache.Remove(symbol, out CachedSearch? cachedSearch))
                SearchCacheOrder.Remove(cachedSearch.OrderNode);
        }

        /// <summary>
        /// Get data from Yahoo finance.
        /// </summary>
        /// <param name="symbol">Symbol</param>
        /// <param name="fromDate">From Date</param>
        /// <param name="toDate">To Date</param>
        /// <returns>Root</returns>
        public async Task<yChartResponse> GetYFinData(string symbol, DateTime? fromDate, DateTime? toDate)
        {
            double fromTick = (fromDate.HasValue ? ConvertToUnixTimestamp(fromDate.Value.Date) : ConvertToUnixTimestamp(DateTime.Now)),
                   toTick = (toDate.HasValue ? ConvertToUnixTimestamp(toDate.Value.Date) : ConvertToUnixTimestamp(DateTime.Now));

            yChartResponse result = new yChartResponse();
            
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
            _httpClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36");

            string periodFilter = fromDate.HasValue ? $"&period1={fromTick}" + (toDate.HasValue ? $"&period2={toTick}" : string.Empty) : string.Empty;

            var response = await _httpClient.GetAsync($"https://query2.finance.yahoo.com/v8/finance/chart/{symbol}?interval=1d{periodFilter}");

            response.EnsureSuccessStatusCode();

            var resData = await(response.Content.ReadAsStringAsync());

            result = JsonSerializer.Deserialize<yChartResponse>(resData);
            return result;
        }
    }
}
