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

        public CommViewModel()
        {
            FromID = 0;
            ToID = 0;
            AccountID = 0;
            MessageID = 0;
            Message = "";
            Status = "";
            CommsPilotMsgs = new List<CommViewModel>();
        }

        public List<CommViewModel> CommsPilotMsgs { get; set; }

        public void GetPilotMsgs(int UserID, DateTime? FilterDate)
        {
            using (ExponentPortalEntities ctx = new ExponentPortalEntities())
            {
                List<CommsDetail> MessageList = new List<CommsDetail>();
                if(FilterDate!=null)
                 MessageList = ctx.CommsDetail.GroupBy(p => p.MessageID)
                                .Select(x =>x.Where(s=>s.Status=="READ" || s.Status=="NEW").OrderBy(l=>l.StatusUpdatedOn).FirstOrDefault())
                                .Where(p => (p.FromID == UserID || p.ToID == UserID) && p.CreatedOn>=FilterDate)
                                .OrderBy(x=>x.CreatedOn).Take(10).ToList();
                else
                    MessageList = ctx.CommsDetail.GroupBy(p => p.MessageID)
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
                    cvm.Status = cd.Status;
                    CommsPilotMsgs.Add(cvm);
                    

                }

            }
        }
    }
}