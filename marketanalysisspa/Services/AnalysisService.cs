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

            // 1. Setup Indicators
            var macdClone = CloneOhlc(sorted);
            var macdCalc = new MACD(fast, slow, signal);
            macdCalc.Load(macdClone);
            var macd = macdCalc.Calculate();

            var bbClone = CloneOhlc(sorted);
            var bbCalc = new BollingerBand(bbPeriod, bbFactor);
            bbCalc.Load(bbClone);
            var bb = bbCalc.Calculate();

            var atrClone = CloneOhlc(sorted);
            var atrCalc = new ATR(atrPeriod);
            atrCalc.Load(atrClone);
            var atr = atrCalc.Calculate();

            var rsiClone = CloneOhlc(sorted);
            var rsiCalc = new RSI(14); // Standard RSI period
            rsiCalc.Load(rsiClone);
            var rsi = rsiCalc.Calculate();

            // 2. Apply Multiple Algorithms (Ensemble)
            
            // Algorithm A: MACD Trend
            var validMacd = macd.MACDLine.Where(v => v.HasValue).Select(v => v!.Value).ToList();
            var validSignal = macd.Signal.Where(v => v.HasValue).Select(v => v!.Value).ToList();
            string macdSignal = "Neutral";
            if (validMacd.Count >= 2 && validSignal.Count >= 2)
            {
                double lastM = validMacd.Last();
                double prevM = validMacd[^2];
                double lastS = validSignal.Last();
                double prevS = validSignal[^2];
                if (prevM <= prevS && lastM > lastS) macdSignal = "Bullish";
                else if (prevM >= prevS && lastM < lastS) macdSignal = "Bearish";
                else if (lastM > lastS) macdSignal = "Bullish";
                else if (lastM < lastS) macdSignal = "Bearish";
            }
            result.AlgorithmDetails.Add(new AlgorithmResult { 
                AlgorithmName = "MACD Trend", 
                Signal = macdSignal, 
                Weight = 1.5, 
                Reasoning = macdSignal == "Bullish" ? "MACD Line above Signal Line" : (macdSignal == "Bearish" ? "MACD Line below Signal Line" : "No clear crossover")
            });

            // Algorithm B: RSI Mean Reversion
            var validRsi = rsi.RSI.Where(v => v.HasValue).Select(v => v!.Value).ToList();
            string rsiSignal = "Neutral";
            if (validRsi.Count > 0)
            {
                double lastRsi = validRsi.Last();
                if (lastRsi < 30) rsiSignal = "Bullish";
                else if (lastRsi > 70) rsiSignal = "Bearish";
            }
            result.AlgorithmDetails.Add(new AlgorithmResult { 
                AlgorithmName = "RSI Reversal", 
                Signal = rsiSignal, 
                Weight = 1.0, 
                Reasoning = rsiSignal == "Bullish" ? "Oversold (RSI < 30)" : (rsiSignal == "Bearish" ? "Overbought (RSI > 70)" : "Neutral range")
            });

            // Algorithm C: Linear Regression Forecast
            double lrSlope = CalculateLinearRegressionSlope(sorted.Select(o => o.Close).ToList());
            string lrSignal = lrSlope > 0 ? "Bullish" : (lrSlope < 0 ? "Bearish" : "Neutral");
            result.AlgorithmDetails.Add(new AlgorithmResult { 
                AlgorithmName = "Linear Regression", 
                Signal = lrSignal, 
                Weight = 1.2, 
                Reasoning = lrSignal == "Bullish" ? "Positive price slope" : (lrSignal == "Bearish" ? "Negative price slope" : "Flat slope")
            });

            // 3. Aggregate Ensemble Result
            double bullishScore = 0;
            double bearishScore = 0;
            double totalWeight = 0;

            foreach (var alg in result.AlgorithmDetails)
            {
                totalWeight += alg.Weight;
                if (alg.Signal == "Bullish") bullishScore += alg.Weight;
                else if (alg.Signal == "Bearish") bearishScore += alg.Weight;
            }

            if (bullishScore > bearishScore && bullishScore > (totalWeight * 0.3))
            {
                result.Trend = "Bullish";
                result.Confidence = Math.Round(bullishScore / totalWeight * 100, 1);
            }
            else if (bearishScore > bullishScore && bearishScore > (totalWeight * 0.3))
            {
                result.Trend = "Bearish";
                result.Confidence = Math.Round(bearishScore / totalWeight * 100, 1);
            }
            else
            {
                result.Trend = "Neutral";
                result.Confidence = 0;
            }

            // 4. Final Targets & Stop Loss (Using ATR and BB)
            double? lastAtrVal = atr.ATR.LastOrDefault(v => v.HasValue && v > 0);
            double currentAtr = lastAtrVal ?? (result.LastPrice * 0.02);
            result.CurrentATR = Math.Round(currentAtr, 2);

            double? lastUpVal = bb.UpperBand.LastOrDefault(v => v.HasValue);
            double? lastMidVal = bb.MidBand.LastOrDefault(v => v.HasValue);
            double? lastLowVal = bb.LowerBand.LastOrDefault(v => v.HasValue);
            result.BollingerUpper = lastUpVal.HasValue ? Math.Round(lastUpVal.Value, 2) : (double?)null;
            result.BollingerMid = lastMidVal.HasValue ? Math.Round(lastMidVal.Value, 2) : (double?)null;
            result.BollingerLower = lastLowVal.HasValue ? Math.Round(lastLowVal.Value, 2) : (double?)null;

            bool isBullish = result.Trend == "Bullish";
            bool isBearish = result.Trend == "Bearish";

            if (isBullish)
            {
                result.TargetPrice = Math.Max(result.BollingerUpper ?? 0, result.LastPrice + 2 * currentAtr);
                result.StopLossPrice = result.LastPrice - 2 * currentAtr;
            }
            else if (isBearish)
            {
                result.TargetPrice = Math.Min(result.BollingerLower ?? double.MaxValue, result.LastPrice - 2 * currentAtr);
                result.StopLossPrice = result.LastPrice + 2 * currentAtr;
            }
            else
            {
                result.TargetPrice = result.BollingerUpper;
                result.StopLossPrice = result.BollingerLower;
            }

            if (result.TargetPrice.HasValue) result.TargetPrice = Math.Round(result.TargetPrice.Value, 2);
            if (result.StopLossPrice.HasValue) result.StopLossPrice = Math.Round(result.StopLossPrice.Value, 2);

            double risk = Math.Abs(result.LastPrice - (result.StopLossPrice ?? 0));
            double reward = Math.Abs((result.TargetPrice ?? 0) - result.LastPrice);
            result.RiskRewardRatio = risk > 0 ? Math.Round(reward / risk, 2) : 1.0;

            // 5. Charting Data
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
                else { trailingStop = bar.Close - 2.0 * a; }
                result.StopLossLine.Add(trailingStop.HasValue ? Math.Round(trailingStop.Value, 2) : (double?)null);
            }

            return result;
        }

        private double CalculateLinearRegressionSlope(List<double> values)
        {
            if (values == null || values.Count < 2) return 0;
            int n = values.Count;
            double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;
            for (int i = 0; i < n; i++)
            {
                sumX += i;
                sumY += values[i];
                sumXY += i * values[i];
                sumX2 += i * i;
            }
            double denominator = (n * sumX2 - sumX * sumX);
            return denominator == 0 ? 0 : (n * sumXY - sumX * sumY) / denominator;
        }
    }

    public class PredictionResult
    {
        public string Trend { get; set; } = "Neutral";
        public double Confidence { get; set; } = 0;
        public double LastPrice { get; set; }
        public double? TargetPrice { get; set; }
        public double? StopLossPrice { get; set; }
        public double? RiskRewardRatio { get; set; }
        public double? CurrentATR { get; set; }

        public List<AlgorithmResult> AlgorithmDetails { get; set; } = new();

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

    public class AlgorithmResult
    {
        public string AlgorithmName { get; set; } = "";
        public string Signal { get; set; } = "Neutral";
        public double Weight { get; set; } = 1.0;
        public string Reasoning { get; set; } = "";
    }
}
