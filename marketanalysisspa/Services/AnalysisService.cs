// ----------------------------------------------------------------------------
// Owner   : gnsoni
// Project : MarketAnalysis
// Licence : MIT - see LICENSE in the repository root
// ----------------------------------------------------------------------------
using MarketAnalysis;

namespace MarketAnalysis.Services
{
    /// <summary>
    /// Lightweight local AnalysisService that wraps indicator classes from MarketAnalysis.
    /// Mirrors MarketPlusDAL.Services.AnalysisService without the DAL/EF dependencies.
    /// </summary>
    public class AnalysisService
    {
        private static AnalysisService? _instance;

        private AnalysisService() { }

        public static AnalysisService Instance
        {
            get
            {
                _instance ??= new AnalysisService();
                return _instance;
            }
        }

        public MACDSerie MACD(List<Ohlc> data, int days, int fast = 12, int slow = 26, int signal = 9)
        {
            var macd = new MACD(fast, slow, signal);
            macd.Load(data.OrderBy(o => o.Date).ToList());
            return macd.Calculate().Take(days);
        }

        public AroonSerie Aroon(List<Ohlc> data, int days, int aroonDays = 25)
        {
            var aroon = new Aroon(aroonDays);
            aroon.Load(data.OrderBy(o => o.Date).ToList());
            return aroon.Calculate().Take(days);
        }

        public ADXSerie ADX(List<Ohlc> data, int days, int period)
        {
            var adx = new ADX(period);
            adx.Load(data.OrderBy(o => o.Date).ToList());
            return adx.Calculate().Take(days);
        }

        public BollingerBandSerie BollingerBand(List<Ohlc> data, int days, int period, int factor)
        {
            var bb = new BollingerBand(period, factor);
            bb.Load(data.OrderBy(o => o.Date).ToList());
            return bb.Calculate().Take(days);
        }

        public RSISerie RSI(List<Ohlc> data, int days)
        {
            var rsi = new RSI(7);
            rsi.Load(data.OrderBy(o => o.Date).ToList());
            return rsi.Calculate().Take(days);
        }

        public VolumeSerie Volume(List<Ohlc> data, int days)
        {
            var vol = new Volume();
            vol.Load(data.OrderBy(o => o.Date).ToList());
            return vol.Calculate().Take(days);
        }

        public SingleDoubleSerie SAR(List<Ohlc> data, int days)
        {
            var sar = new SAR();
            sar.Load(data.OrderBy(o => o.Date).ToList());
            return sar.Calculate().Take(days);
        }

        public SingleDoubleSerie EMA(List<Ohlc> data, int days, int period)
        {
            var ema = new EMA(period);
            ema.Load(data.OrderBy(o => o.Date).ToList());
            return ema.Calculate().Take(days);
        }

        public SingleDoubleSerie WPR(List<Ohlc> data, int days, int period = 14)
        {
            var wpr = new WPR(period);
            wpr.Load(data.OrderBy(o => o.Date).ToList());
            return wpr.Calculate();
        }

        public SingleDoubleSerie DPO(List<Ohlc> data, int days, int period = 20)
        {
            var dpo = new DPO(period);
            dpo.Load(data.OrderBy(o => o.Date).ToList());
            return dpo.Calculate();
        }

        public SingleDoubleSerie OBV(List<Ohlc> data, int days)
        {
            var obv = new OBV();
            obv.Load(data.OrderBy(o => o.Date).ToList());
            return obv.Calculate();
        }

        public SingleDoubleSerie PVT(List<Ohlc> data, int days)
        {
            var pvt = new PVT();
            pvt.Load(data.OrderBy(o => o.Date).ToList());
            return pvt.Calculate();
        }

        public SingleDoubleSerie VROC(List<Ohlc> data, int days, int period = 12)
        {
            var vroc = new VROC(period);
            vroc.Load(data.OrderBy(o => o.Date).ToList());
            return vroc.Calculate();
        }

        public SingleDoubleSerie Momentum(List<Ohlc> data, int days, int period = 10)
        {
            var momentum = new Momentum();
            momentum.Load(data.OrderBy(o => o.Date).ToList());
            return momentum.Calculate();
        }
    }
}
