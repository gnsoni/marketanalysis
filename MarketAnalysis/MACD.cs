// ----------------------------------------------------------------------------
// Owner   : gnsoni
// Project : MarketAnalysis
// Licence : MIT - see LICENSE in the repository root
// ----------------------------------------------------------------------------

namespace MarketAnalysis;

public class MACD : IndicatorCalculatorBase<MACDSerie>
{
    protected override List<Ohlc> OhlcList { get; set; }
    protected int Fast = 12;
    protected int Slow = 26;
    protected int Signal = 9;
    protected bool Percent = false;

    public MACD()
    {

    }

    public MACD(bool percent)
    {
        Percent = percent;
    }

    public MACD(int fast, int slow, int signal)
    {
        Fast = fast;
        Slow = slow;
        Signal = signal;
    }

    public MACD(int fast, int slow, int signal, bool percent)
    {
        Fast = fast;
        Slow = slow;
        Signal = signal;
        Percent = percent;
    }

    public override MACDSerie Calculate()
    {
        MACDSerie macdSerie = new MACDSerie();

        EMA ema = new EMA(Fast, false);
        ema.Load(OhlcList);
        List<double?> fastEmaValues = ema.Calculate().Values;

        ema = new EMA(Slow, false);
        ema.Load(OhlcList);
        List<double?> slowEmaValues = ema.Calculate().Values;

        for (int i = 0; i < OhlcList.Count; i++)
        {
            // MACD Line
            if (fastEmaValues[i].HasValue && slowEmaValues[i].HasValue)
            {
                if (!Percent)
                {
                    macdSerie.MACDLine.Add(fastEmaValues[i].Value - slowEmaValues[i].Value);
                }
                else
                {
                    // macd <- 100 * ( mavg.fast / mavg.slow - 1 )
                    macdSerie.MACDLine.Add(100 * (fastEmaValues[i].Value / slowEmaValues[i].Value - 1));
                }
                OhlcList[i].Close = macdSerie.MACDLine[i].Value;
                macdSerie.Dates.Add(OhlcList[i].Date);
            }
            else
            {
                macdSerie.MACDLine.Add(null);
                macdSerie.Dates.Add(OhlcList[i].Date);
                OhlcList[i].Close = 0.0;
            }
        }

        int zeroCount = macdSerie.MACDLine.Where(x => x == null).Count();
        ema = new EMA(Signal, false);
        ema.Load(OhlcList.Skip(zeroCount).ToList());
        List<double?> signalEmaValues = ema.Calculate().Values;
        for (int i = 0; i < zeroCount; i++)
        {
            signalEmaValues.Insert(0, null);
        }

        // Fill Signal and MACD Histogram lists
        for (int i = 0; i < signalEmaValues.Count; i++)
        {
            macdSerie.Signal.Add(signalEmaValues[i]);

            macdSerie.MACDHistogram.Add(macdSerie.MACDLine[i] - macdSerie.Signal[i]);
        }

        return macdSerie;
    }
}
