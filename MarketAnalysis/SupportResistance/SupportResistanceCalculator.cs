// ----------------------------------------------------------------------------
// Owner   : gnsoni
// Project : MarketAnalysis
// Licence : MIT - see LICENSE in the repository root
// ----------------------------------------------------------------------------

namespace MarketAnalysis.SupportResistance
{
    public class SupportResistanceIndicator : IndicatorCalculatorBase<PivotPoint>
    {
        public bool debug = false;
        protected override List<Ohlc> OhlcList { get; set ; }
        private Cluster SupportList = new Cluster(true);
        private Cluster ResistanceList = new Cluster(false);


        public override PivotPoint Calculate()
        {
            SingleDoubleSerie line = new SingleDoubleSerie();

            foreach (Ohlc item in OhlcList.OrderBy(o => o.Date))
            {
                line.Dates.Add(item.Date);
                line.Values.Add((double)item.Close);
            }

            double? minValue = line.Values.Min();
            double? maxValue = line.Values.Max();
            double insignificantValue = (maxValue!.Value - minValue!.Value) / 50;
            double? currentValue = line.Values[line.Values.Count - 1];
            PointList TopList = new PointList();
            PointList BottomList = new PointList();
            for (int i = line.Values.Count - 2; i > 0; i--)
            {
                if (line.Values[i] < line.Values[i + 1] && line.Values[i] <= line.Values[i - 1] && line.Values[i] < currentValue)
                    BottomList.Add((double)line.Values[i]!);

                if (line.Values[i] > line.Values[i + 1] && line.Values[i] >= line.Values[i - 1] && line.Values[i] > currentValue)
                    TopList.Add((double)line.Values[i]!);
            }

            foreach (var item in TopList)
            {
                this.ResistanceList.Clustering(item, insignificantValue);
            }

            foreach (var item in BottomList)
            {
                this.SupportList.Clustering(item, insignificantValue);
            }

            List<double> ResistanceNumbers = this.ResistanceList.getBest();
            List<double> SupportNumbers = this.SupportList.getBest();

            PivotPoint pp = new PivotPoint();

            if (ResistanceNumbers.Count > 0) pp.r1 = ResistanceNumbers[0];
            if (ResistanceNumbers.Count > 1) pp.r2 = ResistanceNumbers[1];

            if (SupportNumbers.Count > 0) pp.s1 = SupportNumbers[0];
            if (SupportNumbers.Count > 1) pp.r2 = SupportNumbers[1];

            if (pp.s1 == 0) pp.s1 = currentValue;
            if (pp.r1 == 0) pp.r1 = currentValue;

            return pp;
        }
    }
}
