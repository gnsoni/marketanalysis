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
    /// Price Volume Trend (PVT)
    /// </summary>
    public class PVT : IndicatorCalculatorBase<SingleDoubleSerie>
    {
        protected override List<Ohlc> OhlcList { get; set; }

        /// <summary>
        /// PVT = [((CurrentClose - PreviousClose) / PreviousClose) x Volume] + PreviousPVT
        /// </summary>
        /// <see cref="https://www.tradingview.com/stock-charts-support/index.php/Price_Volume_Trend_(PVT)"/>
        /// <returns></returns>
        public override SingleDoubleSerie Calculate()
        {
            SingleDoubleSerie pvtSerie = new SingleDoubleSerie();
            pvtSerie.Values.Add(0d);

            double previousPvt = 0d;
            for (int i = 1; i < OhlcList.Count; i++)
            {
                var previousClose = OhlcList[i - 1].Close;
                var delta = previousClose == 0
                    ? 0d
                    : ((OhlcList[i].Close - previousClose) / previousClose) * OhlcList[i].Volume;

                previousPvt += delta;
                pvtSerie.Values.Add(previousPvt);
            }

            return pvtSerie;
        }
    }
}

