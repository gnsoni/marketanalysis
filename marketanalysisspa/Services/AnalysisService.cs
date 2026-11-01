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

        private static List<Ohlc> CloneOhlc(IEnumerable<Ohlc> data) =>
            data.Select(o => new Ohlc
            {
                Date = o.Date,
                Open = o.Open,
                High = o.High,
                Low = o.Low,
                Close = o.Close,
                Volume = o.Volume,
                Average = o.Average
            }).OrderBy(o => o.Date).ToList();

        public PredictionResult PredictNextTrend(List<Ohlc> data, int days = 120, int fast = 12, int slow = 26, int signal = 9, int bbPeriod = 20, int bbFactor = 2, int atrPeriod = 14)
        {
            var result = new PredictionResult();
            if (data == null || data.Count == 0) return result;

            var sorted = CloneOhlc(data);
            var lastItem = sorted.Last();
            result.LastPrice = Math.Round(lastItem.Close, 2);

            // Clone for MACD calculation (because MACD mutates Close)
            var macdClone = CloneOhlc(sorted);
            var macdCalc = new MACD(fast, slow, signal);
            macdCalc.Load(macdClone);
            var macd = macdCalc.Calculate();

            // Clone for Bollinger Band calculation (because BollingerBand mutates Close)
            var bbClone = CloneOhlc(sorted);
            var bbCalc = new BollingerBand(bbPeriod, bbFactor);
            bbCalc.Load(bbClone);
            var bb = bbCalc.Calculate();

            // Clone for ATR calculation
            var atrClone = CloneOhlc(sorted);
            var atrCalc = new ATR(atrPeriod);
            atrCalc.Load(atrClone);
            var atr = atrCalc.Calculate();

            // Filter to requested days window
            int firstIndex = sorted.FindIndex(d => d.Date >= DateTime.Now.AddDays(-days));
            if (firstIndex < 0) firstIndex = 0;
            var window = sorted.Skip(firstIndex).ToList();

            var macdByDate = macd.Dates.Zip(macd.MACDLine, (d, v) => (d, v)).ToDictionary(x => x.d.Date, x => x.v);
            var signalByDate = macd.Dates.Zip(macd.Signal, (d, v) => (d, v)).ToDictionary(x => x.d.Date, x => x.v);
            var histByDate = macd.Dates.Zip(macd.MACDHistogram, (d, v) => (d, v)).ToDictionary(x => x.d.Date, x => x.v);

            var upperByDate = bb.Dates.Zip(bb.UpperBand, (d, v) => (d, v)).Where(x => x.d.HasValue).ToDictionary(x => x.d!.Value.Date, x => x.v);
            var midByDate = bb.Dates.Zip(bb.MidBand, (d, v) => (d, v)).Where(x => x.d.HasValue).ToDictionary(x => x.d!.Value.Date, x => x.v);
            var lowerByDate = bb.Dates.Zip(bb.LowerBand, (d, v) => (d, v)).Where(x => x.d.HasValue).ToDictionary(x => x.d!.Value.Date, x => x.v);

            var atrByDate = atr.Dates.Zip(atr.ATR, (d, v) => (d, v)).ToDictionary(x => x.d.Date, x => x.v);

            // Determine Trend from MACD
            var validMacd = macd.MACDLine.Where(v => v.HasValue).Select(v => v!.Value).ToList();
            var validSignal = macd.Signal.Where(v => v.HasValue).Select(v => v!.Value).ToList();
            string trend = "Neutral";
            if (validMacd.Count >= 2 && validSignal.Count >= 2)
            {
                double lastM = validMacd.Last();
                double prevM = validMacd[^2];
                double lastS = validSignal.Last();
                double prevS = validSignal[^2];

                bool bullishCrossover = prevM <= prevS && lastM > lastS;
                bool bearishCrossover = prevM >= prevS && lastM < lastS;

                if (bullishCrossover) trend = "Bullish (Strong Reversal)";
                else if (bearishCrossover) trend = "Bearish (Strong Reversal)";
                else if (lastM > lastS && lastM > 0) trend = "Bullish (Up Trend)";
                else if (lastM > lastS) trend = "Bullish (Consolidating Up)";
                else if (lastM < lastS && lastM < 0) trend = "Bearish (Down Trend)";
                else if (lastM < lastS) trend = "Bearish (Consolidating Down)";
            }
            result.Trend = trend;

            // Current Indicator Values
            double? lastAtrVal = atr.ATR.LastOrDefault(v => v.HasValue && v > 0);
            double currentAtr = lastAtrVal ?? (result.LastPrice * 0.02);
            result.CurrentATR = Math.Round(currentAtr, 2);

            double? lastUpVal = bb.UpperBand.LastOrDefault(v => v.HasValue);
            double? lastMidVal = bb.MidBand.LastOrDefault(v => v.HasValue);
            double? lastLowVal = bb.LowerBand.LastOrDefault(v => v.HasValue);

            result.BollingerUpper = lastUpVal.HasValue ? Math.Round(lastUpVal.Value, 2) : (double?)null;
            result.BollingerMid = lastMidVal.HasValue ? Math.Round(lastMidVal.Value, 2) : (double?)null;
            result.BollingerLower = lastLowVal.HasValue ? Math.Round(lastLowVal.Value, 2) : (double?)null;

            // Compute Target Price & Stop Loss
            bool isBullish = trend.Contains("Bullish");
            bool isBearish = trend.Contains("Bearish");

            double targetPrice;
            double stopLossPrice;

            if (isBullish)
            {
                double minTarget = result.LastPrice + 2.0 * currentAtr;
                targetPrice = result.BollingerUpper.HasValue && result.BollingerUpper.Value > result.LastPrice
                    ? Math.Max(result.BollingerUpper.Value, minTarget)
                    : minTarget;

                stopLossPrice = result.LastPrice - 2.0 * currentAtr;
                if (stopLossPrice >= result.LastPrice) stopLossPrice = result.LastPrice * 0.97;
            }
            else if (isBearish)
            {
                double minTarget = result.LastPrice - 2.0 * currentAtr;
                targetPrice = result.BollingerLower.HasValue && result.BollingerLower.Value < result.LastPrice
                    ? Math.Min(result.BollingerLower.Value, minTarget)
                    : Math.Max(0.01, minTarget);

                stopLossPrice = result.LastPrice + 2.0 * currentAtr;
            }
            else // Neutral
            {
                targetPrice = result.BollingerUpper ?? (result.LastPrice + 1.5 * currentAtr);
                stopLossPrice = result.BollingerLower ?? (result.LastPrice - 1.5 * currentAtr);
            }

            result.TargetPrice = Math.Round(targetPrice, 2);
            result.StopLossPrice = Math.Round(stopLossPrice, 2);

            double risk = Math.Abs(result.LastPrice - result.StopLossPrice.Value);
            double reward = Math.Abs(result.TargetPrice.Value - result.LastPrice);
            result.RiskRewardRatio = risk > 0 ? Math.Round(reward / risk, 2) : 1.0;

            // Build aligned time series for charting
            double? trailingStop = null;
            foreach (var bar in window)
            {
                result.Dates.Add(bar.Date.ToString("yyyy-MM-dd"));
                result.Candles.Add(new double[] { bar.Open, bar.Close, bar.Low, bar.High });

                upperByDate.TryGetValue(bar.Date.Date, out var u);
                midByDate.TryGetValue(bar.Date.Date, out var m);
                lowerByDate.TryGetValue(bar.Date.Date, out var l);

                result.BollingerUpperBand.Add(u.HasValue ? Math.Round(u.Value, 2) : (double?)null);
                result.BollingerMidBand.Add(m.HasValue ? Math.Round(m.Value, 2) : (double?)null);
                result.BollingerLowerBand.Add(l.HasValue ? Math.Round(l.Value, 2) : (double?)null);

                macdByDate.TryGetValue(bar.Date.Date, out var ml);
                signalByDate.TryGetValue(bar.Date.Date, out var ms);
                histByDate.TryGetValue(bar.Date.Date, out var mh);

                result.MacdLine.Add(ml.HasValue ? Math.Round(ml.Value, 4) : (double?)null);
                result.MacdSignal.Add(ms.HasValue ? Math.Round(ms.Value, 4) : (double?)null);
                result.MacdHistogram.Add(mh.HasValue ? Math.Round(mh.Value, 4) : (double?)null);

                // Calculate Chandelier / ATR Trailing Stop Line
                atrByDate.TryGetValue(bar.Date.Date, out var barAtr);
                double a = barAtr ?? currentAtr;
                if (isBullish)
                {
                    double currentFloor = bar.Close - 2.0 * a;
                    trailingStop = trailingStop.HasValue ? Math.Max(trailingStop.Value, currentFloor) : currentFloor;
                }
                else if (isBearish)
                {
                    double currentCeiling = bar.Close + 2.0 * a;
                    trailingStop = trailingStop.HasValue ? Math.Min(trailingStop.Value, currentCeiling) : currentCeiling;
                }
                else
                {
                    trailingStop = bar.Close - 2.0 * a;
                }
                result.StopLossLine.Add(trailingStop.HasValue ? Math.Round(trailingStop.Value, 2) : (double?)null);
            }

            return result;
        }
    }

    public class PredictionResult
    {
        public string Trend { get; set; } = "Neutral";
        public double LastPrice { get; set; }
        public double? TargetPrice { get; set; }
        public double? StopLossPrice { get; set; }
        public double? RiskRewardRatio { get; set; }
        public double? CurrentATR { get; set; }

        public double? BollingerUpper { get; set; }
        public double? BollingerMid { get; set; }
        public double? BollingerLower { get; set; }

        public List<string> Dates { get; set; } = new();
        public List<double[]> Candles { get; set; } = new();
        public List<double?> BollingerUpperBand { get; set; } = new();
        public List<double?> BollingerMidBand { get; set; } = new();
        public List<double?> BollingerLowerBand { get; set; } = new();
        public List<double?> StopLossLine { get; set; } = new();

        public List<double?> MacdLine { get; set; } = new();
        public List<double?> MacdSignal { get; set; } = new();
        public List<double?> MacdHistogram { get; set; } = new();
    }
}
