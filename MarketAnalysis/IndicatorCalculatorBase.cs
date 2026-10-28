namespace MarketPlusAnalysis;

public abstract class IndicatorCalculatorBase<T>
{
    protected abstract List<Ohlc> OhlcList { get; set; }

    public virtual void Load(List<Ohlc> ohlcList)
    {
        OhlcList = ohlcList.DeepClone();
    }

    public abstract T Calculate();
}
