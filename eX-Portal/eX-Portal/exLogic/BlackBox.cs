using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using eX_Portal.Models;
using System.Text;
using eX_Portal.ViewModel;

namespace eX_Portal.exLogic {
  public class BlackBox {


    public List<BlackBoxCostCalucation> getBlackBoxCost(DateTime FromDate, DateTime ToDate) {
      var NumDays = (FromDate - ToDate).TotalDays;
      return getBlackBoxCost((int)NumDays);
    }


    public List<BlackBoxCostCalucation> getBlackBoxCost(int NumOfDays = 0) {
      int ID = 0;
      var TheCost = new List<BlackBoxCostCalucation>();
      using (var ctx = new ExponentPortalEntities()) {
        var BlackBoxItems = (from n in ctx.BlackBoxCosts select n).ToList();
        foreach(var Item in BlackBoxItems) {
          BlackBoxCostCalucation ThisItem = new BlackBoxCostCalucation {
            CostID = ID++,
            RentType = Item.RentType,
            RentAmount = Item.RentAmount,
            RentDays = Item.RentDays,
            isSelected = false
          };
          ThisItem.getItemCost(NumOfDays);
          TheCost.Add(ThisItem);
        }//foreach
      }//using (var ctx = new ExponentPortalEntities())

      //-- now sort the list by cost to get the minimum amount on first
      var SortedList = TheCost.OrderBy(n => n.CalcuatedCost);
            //-- get the First Item, the marked it as selected
            if (SortedList.Count() > 0)
            {
                SortedList.FirstOrDefault().isSelected = true;
                //totalAmt =Convert.ToDecimal(SortedList.FirstOrDefault().CalcuatedCost);
            }
                //Return the list sorted by ID, to get the order from database
                return SortedList.OrderBy(n => n.RentAmount).ToList();
    }



    public int getBBID(String FileName) {
      String FileOnly = Path.GetFileNameWithoutExtension(FileName);
      int BB_ID = 0;
      Int32.TryParse(FileOnly.TrimStart('0'), out BB_ID);
      return BB_ID;
    }

    public int BulkInsert(String FileName) {
      int ImportedRows = 0;
      using (var ctx = new ExponentPortalEntities()) {
        ctx.Database.Connection.Open();
        ImportedRows = doBulkInsert(ctx, FileName);
        File.Delete(FileName);
      }// using database

      return ImportedRows;
    }// BulkInsert()


    private int doBulkInsert(ExponentPortalEntities ctx, String FileName ) {
      String theDir = Path.GetDirectoryName(FileName);
      String Line;
      StringBuilder SQL = new StringBuilder();
      
      //Find the Drone ID to insert
      int DroneID = 0; // getDroneID(ctx);
      int FlightID = 0; // getFlightID(ctx, DroneID, BBFlightID);
      int BBFlightID = getBBID(FileName);

      int LineCount = 0;
      int RowCount = 0;
      int ImportRowsCount = 0;
      using (var reader = System.IO.File.OpenText(FileName)) {
        while ((Line = reader.ReadLine()) != null) {
          if (LineCount >= 1 && Line != "") {
            RowCount = RowCount + 1;

            //parse the line to get Drone ID
            if (ImportRowsCount == 0) { 
              DroneID = getDroneID(ctx, Line);
              FlightID = getFlightID(ctx, DroneID, BBFlightID);
            }

            if (RowCount == 1)  SQL.AppendLine(getBulkInsertHeader());
            if(RowCount > 1)    SQL.Append(",");
            SQL.AppendLine(getBulkInsertRow(Line, DroneID, FlightID, BBFlightID));

            ImportRowsCount++;
            if (RowCount == 50) {
              ctx.Database.ExecuteSqlCommand(SQL.ToString());
              SQL.Clear();
              RowCount = 0;
            } 

          }//if (LineCount >= 1 && Line != "")
          LineCount++;
        }//while
      }//using  reader


      //check is the
      if (RowCount > 0) {
        ctx.Database.ExecuteSqlCommand(SQL.ToString());
      }//if(RowCount == 10)

      return ImportRowsCount;
    }//doBulkInsert()

