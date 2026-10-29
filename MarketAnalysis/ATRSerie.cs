// ----------------------------------------------------------------------------
// Owner   : gnsoni
// Project : MarketAnalysis
// Licence : MIT - see LICENSE in the repository root
// ----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketAnalysis
{
    public class ATRSerie : IIndicatorSerie
    {
        public List<DateTime> Dates { get; set; }
        public List<double?> TrueRange { get; private set; }
        public List<double?> ATR { get; private set; }

        public ATRSerie()
        {
            Dates = new List<DateTime>();
            TrueRange = new List<double?>();
            ATR = new List<double?>();
        }

        public ATRSerie Take(int Days)
        {
            int fistIndex = this.Dates.IndexOf(this.Dates.Where(f => f >= DateTime.Now.AddDays(-Days)).DefaultIfEmpty().Min());

            return new ATRSerie
            {
                TrueRange = TrueRange.TakeLast(this.Dates.Count - fistIndex).ToList(),
                ATR = ATR.TakeLast(this.Dates.Count - fistIndex).ToList()
            };
        }
    }
}
