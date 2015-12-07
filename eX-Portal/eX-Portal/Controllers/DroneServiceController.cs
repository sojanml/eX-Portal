using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eX_Portal.Models;
using eX_Portal.ViewModel;
using eX_Portal.exLogic;

namespace eX_Portal.Controllers
{
    public class DroneServiceController : Controller
    {
        // GET: DroneService
        public ActionResult Index()
        {
            ViewBag.Title = "Drone Service Listing";

            String SQL = " select  ROW_NUMBER() Over (Order by a.ServiceId) As SNo, " +
                " b.DroneName as DroneName,c.Name as ServiceType,a.DateOfService as DateOfService,a.FlightHour" +
                " , a.Description,Count(*) Over() as _TotalRecords ,  a.ServiceId as _PKey " +
                " from [ExponentPortal].[dbo].MSTR_DroneService a left join" +
                "[ExponentPortal].[dbo].MSTR_Drone b on a.DroneId = b.DroneId" +
                " left join [ExponentPortal].[dbo].LUP_Drone c on a.TypeOfServiceId " +
                "= c.TypeId where c.Type = 'ServiceType'";
            qView nView = new qView(SQL);

            nView.addMenu("Detail", Url.Action("Details", new { ID = "_PKey" }));
            nView.addMenu("Edit", Url.Action("Edit", new { ID = "_PKey" }));
            nView.addMenu("Delete", Url.Action("Delete", new { ID = "_PKey" }));
            if (Request.IsAjaxRequest())
            {
                Response.ContentType = "text/javascript";
                return PartialView("qViewData", nView);
            }
            else
            {
                return View(nView);
            }//if(IsAjaxRequest)
        }

        // GET: DroneService/Details/5
        public ActionResult Details(int id)
        {


            String SQL = "select DroneId from MSTR_DroneService where  ServiceId=" + id;

            ViewBag.FlightID = Util.getDBVal(SQL);
            ViewBag.Title = "View Checklist";
            ViewBag.Title = "Drone Service Details";

            SQL = "select b.PartsName,a.ServicePartsType as Info,b.Model as ModelType,a.QtyCount as Qty, Count(*) Over() as _TotalRecords from M2M_DroneServiceParts" +
                " a left join MSTR_Parts b on a.PartsId=b.PartsId where a.ServiceId=" + id +
                " group by a.ServicePartsType,b.PartsName,b.Model,a.QtyCount";


            qView nView = new qView(SQL);
            if (Request.IsAjaxRequest())
            {
                Response.ContentType = "text/javascript";
                return PartialView("qViewData", nView);
            }
            else
            {
                return View(nView);
            }//if(IsAjaxRequest)

        }

        public ActionResult ServicePartsReplaced(int ID = 0)
        {
            String SQL = "select \n" +
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


            List<String> theData = new List<String>();
            using (var ctx = new ExponentPortalEntities())
            using (var cmd = ctx.Database.Connection.CreateCommand())
            {
                ctx.Database.Connection.Open();
                cmd.CommandText = SQL;
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read()) {
                        var PartID = reader.GetValue(4).ToString();
                        var Val =
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

            return PartialView(theData);

        }//



        public ActionResult ServicePartsRefurbished(int ID = 0)
        {
            String SQL = "select \n" +
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
                                 "<td><Input type='Text'  name='SelectItemsForRefurbished_" + PartID + "' style='width:40px' value=" + reader.GetValue(3).ToString() + ">" +
                                "<Input type='hidden'  name='SelectItemsForRefurbished' style='width:40px' value=" + PartID + ">" +
                                "</td>" +
                                "<td><a class='delete' href='#'>x</a></td>\n";
                        theData.Add(Val);
                    }
                }
            }

