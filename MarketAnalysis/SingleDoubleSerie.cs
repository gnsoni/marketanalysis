namespace MarketAnalysis;

public class SingleDoubleSerie : IIndicatorSerie
{
    public List<DateTime> Dates {  get; set; }
    public List<double?> Values { get; set; }

    public SingleDoubleSerie()
    {
        Values = new List<double?>();
        Dates = new List<DateTime>();
    }

    public SingleDoubleSerie? Take(int Days)
    {
        int fistIndex = this.Dates.IndexOf(this.Dates.Where(f => f >= DateTime.Now.AddDays(-Days)).DefaultIfEmpty().Min());

        return new SingleDoubleSerie
        {
            Values = this.Values.TakeLast(this.Dates.Count - fistIndex).ToList()
        };
    }

}
