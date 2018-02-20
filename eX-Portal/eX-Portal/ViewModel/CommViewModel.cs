using eX_Portal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eX_Portal.ViewModel {
  public class CommViewModel {
    public int FromID { get; set; }
    public int ToID { get; set; }
    public int AccountID { get; set; }
    public int MessageID { get; set; }
    public string Message { get; set; }
    public string Status { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? StatusUpdatedOn { get; set; }
    public string FromUser { get; set; }
    public CommViewModel() {
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

    public void GetPilotMsgs(DateTime? FilterDate, int UserID = 0, int FlightID = 0) {
      if (UserID == 0 && FlightID == 0) {
        return;
      }

      using (ExponentPortalEntities ctx = new ExponentPortalEntities()) {
        List<CommsDetail> MessageList = new List<CommsDetail>();
        var FlightMessage = ctx.CommsDetail.Select(e => e);
        if (FlightID > 0)
          FlightMessage = FlightMessage.Where(x => x.MSTR_Comms.FlightID == FlightID);
        if (UserID > 0)
          FlightMessage = FlightMessage.Where(x => x.FromID == UserID || x.ToID == UserID);
        if (FilterDate != null)
          FlightMessage = FlightMessage.Where(p => p.CreatedOn > FilterDate);

        var AlComs = from f in FlightMessage
                     group f by f.MessageID into cd
                     select new CommViewModel {
                       Message = cd.FirstOrDefault().MSTR_Comms.Message,
                       MessageID = cd.FirstOrDefault().MessageID,
                       ToID = cd.FirstOrDefault().ToID,
                       FromID = cd.FirstOrDefault().FromID,
                       FromUser = cd.FirstOrDefault().MSTR_User.FirstName,
                       Status = cd.FirstOrDefault().Status,
                       CreatedOn = cd.FirstOrDefault().CreatedOn,
                       StatusUpdatedOn = cd.FirstOrDefault().StatusUpdatedOn,
                     };
      
        CommsPilotMsgs = AlComs.ToList();
      }
    }

  }

  public class CommsSender {
    public int OrganizationID { get; set; }
    public int PilotID { get; set; }
    public bool ActivePilot { get; set; }
    public bool ActiveRegionPilot { get; set; }
    public string Message { get; set; }
    public string Zone { get; set; }
    public CommsSender() {
      OrganizationID = 0;
      PilotID = 0;
      ActivePilot = false;
      ActiveRegionPilot = false;
      Message = "";
      Zone = "";
    }
  }
}
