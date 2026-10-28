using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketAnalysis
{
    public class LLastDayMACDResult
    {
        public int StockID { get; set; }
        public double? MACDLine { get; set; }
        public double? MACDHistogram { get; set; }
        public double? Signal { get; set; }
        bool? upTrend { get; set; }
        bool? downTrend { get; set; }
        MACDSerie macdserieresult { get; set; }

        public LLastDayMACDResult()
        {

        }

        public LLastDayMACDResult(int StockID, MACDSerie macdserie)
        {
            this.macdserieresult = macdserie.Take(7);
            this.StockID = StockID;
            this.MACDLine = macdserie.MACDLine.Last();
            this.MACDHistogram = macdserie.MACDHistogram.Last();
            this.Signal = macdserie.Signal.Last();
        }

        public bool isUpTrend
        {
            get
            {
                if (!this.upTrend.HasValue)
                {
                    bool tempResult = false;
                    double? lastHistogramVal = 0;
                    for (int icounter = 3; icounter < macdserieresult.Dates.Count; icounter++)
                    {
                        if (lastHistogramVal.HasValue && macdserieresult.MACDHistogram[icounter].HasValue)
                        {
                            if (lastHistogramVal.Value < macdserieresult.MACDHistogram[icounter])
                                tempResult = true;
                            else
                            {
                                tempResult = false;
                                break;
                            }
                        }
                        lastHistogramVal = macdserieresult.MACDHistogram[icounter];
                    }
                    this.upTrend = tempResult;
                }
                return this.upTrend.HasValue ? this.upTrend.Value : false;
            }
        }


        public bool isDownTrend
        {
            get
            {
                if (!this.downTrend.HasValue)
                {
                    bool tempResult = false;
                    double? lastHistogramVal = 0;
                    for (int icounter = 0; icounter < macdserieresult.Dates.Count; icounter++)
                    {
                        if (lastHistogramVal.HasValue && macdserieresult.MACDHistogram[icounter].HasValue)
                        {
                            if (lastHistogramVal.Value > macdserieresult.MACDHistogram[icounter])
                                tempResult = true;
                            else
                            {
                                tempResult = false;
                                break;
                            }
                        }
                        lastHistogramVal = macdserieresult.MACDHistogram[icounter];
                    }
                    this.downTrend = tempResult;
                }
                return this.downTrend.HasValue ? this.downTrend.Value : false;
            }
        }


    }
}
