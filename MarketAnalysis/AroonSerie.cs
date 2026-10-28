using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketAnalysis
{
    public class AroonSerie : IIndicatorSerie
    {
        public List<DateTime?> Dates { get; private set; }
        public List<double?> Up { get; private set; }
        public List<double?> Down { get; private set; }

        public AroonSerie()
        {
            Dates = new List<DateTime?>();
            Up = new List<double?>();
            Down = new List<double?>();
        }


        public AroonSerie Take(int Days)
        {
            int fistIndex = this.Dates.IndexOf(this.Dates.Where(f => f >= DateTime.Now.AddDays(-Days)).DefaultIfEmpty().Min());

            return new AroonSerie
            {
                Dates = Dates.TakeLast(this.Dates.Count - fistIndex).ToList(),
                Up = Up.TakeLast(this.Dates.Count - fistIndex).ToList(),
                Down = Down.TakeLast(this.Dates.Count - fistIndex).ToList()
            };
        }

    }
}
