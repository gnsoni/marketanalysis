using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MarketAnalysis.YFinance.Entities.Search
{
    public class YItem
    {
        public string exchange { get; set; }
        public string shortname { get; set; }
        public string quoteType { get; set; }
        public string symbol { get; set; }
        public string index { get; set; }
        public double score { get; set; }
        public string typeDisp { get; set; }
        public string longname { get; set; }
        public string exchDisp { get; set; }
        public string sector { get; set; }
        public string sectorDisp { get; set; }
        public string industry { get; set; }
        public string industryDisp { get; set; }
        public bool dispSecIndFlag { get; set; }
        public bool isYFinance { get; set; }
    }

    public class YSearchResult
    {
        public int count { get; set; }

        public List<YItem> yItems { get; set; }
    }
}
