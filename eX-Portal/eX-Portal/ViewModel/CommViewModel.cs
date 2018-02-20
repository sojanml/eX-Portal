using eX_Portal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eX_Portal.ViewModel
{
    public class CommViewModel
    {
        public int FromID  {get;set;}
        public int ToID { get; set; }
        public int AccountID { get; set; }
        public int MessageID { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? StatusUpdatedOn { get; set; }
        public string FromUser { get; set; }
        public CommViewModel()
        {
            FromID = 0;
            ToID = 0;
            AccountID = 0;
            MessageID = 0;
            Message = "";
            Status = "";
            CommsPilotMsgs = new List<CommViewModel>();
            CreatedOn = new DateTime();
            StatusUpdatedOn = new DateTime();
            FromUser = "";
        }

        public List<CommViewModel> CommsPilotMsgs { get; set; }

        public void GetPilotMsgs(int UserID, DateTime? FilterDate,int FlightID)
        {
            using (ExponentPortalEntities ctx = new ExponentPortalEntities())
            {
                List<CommsDetail> MessageList = new List<CommsDetail>();
                var FlightMessage = ctx.CommsDetail.Where(x => x.MSTR_Comms.FlightID == FlightID);
                if (FilterDate != null)

                   
                 MessageList = FlightMessage.GroupBy(p => p.MessageID )
                                .Select(x =>x.Where(s=>(s.Status=="READ" || s.Status=="NEW") ).OrderBy(l=>l.StatusUpdatedOn).FirstOrDefault())
                                .Where(p => (p.FromID == UserID || p.ToID == UserID) && p.CreatedOn > FilterDate)
                                .OrderBy(x=>x.CreatedOn).Take(10).ToList();
                else
                    MessageList = FlightMessage.GroupBy(p => p.MessageID)
                               .Select(x => x.Where(s => s.Status == "READ" || s.Status == "NEW").OrderBy(l => l.StatusUpdatedOn).FirstOrDefault())
                               .Where(p => (p.FromID == UserID || p.ToID == UserID))
                               .OrderBy(x => x.CreatedOn).Take(10).ToList();
               
                foreach (CommsDetail cd in MessageList)
                {
                    CommViewModel cvm = new CommViewModel();

                    cvm.Message = cd.MSTR_Comms.Message;
                    cvm.MessageID = cd.MessageID;
                    cvm.ToID = cd.ToID;
                    cvm.FromID = cd.FromID;
                    if (cvm.FromID == UserID)
                        cvm.FromUser = "Me";
                    else
                        cvm.FromUser = cd.MSTR_User.FirstName;
                    cvm.Status = cd.Status;
                    cvm.CreatedOn = cd.CreatedOn;
                    cvm.StatusUpdatedOn = cd.StatusUpdatedOn;
                    CommsPilotMsgs.Add(cvm);
                    

                }

            }
        }
    }

    public class CommsSender
    {
        public int OrganizationID { get; set; }
        public int PilotID { get; set; }
        public bool ActivePilot { get; set; }
        public bool ActiveRegionPilot { get; set; }
        public string Message { get; set; }
        public string Zone { get; set; }
        public CommsSender()
        {
            OrganizationID = 0;
            PilotID = 0;
            ActivePilot = false;
            ActiveRegionPilot = false;
            Message = "";
            Zone = "";
        }
    }       
}
