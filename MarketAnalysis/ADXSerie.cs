using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketAnalysis
{
    public class ADXSerie : IIndicatorSerie
    {
        public List<DateTime?> Dates { get; set; }
        public List<double?> TrueRange { get; set; }
        public List<double?> DINegative { get; set; }
        public List<double?> DIPositive { get; set; }
        public List<double?> DX { get; set; }
        public List<double?> ADX { get; set; }

        public ADXSerie()
        {
            Dates = new List<DateTime?>();
            TrueRange = new List<double?>();
            DINegative = new List<double?>();
            DIPositive = new List<double?>();
            DX = new List<double?>();
            ADX = new List<double?>();
        }


        public ADXSerie Take(int Days)
        {
            int fistIndex = Dates.IndexOf(Dates.Where(f => f >= DateTime.Now.AddDays(-Days)).DefaultIfEmpty().Min());

            return new ADXSerie
            {
                Dates = Dates.TakeLast(Dates.Count - fistIndex).ToList(),
                TrueRange = TrueRange.TakeLast(Dates.Count - fistIndex).ToList(),
                DIPositive = DIPositive.TakeLast(Dates.Count - fistIndex).ToList(),
                DINegative = DINegative.TakeLast(Dates.Count - fistIndex).ToList(),
                DX = DX.TakeLast(Dates.Count - fistIndex).ToList(),
                ADX = ADX.TakeLast(Dates.Count - fistIndex).ToList(),
            };
        }
    }
}
