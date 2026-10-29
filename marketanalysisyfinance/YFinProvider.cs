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
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
                client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36");


                var response = await client.GetAsync($"https://query1.finance.yahoo.com/v1/finance/search?q={symbol}");

                response.EnsureSuccessStatusCode();

                //GetCookie(response);

                string resData =  await (response.Content.ReadAsStringAsync());

                resData = resData.Replace("quotes", "yItems").Replace("isYahooFinance", "isYFinance");

                return JsonSerializer.Deserialize<YSearchResult>(resData);
            }
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
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
                client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36");

                string periodFilter = fromDate.HasValue ? $"&period1={fromTick}" + (toDate.HasValue ? $"&period2={toTick}" : string.Empty) : string.Empty;

                var response = await client.GetAsync($"https://query2.finance.yahoo.com/v8/finance/chart/{symbol}?interval=1d{periodFilter}");

                response.EnsureSuccessStatusCode();

                var resData = await(response.Content.ReadAsStringAsync());

                result = JsonSerializer.Deserialize<yChartResponse>(resData);
            }
            return result;
        }
    }
}
