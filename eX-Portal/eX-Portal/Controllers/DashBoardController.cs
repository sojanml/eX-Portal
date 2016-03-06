using eX_Portal.exLogic;
using eX_Portal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eX_Portal.Controllers {
  public class DashBoardController : Controller {
    // GET: DashBoard
    public ActionResult Default() {
      return View();
    }

    public ActionResult Dewa() {
      String SQL = @"Select 
        Max(DroneDocuments.ID) as LastID,
        DroneDocuments.DocumentType,
        Max(DroneDocuments.UploadedDate) as LastDate
      FROM
        DroneDocuments,
        MSTR_Drone
      WHERE
        MSTR_Drone.DroneID = DroneDocuments.DroneID AND
        MSTR_Drone.AccountID = " + Util.getAccountID() + @" AND
        DroneDocuments.DocumentType IN (
          'Power Line Inspection',
          'Water Sampling',
          'Red Tide and Oil Spill Detection',
          'Power Plants Surveillance',
            'Vehicle Tracking - QR Codes',
            'RFID Inventory Tracking',
'Port Facility Surveillance'
        ) 
      Group BY
        DroneDocuments.DocumentType
      ORDER BY
        LastDate";
      var Rows = Util.getDBRows(SQL);
      return View(Rows);
    }

    public ActionResult SubSection(int DocumentID = 0) {
      var DB = new ExponentPortalEntities();
      var Doc = DB.DroneDocuments.Find(DocumentID);

      return View(Doc);
    }
  }
}