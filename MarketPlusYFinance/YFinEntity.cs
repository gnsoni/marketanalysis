namespace MarketAnalysis.YFinance.Entities
{
    #region yahoo finance resonse object.
    public class Chart
    {
        public List<Result> result { get; set; }
        public object error { get; set; }
    }

    public class Indicators
    {
        public List<Quote> quote { get; set; }
    }

    public class Meta
    {
        public string currency { get; set; }
    }

    public class Quote
    {
        public List<double?> close { get; set; }
        public List<double?> volume { get; set; }
        public List<double?> low { get; set; }
        public List<double?> high { get; set; }
        public List<double?> open { get; set; }
    }

    public class Result
    {
        public Meta meta { get; set; }
        public List<double> timestamp { get; set; }
        public Indicators indicators { get; set; }
    }

    public class yChartResponse
    {
        public Chart chart { get; set; }
    }
    #endregion



}