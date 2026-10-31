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
    /// <summary>
    /// Volume Rate of Change (VROC)
    /// </summary>
    public class VROC : IndicatorCalculatorBase<SingleDoubleSerie>
    {
        protected override List<Ohlc> OhlcList { get; set; }
        protected int Period { get; set; }

        public VROC(int period)
        {
            Period = period;
        }

        /// <summary>
        /// VROC = ((VOLUME (i) - VOLUME (i - n)) / VOLUME (i - n)) * 100
        /// </summary>
        /// <see cref="http://ta.mql4.com/indicators/volumes/rate_of_change"/>
        /// <returns></returns>
        public override SingleDoubleSerie Calculate()
        {
            SingleDoubleSerie rocSerie = new SingleDoubleSerie();

            for (int i = 0; i < OhlcList.Count; i++)
            {
                if (i >= Period)
                {
                    var currentVolume = OhlcList[i].Volume;
                    var priorVolume = OhlcList[i - Period].Volume;
                    var value = priorVolume == 0
                        ? 0d
                        : ((currentVolume - priorVolume) / priorVolume) * 100;

                    rocSerie.Values.Add(value);
                }
                else
                {
                    rocSerie.Values.Add(null);
                }
            }

            return rocSerie;
        }
    }
}

