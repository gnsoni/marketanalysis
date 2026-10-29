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
    public class Volume : IndicatorCalculatorBase<VolumeSerie>
    {
        protected override List<Ohlc> OhlcList { get; set; }

        public override VolumeSerie Calculate()
        {
            VolumeSerie volumeSerie = new VolumeSerie();

            foreach (var item in OhlcList)
            {
                volumeSerie.Dates.Add(item.Date);
                volumeSerie.volumes.Add(item.Volume);
            }

            OBV _obv = new OBV();
            _obv.Load(OhlcList);
            volumeSerie.obv = _obv.Calculate().Values;

            VROC _vroc = new VROC(7);
            _vroc.Load(OhlcList);
            volumeSerie.vroc = _vroc.Calculate().Values;

            WMA _vwma = new WMA(7, ColumnType.Volume);
            _vwma.Load(OhlcList);
            volumeSerie.vwma = _vwma.Calculate().Values;

            return volumeSerie;
        }
    }
}
