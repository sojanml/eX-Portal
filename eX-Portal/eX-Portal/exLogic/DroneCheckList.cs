using eX_Portal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eX_Portal.exLogic {
  public class DroneCheckList {
    public int CheckListID = 0;
    public String CheckListTitle {get; set;}
    public String CheckListSubTitle { get; set; }

    public List<CheckItem> HeaderCheckItems = new List<CheckItem>();
    public List<CheckItem> CheckItems = new List<CheckItem>();


    System.Web.SessionState.HttpSessionState Session = HttpContext.Current.Session;

    //constructior
    public DroneCheckList(int pCheckListID) {
      CheckListID = pCheckListID;
      using (var ctx = new ExponentPortalEntities())
      using (var cmd = ctx.Database.Connection.CreateCommand()) {
        ctx.Database.Connection.Open();
        cmd.CommandText = "SELECT * FROM [MSTR_DroneCheckList] WHERE ID=" + CheckListID;
        using (var reader = cmd.ExecuteReader()) {
          //For each row
          while (reader.Read()) {
            CheckListTitle = reader["CheckListTitle"].ToString();
            CheckListSubTitle = reader["CheckListSubTitle"].ToString();
          }//while
        }//using reader

        //fill the details of Items in the Checklist
        setSubItems(cmd, CheckListID);

      }//using cmd


    }//Constructor


    private void setSubItems(System.Data.Common.DbCommand cmd, int CheckListID) {
      cmd.CommandText = "SELECT" + "\n" +
        " *" + "\n" +
        "FROM" + "\n" +
        "  [MSTR_DroneCheckListItems]" + "\n" +
        "WHERE" + "\n" +
        "  [DroneCheckList_ID]=" + CheckListID + "\n" +
        "ORDER BY" + "\n" +
        " [SlNo]";

      CheckItems.Clear();

      using (var reader = cmd.ExecuteReader()) {
        //For each row
        while (reader.Read()) {
          CheckItem item = new CheckItem {
            ID = reader.GetInt32(reader.GetOrdinal("ID")),
            SlNo = reader.GetDecimal(reader.GetOrdinal("SlNo")),
            Title = reader["Title"].ToString(),
            FieldType = reader["FieldType"].ToString(),
            Responsibility = reader["Responsibility"].ToString()
          };
          if (reader.GetDecimal(reader.GetOrdinal("SlNo")) < 1) {
            HeaderCheckItems.Add(item);
          } else {
            CheckItems.Add(item);
          }//if          

        }//while
      }//using reader

    }//setSubItems()



    public bool saveCheckList() {
      String SQL = "";
      String UserID = Session["UserID"] == null ? "" : Session["UserID"].ToString();

      //Save the header of checklist
      SQL = 
      "INSERT INTO [DroneCheckList](\n" +
      "   [DroneCheckListID],\n" +
      "   [DroneID],\n" +
      "   [FlightID],\n" +
      "   [CreatedBy]\n" +
      ") VALUES(\n" +
      " "+ CheckListID.ToString() + ",\n" +
      " 0,\n" +
      " 0,\n" +
       " '" + UserID + "'\n" +
      ")";

      var DroneCheckListID = Util.InsertSQL(SQL);
       

      foreach(var item in HeaderCheckItems) {
        item.saveCheckListItem(DroneCheckListID);
      }
      foreach (var item in CheckItems) {
        item.saveCheckListItem(DroneCheckListID);
      }
      return false;


    } 

  }//class

  public class CheckItem {
    private HttpRequest Request = HttpContext.Current.Request;
    private int sID  ;
    
    public Decimal SlNo { get; set; }
    public String Title { get; set; }
    public String Responsibility { get; set; }
    public String FieldType { get; set; }

    public String FieldValue {get; set; }
    public String FieldNote { get; set; }

    public int ID {
      get {        
        return sID;
      }
      set {
        sID = value;
        setValues(sID);
      }
    }

    private void setValues(int FieldID) {
      FieldValue = Request["Field_" + FieldID] != null ? Request["Field_" + FieldID].ToString() : "";
      FieldNote = Request["FieldNote_" + FieldID] != null ? Request["FieldNote_" + FieldID].ToString() : "";
    }//setValues

    //generate form field for the item
    public String DisplayField() {
      String Field = "";
      switch (FieldType.ToLower()) {
        case "textbox":          
          Field = "<input type=\"text\" value=\"" + HttpUtility.HtmlDecode(FieldValue) + "\" name=\"Field_" + ID.ToString() + "\">";
          break;
        case "checkbox":
          String isChecked = FieldValue == "1" ? " checked" : "";
          Field = "<input" + isChecked + " type =\"checkbox\" name=\"Field_" + ID.ToString() + "\">";
          break;
      }
      return Field;
    }//DisplayField()

    public String DisplayNote() {
      String Field = "";
          Field = "<input type=\"text\" value=\"" + HttpUtility.HtmlDecode(FieldNote) + "\" name=\"FieldNote_" + ID.ToString() + "\">";

      return Field;
    }//DisplayField()

    public int saveCheckListItem(int CheckListItemID) {

      String SQL = "INSERT INTO [DroneCheckListItem](\n" +
        "  [DroneCheckListID],\n" +
        "  [FieldValue],\n" +
        "  [FieldNote]\n" +
        ") VALUES(\n" +
        "  " + CheckListItemID.ToString() + ",\n" +
        "  '" + Util.toSQL(FieldValue) + "',\n" +
        "  '" + Util.toSQL(FieldNote) + "'\n" +
        ")";
      Util.doSQL(SQL);
      return 0;
    }

  }//class CheckItem

}//namespace