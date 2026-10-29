// ----------------------------------------------------------------------------
// Owner   : gnsoni
// Project : MarketAnalysis
// Licence : MIT - see LICENSE in the repository root
// ----------------------------------------------------------------------------
using System.Text;

namespace MarketAnalysis.SupportResistance
{
    class Cluster : List<PointList>
    {
        private bool isDown = false;

        public Cluster(bool isDown)
        {
            this.isDown = isDown;
        }

        public void Add(Point point)
        {
            PointList list = new PointList(isDown);
            list.Add(point);
            Add(list);
        }

        public void Clustering(Point point, double insignificantValue)
        {
            bool isAdded = false;
            for (int i = 0; i < Count; i++)
            {
                PointList item = this[i];
                isAdded = item.Clustering(point, insignificantValue);
            }
            if (!isAdded)
            {
                Add(point);
            }
        }

        public List<double> getBest()
        {
            List<PointList> suggested = this.OrderByDescending(x => x.Count).ToList();
            List<double> result = new List<double>();
            if (suggested.Count > 0)
                result.Add(suggested[0].Result);
            if (suggested.Count > 1 && suggested[0].Count - 1 <= suggested[1].Count && suggested[1].Count > 1)
                result.Add(suggested[1].Result);

            if (isDown)
                return result.OrderByDescending(x => x).ToList();
            else
                return result.OrderBy(x => x).ToList();
        }
    }
}
