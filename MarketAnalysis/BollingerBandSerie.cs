using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketAnalysis
{
    public class BollingerBandSerie : IIndicatorSerie
    {
        public List<DateTime?> Dates { get; set; }
        public List<double?> LowerBand { get; set; }
        public List<double?> MidBand { get; set; }
        public List<double?> UpperBand { get; set; }
        public List<double?> BandWidth { get; set; }
        public List<double?> BPercent { get; set; }

        public BollingerBandSerie()
        {
            Dates = new List<DateTime?>();
            LowerBand = new List<double?>();
            MidBand = new List<double?>();
            UpperBand = new List<double?>();
            BandWidth = new List<double?>();
            BPercent = new List<double?>();
        }

        public BollingerBandSerie Take(int Days)
        {
            int fistIndex = Dates.IndexOf(Dates.Where(f => f >= DateTime.Now.AddDays(-Days)).DefaultIfEmpty().Min());

            return new BollingerBandSerie
            {
                Dates = Dates.TakeLast(Dates.Count - fistIndex).ToList(),
                LowerBand = LowerBand.TakeLast(Dates.Count - fistIndex).ToList(),
                MidBand = MidBand.TakeLast(Dates.Count - fistIndex).ToList(),
                UpperBand = UpperBand.TakeLast(Dates.Count - fistIndex).ToList(),
                BandWidth = BandWidth.TakeLast(Dates.Count - fistIndex).ToList(),
                BPercent = BPercent.TakeLast(Dates.Count - fistIndex).ToList(),
            };
        }

    }
}
