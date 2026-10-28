using MarketAnalysis;
using MarketAnalysis.Services;
using MarketAnalysis.YFinance;
using MarketAnalysis.YFinance.Entities;
using MarketAnalysis.YFinance.Entities.Search;
using Microsoft.AspNetCore.Mvc;

namespace MarketAnalysis.Controllers
{
    [Route("[controller]/[action]")]
    public class StockController : Controller
    {
        private readonly YFinProvider _provider = new YFinProvider();

        private async Task<List<Ohlc>> YFDailyData(string symbol, int days)
        {
            var response = await _provider.GetYFinData(
                symbol,
                DateTime.Now.Date.AddDays(-days),
                DateTime.Now.Date.AddDays(1));

            var result = new List<Ohlc>();

            if (response?.chart?.result == null || response.chart.result.Count == 0)
                return result;

            var chartResult = response.chart.result[0];
            var timestamps = chartResult.timestamp;
            var quote = chartResult.indicators?.quote?[0];

            if (timestamps == null || quote == null)
                return result;

            for (int i = 0; i < timestamps.Count; i++)
            {
                double open   = quote.open?[i]   ?? 0;
                double high   = quote.high?[i]   ?? 0;
                double low    = quote.low?[i]    ?? 0;
                double close  = quote.close?[i]  ?? 0;
                double volume = quote.volume?[i] ?? 0;

                if (open == 0 && close == 0 && volume == 0)
                    continue;

                result.Add(new Ohlc
                {
                    Date    = _provider.ConvertFromUnixTimestamp(timestamps[i]),
                    Open    = open,
                    High    = high,
                    Low     = low,
                    Close   = close,
                    Volume  = volume,
                    Average = (open + high + low + close) / 4
                });
            }

            return result;
        }

        [HttpGet]
        public async Task<IActionResult> Search(string symbol)
        {
            var result = await _provider.Search(symbol);
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> DailyData(string symbol, int days = 365)
        {
            var list = await YFDailyData(symbol, days);
            return Json(list);
        }

        [HttpGet]
        public async Task<IActionResult> MACD(string symbol, int days = 120)
        {
            var data = await YFDailyData(symbol, days);
            var macd = AnalysisService.Instance.MACD(data, days);

            // Build a date→value lookup so JS can align indicator values to OHLC dates
            var macdByDate    = macd.Dates.Zip(macd.MACDLine,      (d, v) => (d, v)).ToDictionary(x => x.d.Date, x => x.v);
            var signalByDate  = macd.Dates.Zip(macd.Signal,        (d, v) => (d, v)).ToDictionary(x => x.d.Date, x => x.v);
            var histByDate    = macd.Dates.Zip(macd.MACDHistogram, (d, v) => (d, v)).ToDictionary(x => x.d.Date, x => x.v);

            // Return OHLC dates + three indicator series all aligned to the same index
            var dates      = data.Select(d => d.Date.ToString("dd/MM/yy")).ToList();
            var macdLine   = data.Select(d => macdByDate.TryGetValue(d.Date.Date,   out var v) ? v : (double?)null).ToList();
            var signal     = data.Select(d => signalByDate.TryGetValue(d.Date.Date, out var v) ? v : (double?)null).ToList();
            var histogram  = data.Select(d => histByDate.TryGetValue(d.Date.Date,   out var v) ? v : (double?)null).ToList();

            return Json(new { dates, macdLine, signal, histogram });
        }

        [HttpGet]
        public async Task<IActionResult> BollingerBands(string symbol, int days = 120, int bbDays = 20, int factor = 2)
        {
            var data = await YFDailyData(symbol, days);
            var result = AnalysisService.Instance.BollingerBand(data, days, bbDays, factor);
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> RSI(string symbol, int days = 120)
        {
            var data = await YFDailyData(symbol, days);
            var result = AnalysisService.Instance.RSI(data, days);
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> ADX(string symbol, int days = 120, int adxDays = 14)
        {
            var data = await YFDailyData(symbol, days);
            var result = AnalysisService.Instance.ADX(data, days, adxDays);
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> Aroon(string symbol, int days = 120, int aroonDays = 25)
        {
            var data = await YFDailyData(symbol, days);
            var result = AnalysisService.Instance.Aroon(data, days, aroonDays);
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> SAR(string symbol, int days = 120)
        {
            var data = await YFDailyData(symbol, days);
            var result = AnalysisService.Instance.SAR(data, days);
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> EMA(string symbol, int days = 120, int period = 20)
        {
            var data = await YFDailyData(symbol, days);
            var result = AnalysisService.Instance.EMA(data, days, period);
            return Json(result);
        }
    }
}
