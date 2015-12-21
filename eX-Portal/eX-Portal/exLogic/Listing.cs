using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eX_Portal.Models;
using eX_Portal.ViewModel;
using eX_Portal.exLogic;

namespace eX_Portal.exLogic
{
    public class Listing
    {

        public static List<Dictionary<String, Object>> getFileNames(int DroneID = 0)
        {

            string SQL = "SELECT DroneID,DocumentType,UploadedDate,DocumentName,\n" +
                 "SUBSTRING(DocumentName, CHARINDEX('~', DocumentName) + 1, \n" +
                 "LEN(DocumentName)) as Name from DroneDocuments where DroneID = " + DroneID +
                 " Order by DocumentType ";


            return Util.getDBRows(SQL);
        }

        public static List<Dictionary<String, Object>>getParts(int DroneID = 0) {
      string SQL = "select\n" +
        "  PartsName,\n" +
        "  Model,\n" +
        "  ISNULL(MSTR_Account.Name, '') as Supplier," +
        "  M2M_DroneParts.Quantity,\n" +
        "  M2M_DroneParts.PartsId as id " +
        "from\n" +
        "  M2M_DroneParts\n" +
        "LEFT JOIN  MSTR_Parts on\n" +
        "  M2M_DroneParts.PartsId = MSTR_Parts.PartsId\n" +
        "LEFT JOIN MSTR_Account On\n" +
        "  MSTR_Account.AccountId = MSTR_Parts.SupplierId\n" +
        "where\n" +
        "  M2M_DroneParts.DroneId =" + DroneID;
      return Util.getDBRows(SQL);
    }


        //this creates list of droneparts  for partial view
        public static List<string>DroneListing(int DroneId)
          {
            string SQL = "select PartsName,Model,ISNULL(MSTR_Account.Name, '') as Supplier, " +
                         "M2M_DroneParts.Quantity, M2M_DroneParts.PartsId as id " +
                         "from M2M_DroneParts LEFT JOIN  MSTR_Parts on " +
                         "M2M_DroneParts.PartsId = MSTR_Parts.PartsId" +
                         " LEFT JOIN MSTR_Account On     MSTR_Account.AccountId = MSTR_Parts.SupplierId" +
                         " where M2M_DroneParts.DroneId ="+ DroneId;
            List<String> theData = new List<String>();
            using (var ctx = new ExponentPortalEntities())
            using (var cmd = ctx.Database.Connection.CreateCommand())
            {
                ctx.Database.Connection.Open();
                cmd.CommandText = SQL;
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        
                        var PartID = reader.GetValue(4).ToString();
                        var Val =
                                "<td>" + reader.GetValue(0).ToString() + "</td>\n" +
                                "<td>" + reader.GetValue(1).ToString() + "</td>\n" +
                                "<td>" + reader.GetValue(2).ToString() + "</td>\n" +
                                // "<td>" + reader.GetValue(3).ToString() + "</td>\n" +
                                "<td><Input type='Text'  name='SelectItemsForParts_" + PartID + "' style='width:40px' value=" + reader.GetValue(3).ToString() + ">" +
                                "<Input type='hidden'  name='SelectItemsForParts' style='width:40px' value=" + PartID + ">" +
                                "</td>" +

                                "<td><a class='delete' href='#'>x</a></td>\n";
                        theData.Add(Val);
                    }
                }
            }


            return (theData);
        }



        public static List<string> ServicePartsListing(int ID,String TypeOfService)
            
        {
            String SQL = "";
            if (TypeOfService == "REF")
            {
                 SQL = "select \n" +
                            "PartsName,\n" +
                            "Model,\n" +
                            "ISNULL(MSTR_Account.Name, '') as Supplier,\n" +
                            "M2M_DroneServiceParts.QtyCount,\n" +
                            " mstr_parts.PartsId as id\n" +
                          " from M2M_DroneServiceParts LEFT JOIN  MSTR_Parts on \n" +
                            "    M2M_DroneServiceParts.PartsId = MSTR_Parts.PartsId \n" +
                            "    LEFT JOIN MSTR_Account On\n " +
                            "   MSTR_Account.AccountId = MSTR_Parts.SupplierId \n" +
                            "    where M2M_DroneServiceParts.ServiceId =" + ID + " and M2M_DroneServiceParts.ServicePartsType = 'REF'";
            }
            else if (TypeOfService =="REP")
            {


                SQL = "select \n" +
                            "PartsName,\n" +
                            "Model,\n" +
                            "ISNULL(MSTR_Account.Name, '') as Supplier,\n" +
                            "M2M_DroneServiceParts.QtyCount,\n" +
                            " mstr_parts.PartsId as id\n" +
                          " from M2M_DroneServiceParts LEFT JOIN  MSTR_Parts on \n" +
                            "    M2M_DroneServiceParts.PartsId = MSTR_Parts.PartsId \n" +
                            "    LEFT JOIN MSTR_Account On\n " +
                            "   MSTR_Account.AccountId = MSTR_Parts.SupplierId \n" +
                            "    where M2M_DroneServiceParts.ServiceId =" + ID + " and M2M_DroneServiceParts.ServicePartsType = 'REP'";
            }

            List<String> theData = new List<String>();
            using (var ctx = new ExponentPortalEntities())
            using (var cmd = ctx.Database.Connection.CreateCommand())
            {
                ctx.Database.Connection.Open();
                cmd.CommandText = SQL;
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var PartID = reader.GetValue(4).ToString();
                        var Val="";
                        if (TypeOfService == "REF")
                        {
                             Val =
                                    "<td>" + reader.GetValue(0).ToString() + "</td>\n" +
                                    "<td>" + reader.GetValue(1).ToString() + "</td>\n" +
                                    "<td>" + reader.GetValue(2).ToString() + "</td>\n" +
                                     // "<td>" + reader.GetValue(3).ToString() + "</td>\n" +
                                     "<td><Input type='Text'  name='SelectItemsForRefurbished_" + PartID + "' style='width:40px' value=" + reader.GetValue(3).ToString() + ">" +
                                    "<Input type='hidden'  name='SelectItemsForRefurbished' style='width:40px' value=" + PartID + ">" +
                                    "</td>" +
                                    "<td><a class='delete' href='#'>x</a></td>\n";
                            theData.Add(Val);
                        }
                        else if(TypeOfService == "REP")
                        {
                             Val =
                               "<td>" + reader.GetValue(0).ToString() + "</td>\n" +
                               "<td>" + reader.GetValue(1).ToString() + "</td>\n" +
                               "<td>" + reader.GetValue(2).ToString() + "</td>\n" +
                               // "<td>" + reader.GetValue(3).ToString() + "</td>\n" +
                               "<td><Input type='Text'  name='SelectItemsForReplaced_" + PartID + "' style='width:40px' value=" + reader.GetValue(3).ToString() + ">" +
                               "<Input type='hidden'  name='SelectItemsForReplaced' style='width:40px' value=" + PartID + ">" +
                               "</td>" +

                               "<td><a class='delete' href='#'>x</a></td>\n";
                            theData.Add(Val);

                        }

                        
                    }
                }
            }

            return theData;

        }




    }
}