            return PartialView(theData);

        }//

        public String DroneDetail(int ID = 0)
        {
            String SQL =
            "SELECT \n" +
            "  D.[DroneId] , \n" +
            "  D.[DroneName], \n" +
            "  D.[DroneIdHexa] as DroneHex, \n" +
            "  D.[CommissionDate] , \n" +
            "  D.[DroneSerialNo],  \n" +
            "  O.Name as OwnerName,  \n" +
            "  M.Name as ManufactureName,  \n" +
            "  U.Name as UAVType,  \n" +
            "  Count(*) Over() as _TotalRecords, \n" +
            "  D.[DroneId] as _PKey \n" +
            "FROM \n" +
            "  [ExponentPortal].[dbo].[MSTR_Drone] D \n" +
            "inner join[ExponentPortal].[dbo].LUP_Drone  O on\n" +
            "  OwnerID = O.TypeID and O.Type = 'Owner' " +
            "inner join[ExponentPortal].[dbo].LUP_Drone M on\n" +
            "  ManufactureID = M.TypeID and M.Type='Manufacturer' " +
            "inner join [ExponentPortal].[dbo].LUP_Drone U on\n" +
            "  UAVTypeID = U.TypeID and U.Type= 'UAV Type' \n" +
            "WHERE\n" +
            "  D.DroneId =" + ID.ToString();

            qDetailView theView = new qDetailView(SQL);
            theView.Columns = 3;

            return theView.getTable();
        }//DroneFlightDeta

        // GET: DroneService/Create
        public ActionResult Create()
        {
            var viewModel = new ViewModel.DroneServiceViewModel
            {
                DroneService = new MSTR_DroneService(),

                ServiceType = Util.GetDropDowntLists("ServiceType", "DroneName", "Code", "usp_Portal_DroneServiceType"),
                DroneList = Util.DroneList("usp_Portal_DroneNameList"),
                //   DronePartsList=Util1.DronePartsList("usp_Portal_GetDroneParts")

            };

            return View(viewModel);
        }



        // POST: DroneService/Create
        [HttpPost]
        public ActionResult Create(ViewModel.DroneServiceViewModel DroneServiceView)
        {
            try
            {
                // TODO: Add insert logic here
                if (ModelState.IsValid)
                {

                    MSTR_DroneService DroneService = DroneServiceView.DroneService;

                    if (Session["UserId"] == null)
                    {
                        Session["UserId"] = -1;
                    }
                    string SQL = "INSERT INTO MSTR_DRONESERVICE(Description,CreatedBy,CreatedOn,DroneId,TypeOfServiceId,TypeOfService,DateOfService,FlightHour) VALUES('"
                              + DroneService.Description + "'," + Session["UserId"] + ",'" +
                               DroneService.DateOfService.Value.ToString("yyyy-MM-dd") + "','"
                              + DroneServiceView.DroneID + "'," + DroneService.TypeOfService + ",'" + DroneService.TypeOfService + "','" + DroneService.DateOfService.Value.ToString("yyyy-MM-dd") + "'," + DroneService.FlightHour + "); ";
                    int ID = Util.InsertSQL(SQL);

                    int ServiceId = Util.GetServiceId();
                    if (DroneServiceView.SelectItemsForReplaced != null)
                    {
                        if (DroneServiceView.SelectItemsForReplaced != null)
                        {
                            for (var count = 0; count < DroneServiceView.SelectItemsForReplaced.Count(); count++)
                            {


                                string PartsId = ((string[])DroneServiceView.SelectItemsForReplaced)[count];
                                int Qty = Util.toInt(Request["SelectItemsForReplaced_" + PartsId]);
                                SQL = "Insert into M2M_DroneServiceParts (ServiceId,PartsId,ServicePartsType,QtyCount) values(" + ServiceId + "," + PartsId + ",'REP'," + Qty + ");";



                                ID = Util.InsertSQL(SQL);
                            }
                        }
                    }


                    if (DroneServiceView.SelectItemsForRefurbished != null)
                    {
                        for (var count = 0; count < DroneServiceView.SelectItemsForRefurbished.Count(); count++)
                        { //int PartsId = Int32.Parse((DroneServiceView.SelectItems)[2])

                            string PartsId = ((string[])DroneServiceView.SelectItemsForRefurbished)[count];
                            int Qty = Util.toInt(Request["SelectItemsForRefurbished_" + PartsId]);
                            SQL = "Insert into M2M_DroneServiceParts (ServiceId,PartsId,ServicePartsType,QtyCount) values(" + ServiceId + "," + PartsId + ",'REF'," + Qty + " );";

                            ID = Util.InsertSQL(SQL);
                        }


                    }

                }

                // return RedirectToAction("Index");
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: DroneService/Edit/5
        public ActionResult Edit(int id)
        {
            ExponentPortalEntities db = new ExponentPortalEntities();
            int DroneId = Util.GetDroneIdFromService(id);
            int TypeOfServiceId = Util.GetTypeOfIdFromService(id);
            ViewBag.ServiceId = id;
            var list = (from data in db.M2M_DroneServiceParts
                        where data.ServiceId == 39
                        select new
                        {
                            data.PartsId
                        });
            var viewModel = new ViewModel.DroneServiceViewModel
            {


                DroneService = db.MSTR_DroneService.Find(id),

                ServiceType = Util.GetDropDowntLists("ServiceType", "DroneName", "Code", "usp_Portal_DroneServiceType"),
                DroneList = Util.DroneList("usp_Portal_DroneNameList")
             // SelectItemsForRefurbished = list;
            };
            

           
            return View(viewModel);
       
    }
        // POST: DroneService/Edit/5
        [HttpPost]
        public ActionResult Edit(ViewModel.DroneServiceViewModel DroneServiceView)
        {
            try
            {
                // TODO: Add update logic here
                if (ModelState.IsValid)
                {

                    MSTR_DroneService DroneService = DroneServiceView.DroneService;
                    if (Session["UserId"] == null)
                    {
                        Session["UserId"] = -1;
                    }

                    string SQL = "UPDATE MSTR_DRONESERVICE SET Description='" + DroneService.Description + "',CreatedBy=" + Session["UserId"] + ", CreatedOn='" + DroneService.DateOfService.Value.ToString("yyyy-MM-dd") +
                        "', DroneId=" + DroneService.DroneId + ",TypeOfServiceId='" + DroneService.TypeOfService + "' ,DateOfService='" + DroneService.DateOfService.Value.ToString("yyyy-MM-dd") + "', FlightHour=" + DroneService.FlightHour + " WHERE ServiceId=" + DroneService.ServiceId;

                    int ID = Util.doSQL(SQL);
                    SQL = "delete from M2M_DroneServiceParts where ServiceId =" + DroneService.ServiceId;
                    ID = Util.doSQL(SQL);
                    if (DroneServiceView.SelectItemsForReplaced != null)
                    {
                       
                        for (var count = 0; count < DroneServiceView.SelectItemsForReplaced.Count(); count++)
                        {
                           
                            string PartsId = ((string[])DroneServiceView.SelectItemsForReplaced)[count];
                            int Qty = Util.toInt(Request["SelectItemsForReplaced_" + PartsId]);
                            SQL = "Insert into M2M_DroneServiceParts (ServiceId,PartsId,ServicePartsType,QtyCount) values(" + DroneService.ServiceId + "," + PartsId + ",'REP'," + Qty + ");";
                            ID = Util.InsertSQL(SQL);
                        }
                    }

                    if (DroneServiceView.SelectItemsForRefurbished != null)
                    {
                      
                        for (var count = 0; count < DroneServiceView.SelectItemsForRefurbished.Count(); count++)
                        { 
                            
                            string PartsId = ((string[])DroneServiceView.SelectItemsForRefurbished)[count];
                            int Qty = Util.toInt(Request["SelectItemsForRefurbished_" + PartsId]);
                            SQL = "Insert into M2M_DroneServiceParts (ServiceId,PartsId,ServicePartsType,QtyCount) values(" + DroneService.ServiceId + "," + PartsId + ",'REF'," + Qty + " );";

                            ID = Util.InsertSQL(SQL);
                        }


                    }


                }
                    return RedirectToAction("Index");
            }
            catch(Exception e)
            {
                
                return View();
            }
        }

        // GET: DroneService/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }



        //*drone  call//
        public ExponentPortalEntities ctx=new ExponentPortalEntities();
        public ActionResult FillParts(int DroneId)

        {
            var DroneParts = (from MSTR_Parts in ctx.MSTR_Parts
            select new
            {
                MSTR_Parts.PartsId,
                MSTR_Parts.Model,
                MSTR_Parts.PartsName,
            });



            return Json(DroneParts, JsonRequestBehavior.AllowGet);
        }

        private object Toint(int? supplierId)
        {
            throw new NotImplementedException();
        }


        // POST: DroneService/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
