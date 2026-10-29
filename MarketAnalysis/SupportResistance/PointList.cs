// ----------------------------------------------------------------------------
// Owner   : gnsoni
// Project : MarketAnalysis
// Licence : MIT - see LICENSE in the repository root
// ----------------------------------------------------------------------------
using System.Text;

namespace MarketAnalysis.SupportResistance
{
    class PointList : List<Point>
    {
        private bool isDown = false;
        public PointList()
        {
        }

        public PointList(bool isDown)
        {
            this.isDown = isDown;
        }

        public void Add(double value)
        {
            Add(new Point(value));
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            foreach (var item in this)
            {
                result.Append(item.value);
                result.Append(" ");
            }
            return result.ToString();
        }

        public bool Clustering(Point point, double insignificantValue)
        {
            double avg = this.Average(item => item.value);
            if (Math.Abs(avg - point.value) <= insignificantValue)
            {
                Add(point);
                return true;
            }
            return false;
        }

        public double Result
        {
            get
            {
                if (isDown)
                    return this.Min(x => x.value);
                else
                    return this.Max(x => x.value);
            }
        }

    }
}
