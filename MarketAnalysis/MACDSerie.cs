// ----------------------------------------------------------------------------
// Owner   : gnsoni
// Project : MarketAnalysis
// Licence : MIT - see LICENSE in the repository root
// ----------------------------------------------------------------------------
namespace MarketAnalysis;

public class MACDSerie : IIndicatorSerie
{
    public List<DateTime> Dates { get; set; }
    public List<double?> MACDLine { get; set; }
    public List<double?> MACDHistogram { get; set; }
    public List<double?> Signal { get; set; }

    public MACDSerie()
    {
        Dates = new List<DateTime>();
        MACDLine = new List<double?>();
        MACDHistogram = new List<double?>();
        Signal = new List<double?>();
    }

    public MACDSerie Take(int Days)
    {
        int fistIndex = this.Dates.IndexOf(this.Dates.Where(f => f >= DateTime.Now.AddDays(-Days)).DefaultIfEmpty().Min());

        return new MACDSerie
        {
            Dates = this.Dates.TakeLast(this.Dates.Count - fistIndex).ToList(),
            MACDLine = this.MACDLine.TakeLast(this.Dates.Count - fistIndex).ToList(),
            MACDHistogram = this.MACDHistogram.TakeLast(this.Dates.Count - fistIndex).ToList(),
            Signal = this.Signal.TakeLast(this.Dates.Count - fistIndex).ToList()
        };
    }

}
