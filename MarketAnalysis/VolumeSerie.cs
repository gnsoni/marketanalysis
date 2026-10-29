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
    public class VolumeSerie : IIndicatorSerie
    {
        public List<DateTime?> Dates { get; set; }
        public List<double?> volumes { get; set; }
        public List<double?> obv { get; set; }
        public List<double?> vroc { get; set; }
        public List<double?> vwma { get; set; }

        public VolumeSerie()
        {
        Dates = new List<DateTime?>();
        volumes = new List<double?>();
        obv = new List<double?>();
        vroc = new List<double?>();
        vwma = new List<double?>();
    }

        public VolumeSerie Take(int Days)
        {
            int fistIndex = this.Dates.IndexOf(this.Dates.Where(f => f >= DateTime.Now.AddDays(-Days)).DefaultIfEmpty().Min());

            return new VolumeSerie
            {
                Dates = this.Dates.TakeLast(this.Dates.Count - fistIndex).ToList(),
                volumes = this.volumes.TakeLast(this.Dates.Count - fistIndex).ToList(),
                obv = this.obv.TakeLast(this.Dates.Count - fistIndex).ToList(),
                vroc = this.vroc.TakeLast(this.Dates.Count - fistIndex).ToList(),
                vwma = this.vwma.TakeLast(this.Dates.Count - fistIndex).ToList()
            };
        }

    }
}