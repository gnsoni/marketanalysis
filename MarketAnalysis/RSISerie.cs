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
    public class RSISerie : IIndicatorSerie
    {
        public List<DateTime?> Dates { get; set; }
        public List<double?> RSI { get; set; }
        public List<double?> RS { get; set; }

        public RSISerie()
        {
            Dates = new List<DateTime?>();
            RSI = new List<double?>();
            RS = new List<double?>();
        }

        public RSISerie Take(int Days)
        {
            int fistIndex = this.Dates.IndexOf(this.Dates.Where(f => f >= DateTime.Now.AddDays(-Days)).DefaultIfEmpty().Min());

            return new RSISerie
            {
                Dates = this.Dates.TakeLast(this.Dates.Count - fistIndex).ToList(),
                RSI = this.RSI.TakeLast(this.Dates.Count - fistIndex).ToList(),
                RS = this.RS.TakeLast(this.Dates.Count - fistIndex).ToList()
            };
        }

    }
}