    private String getBulkInsertHeader() {
      return "INSERT INTO [BlackBoxData](\n" +
        "[DroneId],\n" +              //*
        "[FlightID],\n" +             //*
        "[BBFlightID],\n" +           //*
        "[RecordNumber],\n" +         //*
        "[DroneSerialID],\n" +        //*
        "[Latitude],\n" +             //*
        "[Longitude],\n" +            //*
        "[Altitude],\n" +             //*
        "[Speed],\n" +                //*
        "[FixQuality],\n" +           //*
        "[Satellites],\n" +           //*
        "[ReadTime],\n" +             //*
        "[Pitch],\n" +                //*
        "[Roll],\n" +                 //*
        "[Heading],\n" +              //*
        "[TotalFlightTime]\n" +       //*
        ") VALUES ";
    }//getBulkInsertHeader()
    private String getBulkInsertRow(String Line, int DroneID, int FlightID, int BBFlightID) {
      String[] Data = Line.Split(',');
      return "(\n" +
        "  " + DroneID + ",\n" +                    //[DroneId],\n" +     
        "  " + FlightID + ",\n" +                   //[FlightID],\n" +    
        "  " + BBFlightID + ",\n" +                 //[BBFlightID],\n" +  
        "  " + Util.toInt(Data[0]) + ",\n" +        //[RecordNumber],\n" +
        "  " + Util.toInt(Data[1]) + ",\n" +        //[DroneSerialID],\n" 
        "  " + Util.toDecimal(Data[2]) + ",\n" +    //[Latitude],\n" +    
        "  " + Util.toDecimal(Data[3]) + ",\n" +    //[Longitude],\n" +   
        "  " + Util.toDecimal(Data[4]) + ",\n" +    //[Altitude],\n" +    
        "  " + Util.toDecimal(Data[5]) + ",\n" +    //[Speed],\n" +       
        "  " + Util.toDecimal(Data[6]) + ",\n" +    //[FixQuality],\n" +  
        "  " + Util.toInt(Data[7]) + ",\n" +        //[Satellites],\n" +  
        "  '" + Util.toDate(Data[8]) + "',\n" +      //[ReadTime],\n" +    
        "  " + Util.toDecimal(Data[9]) + ",\n" +    //[Pitch],\n" +       
        "  " + Util.toDecimal(Data[10]) + ",\n" +   //[Roll],\n" +        
        "  " + Util.toDecimal(Data[11]) + ",\n" +   //[Heading],\n" +     
        "  " + Util.toDecimal(Data[12]) + "\n" +    //[TotalFlightTime]\n"
        ")";
    }//getBulkInsertRow()

    private int getDroneID(ExponentPortalEntities ctx, String Line) {
      String SQL;
      int DroneSerial = 0;

      // Parse the line to get ID
      String[] Data = Line.Split(',');
      Int32.TryParse(Data[1], out DroneSerial);

      //Step 1: Find the drone serial from Blackbox data
      /*
      SQL = "SELECT TOP 1 Drone_Serial FROM #BlackBoxData";
      int DroneSerial = Util.getDBInt(SQL, ctx);
      if(DroneSerial == 0) throw new Exception("Can not find the Drone ID to parse data");
      */

      //Step 2: get the drone ID using the serial number
      SQL = "SELECT TOP 1 DroneID From MSTR_Drone WHERE DroneSerialNo=" + DroneSerial;
      int DroneID = Util.getDBInt(SQL, ctx);
      if (DroneID == 0) throw new Exception("Can not find drone id for the serial number " + DroneSerial);

      return DroneID;
    } //getDroneID(()

    private int getFlightID(ExponentPortalEntities ctx, int DroneID, int BBFlightID) {
      String SQL = "SELECT ID FROM DroneFlight WHERE DroneID=" + DroneID + " AND BBFlightID=" + BBFlightID;
      return Util.getDBInt(SQL, ctx);
    }

  }//class BlackBox  
}//namespace