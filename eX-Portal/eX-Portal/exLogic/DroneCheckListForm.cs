using eX_Portal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eX_Portal.exLogic {
  public class DroneCheckListForm {
    private int pCheckListID = 0;
    public String CheckListTitle { get; set; }
    public String CheckListSubTitle { get; set; }
    public int FlightID { get; set; }
    public int DroneID { get; set; }
    public int DroneCheckListID { get; set; }
    public int ThisCheckListID { get; set; }

    public List<CheckItem> HeaderCheckItems = new List<CheckItem>();
    public List<CheckItem> CheckItems = new List<CheckItem>();


    System.Web.SessionState.HttpSessionState Session = HttpContext.Current.Session;


    //constructior
    public DroneCheckListForm(int CheckListID, int DroneID = 0, int FlightID = 0) {
      this.FlightID = FlightID;
      this.DroneID = DroneID;
      this.CheckListID = CheckListID;
    }
    //constructior
    public DroneCheckListForm(int CheckListID) {
      this.CheckListID = CheckListID;

    }//Constructor

    public int CheckListID {
      get {
        return pCheckListID;
      }
      set {
        pCheckListID = value;
        init();
      }
    }

    private void init() {
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

    }

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
          CheckItem item = new CheckItem(CheckListID, DroneID, FlightID) {
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



    public int saveCheckList() {
      String SQL = "";
      String UserID = Session["UserID"] == null ? "1" : Session["UserID"].ToString();


      //Save the header of checklist
      SQL =
      "INSERT INTO [DroneCheckList](\n" +
      "   [DroneCheckListID],\n" +
      "   [DroneID],\n" +
      "   [FlightID],\n" +
      "   [CreatedBy],\n" +
      "   [CreatedOn]\n" +
      ") VALUES(\n" +
      "  " + CheckListID.ToString() + ",\n" +
      "  0,\n" +
      "  " + FlightID.ToString() + ",\n" +
      "  '" + UserID + "',\n" +
      "  GETDATE()\n" +
      ")";

      var DroneCheckListID = Util.InsertSQL(SQL);


      foreach (var item in HeaderCheckItems) {
        item.saveCheckListItem(DroneCheckListID);
      }
      foreach (var item in CheckItems) {
        item.saveCheckListItem(DroneCheckListID);
      }
      return DroneCheckListID;
    }

    public bool saveValidation() {

      //foreach (var item in HeaderCheckItems) {
      //  item.saveValidationItem();
      //}
      foreach (var item in CheckItems) {
        item.saveValidationItem();
      }
      return true;
    }

    public List<ValidationMap> getValidationMessages(int ThisCheckListID) {
      String SQL = "SELECT\n" +
        "  DroneFlight.DroneID\n" +
        "FROM\n" +
        "  DroneCheckList,\n" +
        "  DroneFlight\n" +
        "WHERE\n" +
        "  DroneFlight.ID = DroneCheckList.FlightID and\n" +
        "  DroneCheckList.ID =" + ThisCheckListID;

      List<ValidationMap> ValidationMessages = new List<ValidationMap>();
      //Load the drone ID to check against 
      String sDroneID = Util.getDBVal(SQL);
      DroneID = Int32.Parse(sDroneID);

      //check the validation agains the saved checklist for drone
      foreach (var item in CheckItems) {
        if (!item.isValidated(ThisCheckListID, DroneID)) {
          ValidationMessages.Add(new ValidationMap {
            ItemTitle = item.Title,
            FieldType = item.FieldType,
            SlNo = item.SlNo
          });
        };
      }

      return ValidationMessages;
    }

  }//class

  public class CheckItem {
    private HttpRequest Request = HttpContext.Current.Request;
    private int sID;

    public Decimal SlNo { get; set; }
    public String Title { get; set; }
    public String Responsibility { get; set; }
    public String FieldType { get; set; }

    public String FieldValue { get; set; }
    public String FieldNote { get; set; }

    public int CheckListID { get; set; }
    public int FlightID { get; set; }
    public int DroneID { get; set; }
    public bool isValid { get; set; }

    public int ID {
      get {
        return sID;
      }
      set {
        sID = value;
        setValues(sID);
      }
    }

    public CheckItem(int CheckListID = 0, int DroneID = 0, int FlightID = 0) {
      this.DroneID = DroneID;
      this.CheckListID = CheckListID;
      this.FlightID = FlightID;
    }



    private void setValues(int FieldID) {
      FieldValue = Request["Field_" + FieldID] != null ? Request["Field_" + FieldID].ToString() : "";
      FieldNote = Request["FieldNote_" + FieldID] != null ? Request["FieldNote_" + FieldID].ToString() : "";
    }//setValues

    public String getValue() {
      if(this.FieldType.ToLower() == "checkbox") {
        if(this.FieldValue.ToString() == "1") {
          return "Passed";
        } else {
          return "Failed";
        }
      } else {
        return this.FieldValue;
      }
    }

    //generate form field for the item
    public String DisplayField() {
      String Field = "";
      switch (FieldType.ToLower()) {
        case "textbox":
          Field = "<input type=\"text\" value=\"" + HttpUtility.HtmlDecode(FieldValue) + "\" name=\"Field_" + ID.ToString() + "\">";
          break;
        case "checkbox":
          String isChecked = FieldValue == "1" ? " checked" : "";
          Field = "<input" + isChecked + " type =\"checkbox\" value=\"1\" name=\"Field_" + ID.ToString() + "\">";
          break;
      }
      return Field;
    }//DisplayField()

    public String DisplayNote() {
      String Field = "";
      Field = "<input type=\"text\" value=\"" + HttpUtility.HtmlDecode(FieldNote) + "\" name=\"FieldNote_" + ID.ToString() + "\">";

      return Field;
    }//DisplayField()

    public int saveCheckListItem(int DroneCheckListID) {
      String SQL = "";


      SQL = "INSERT INTO [DroneCheckListItem](\n" +
        "  [DroneCheckListID],\n" +
        "  [DroneCheckListItemID],\n" +
        "  [FieldValue],\n" +
        "  [FieldNote]\n" +
        ") VALUES(\n" +
        "  " + DroneCheckListID.ToString() + ",\n" +
        "  " + ID.ToString() + ",\n" +
        "  '" + Util.toSQL(FieldValue) + "',\n" +
        "  '" + Util.toSQL(FieldNote) + "'\n" +
        ")";
      Util.doSQL(SQL);
      return 0;
    }


    public String getValidationFieldMin() {
      return getValidationField("Min");

    }
    public String getValidationFieldMax() {
      return getValidationField("Max");
    }

    public Decimal getValidationFieldValue(String Suffix) {
      Decimal dFieldValue = 0;
      String SQL = "SELECT [" + Suffix + "Value] FROM [DroneCheckListValidation] WHERE \n" +
        "  [DroneID]= " + DroneID + " AND\n" +
        "  [DroneCheckListID] = " + CheckListID + " AND\n" +
        "  [DroneCheckListItemID] = " + ID;
      String FieldValue = Util.getDBVal(SQL);
      Decimal.TryParse(FieldValue, out dFieldValue);
      return dFieldValue;
    }

    public String getValidationField(String Sufix) {
      String Field = "";
      Decimal FieldValue = getValidationFieldValue(Sufix);
      switch (FieldType.ToLower()) {
        case "textbox":
          String showFieldValue = FieldValue > 0 ? FieldValue.ToString() : "";
          Field = "<input type=\"text\" value=\"" + showFieldValue + "\" name=\"Field_" + Sufix + "_" + ID.ToString() + "\">";
          break;
        case "checkbox":
          String isChecked = "";
          if (Sufix == "Min") {
            isChecked = FieldValue == 1 ? " checked" : "";
          } else {
            isChecked = FieldValue != 1 ? " checked" : "";
          }
          String Caption = Sufix == "Min" ? "Yes" : "No";
          String CheckValue = Sufix == "Min" ? "1" : "0";
          Field = "<input" + isChecked + " type =\"radio\" value=\"" + CheckValue + "\" name=\"Field_" + ID.ToString() + "\">" + Caption;
          break;
      }
      return Field;
    }


    public bool saveValidationItem() {
      Decimal dMinFieldValue = 0;
      Decimal dMaxFieldValue = 0;

      String MinFieldValue = Util.getQ("Field_Min_" + ID);
      String MaxFieldValue = Util.getQ("Field_Max_" + ID);
      if (FieldType.ToLower() == "checkbox") {
        String FieldValue = Util.getQ("Field_" + ID);
        if (FieldValue == "1") {
          MinFieldValue = "1";
          MaxFieldValue = "1";
        } else {
          MinFieldValue = "0";
          MaxFieldValue = "0";
        }
      }

      Decimal.TryParse(MinFieldValue, out dMinFieldValue);
      Decimal.TryParse(MaxFieldValue, out dMaxFieldValue);
      //Delete from table if already saved
      String SQL = "DELETE FROM [DroneCheckListValidation] WHERE \n" +
        "  [DroneID]= " + DroneID + " AND\n" +
        "  [DroneCheckListID] = " + CheckListID + " AND\n" +
        "  [DroneCheckListItemID] = " + ID;
      Util.doSQL(SQL);

      //Save Mi
      SQL = "INSERT INTO [DroneCheckListValidation](\n" +
        "  [DroneID],\n" +
        "  [DroneCheckListID],\n" +
        "  [DroneCheckListItemID],\n" +
        "  [MinValue],\n" +
        "  [MaxValue],\n" +
        "  [CanBeIgnored],\n" +
        "  [MustBeChecked]\n" +
        ") VALUES (\n" +
        "  " + DroneID + ",\n" +
        "  " + CheckListID + ",\n" +
        "  " + ID + ",\n" +
        "  " + dMinFieldValue.ToString() + ",\n" +
        "  " + dMaxFieldValue.ToString() + ",\n" +
        "  1,\n" +
        "  1\n" +
        ")";
      Util.doSQL(SQL);
      return true;
    }

    public bool isValidated(int ThisCheckListID, int DroneID) {
      String SQL;
      String sFieldValue;
      Decimal FieldValue = 0;
      bool Validated = true;
      SQL = "SELECT FieldValue, FieldNote FROM [DroneCheckListItem] WHERE\n" +
        "  [DroneCheckListID] = " + ThisCheckListID + " AND\n" +
        "  [DroneCheckListItemID] = " + ID;
      var Result = Util.getDBRow(SQL);

      sFieldValue = Result["FieldValue"].ToString();
      this.FieldNote = Result["FieldNote"].ToString();
      this.FieldValue = sFieldValue;
      try {
        FieldValue = Convert.ToDecimal(sFieldValue);
      }  catch(Exception e) {
        //error in parsing data. set to zero
        FieldValue = 0;
      }

      SQL = "SELECT MinValue, MaxValue FROM [DroneCheckListValidation] WHERE \n" +
        "  [DroneID]= " + DroneID + " AND\n" +
        "  [DroneCheckListID] = " + CheckListID + " AND\n" +
        "  [DroneCheckListItemID] = " + ID;

      Result = Util.getDBRow(SQL);
      if ((bool)Result["hasRows"]) {
        Decimal MinValue = (Decimal)Result["MinValue"];
        Decimal MaxValue = (Decimal)Result["MaxValue"];
        if (MinValue > 0 && MaxValue > 0 && FieldValue >= MinValue && FieldValue <= MaxValue) {
          Validated = true;
        } else if (MinValue == 0 && MaxValue == 0) {
          Validated = true;
        } else {
          Validated = false;
        }
      }

      this.isValid = Validated;
      return Validated;

    }//function isValidated

  }//class CheckItem


  public class ValidationMap {
    public String ItemTitle { get; set; }
    public String FieldType { get; set; }
    public Decimal SlNo { get; set; }
    public String getMessage() {
      String Message = "Checkpoint failed";
      if(FieldType.ToLower() == "textbox") {
        return "Value is not in range.";
      }
      return Message;
    }

  }

}//namespace