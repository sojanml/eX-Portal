using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eX_Portal.ViewModel
{
    public class Forcast
    {
        public string Date { get; set; }
        public string TempLow { get; set; }
        public string TempHigh { get; set; }
        public string status { get; set; }

        public int Code { get; set; }
        //public  Forcast(string date, string low, string high, string text)
        //{
        //    Date = date;
        //    TempLow = low;
        //    TempHigh = high;
        //    status = text;
            
        //}
    }

    public class ForcastCollection : CollectionBase
    {
        public virtual void Add(Forcast newForecast)
        {
            this.List.Add(newForecast);
        }
        public virtual Forcast this[int Index]
        {
            get
            {
                return (Forcast)this.List[Index];
            }
        }
    }
}