using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eX_Portal.ViewModel
{
    public class Summary
    {
        public Summary sum { get; set; }
        public int UASCount { get; set; }
        public int HighestFlightTime { get; set; }
        public int Last10days { get; set; }
        public int Last30days { get; set; }
        public int TotalFlightTime { get; set; }

        public int TotalFlightcount { get; set; }

        public int TotalFlightHour { get; set; }

        public int ServiceDueDate { get; set; }
        public int ServiceDueinFlightHours { get; set; }
        public int UASAge { get; set; }

        public int Total_Flight_count { get; set; }
        public int Total_Flight_Hour { get; set; }

        public DateTime? Last_Flight_Date { get; set; }
        public int Last_UAS_ID { get; set; }

        public int UserCount { get; set; }
        public int InActive_User { get; set; }

        public int PilotCount { get; set; }
        public int Certificatecount { get; set; }

        public int Service_in_Last_30_days{get;set;}
        public int  Service_Due_in_next_30_Days{get;set;}
        public int CSTotalDues {get;set;}
        public int CSDue_in_30_days{get;set;}
        public int CSDue_done_in_30_days {get;set;}
        public int OSTotalDues { get; set; }
        public int OSDue_in_30_days { get; set; }
        public int OSDue_done_in_30_days { get; set; }
    }

}
