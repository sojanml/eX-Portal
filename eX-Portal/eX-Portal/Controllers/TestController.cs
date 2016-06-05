using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eX_Portal.exLogic;
using eX_Portal.Models;
using System.Text;
using eX_Portal.ViewModel;

namespace eX_Portal.Controllers {
  public class TestController : Controller {
    // GET: Test
    public ActionResult Index(int id = 9) {
      GeoGrid Info = new GeoGrid(id);
      return View(Info);
    }

    public ActionResult GCA_Approval(int id = 9) {
      ExponentPortalEntities DB = new ExponentPortalEntities();
      var Row = (from m in DB.GCA_Approval
                where m.ApprovalID == id
                select m).First();
      return View(Row);
    }

    public ActionResult Video() {
      return View();
    }

    public ActionResult CompareGraph(int From = 0, int To = 0) {
      String SQL =
      @"SELECT BasePoint.RFID,
        IsNull(BasePoint.Latitude,0) AS bLat,
        IsNull(BasePoint.Longitude,0) AS bLon,
        IsNull(ComparePoint.Latitude,0) AS cLat,
        IsNull(ComparePoint.Longitude,0) AS cLon
      FROM PayLoadMapData AS BasePoint
      LEFT JOIN PayLoadMapData AS ComparePoint
        ON ComparePoint.RFID = BasePoint.RFID
          AND ComparePoint.FlightUniqueID = (
            SELECT FlightUniqueID
            FROM PayLoadFlight
            WHERE PayLoadFlightID = " + To + @"
            )
      WHERE BasePoint.FlightUniqueID = (
          SELECT FlightUniqueID
          FROM PayLoadFlight
          WHERE PayLoadFlightID = " + From + @"
          )";
      ViewBag.Json = Util.getDBRowsJson(SQL);
      return View();
    }

    public ActionResult Analysis(String OrgLoc, String CmpLoc) {
      StringBuilder JSon = new StringBuilder();
      String SQL = @"Select
        OrgLoc.Latitude,
        OrgLoc.Longitude,
        OrgLoc.RFID,
        OrgLoc.RSSI,
        OrgLoc.ReadCount
      FROM
        PayLoadMapData  as OrgLoc
      WHERE
        OrgLoc.FlightUniqueID = '" + OrgLoc + "'";

      StringBuilder SB = new StringBuilder();
      StringBuilder sRow = new StringBuilder();
      List<Dictionary<String, Object>> Rows = Util.getDBRows(SQL);
      foreach (var Row in Rows) {
        if (SB.Length > 0) SB.AppendLine(",");
        sRow.Clear();
        foreach (var Key in Row.Keys) {
          if (sRow.Length > 0) sRow.Append(", ");
          sRow.Append("\"");
          sRow.Append(Key);
          sRow.Append("\": ");
          if (!Util.IsNumber(Row[Key])) sRow.Append("\"");
          sRow.Append(Row[Key]);
          if (!Util.IsNumber(Row[Key])) sRow.Append("\"");
        }
        SB.Append("{");
        SB.Append(sRow);
        SB.Append(", \"Reads\": [");
        SB.Append(getReads(Row["RFID"].ToString(), CmpLoc));
        SB.Append("]}");
      }

      ViewBag.Json = SB.ToString();
      return View();




    }

    public ActionResult BlackBox() {
      BlackBox BB = new BlackBox();
      var theList = new List<List<BlackBoxCostCalucation>>();
      theList.Add(BB.getBlackBoxCost(10));
      theList.Add(BB.getBlackBoxCost(15));
      theList.Add(BB.getBlackBoxCost(20));

      return View(theList);
    }

    private String getReads(String RFID, String uKey) {
      String SQL = @"select
        Latitude,
        Longitude,
        RFID,
        RSSI,
        ReadCount
      FROM
        PayLoadData as OrgLocation
      WHERE
        FlightUniqueID = '" + uKey + @"' AND
        RFID='" + RFID + @"'
      ORDER BY
        CreatedTime";
     return Util.getDBRowsJson(SQL);
    }


    public ActionResult dt() {
      return View();
    }
  }

}