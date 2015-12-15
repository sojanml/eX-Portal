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
    public class ListController : Controller
    {
        // GET: List
        public ActionResult Index()
        {
            return View();
        }





        public PartialViewResult ListNames(ViewModel.ListViewModel list   ,String TypeName)
        {
            ExponentPortalEntities db = new ExponentPortalEntities();
            var result = from r in db.LUP_Drone
                         where r.Type == TypeName
                         select r;

            list.NameList = result;
            list.TypeCopy = TypeName;
            return PartialView("Details", list);
        }

        
        // GET: List/Details/5
        public ActionResult Details(string TypeName)
        {




            String SQL = "select name, Count(*) Over() as _TotalRecords,TypeId as _PKey from LUP_Drone where type='" + TypeName +"'";
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

        // GET: List/Create
        public ActionResult Create()

        {
           // if (!exLogic.User.hasAccess("lOOKUP.CREATE")) return RedirectToAction("NoAccess", "Home");
            var viewModel = new ViewModel.ListViewModel
            {
               Typelist = Util.LUPTypeList()

            };
            return View(viewModel);
       
        }

        // POST: List/Create
        [HttpPost]
        public ActionResult Create(ViewModel.ListViewModel ListView)
        {
            try
            {

                // TODO: Add insert logic here
            
                if (ModelState.IsValid)
                {
                    ExponentPortalEntities db = new ExponentPortalEntities();
                    if (Session["UserId"] == null)
                    {
                        Session["UserId"] = -1;
                    }

                    int ID = ListView.Id;
                    //checking for upadation or new record creation
                    if (ID == 0)
                    {

                        if (!exLogic.User.hasAccess("lOOKUP.CREATE")) return RedirectToAction("NoAccess", "Home");

                        int TypeId = Util.GetTypeId(ListView.TypeCopy);

                        string BinaryCode = Util.DecToBin(TypeId);
                        string SQL = "INSERT INTO LUP_DRONE(Type,Code,TypeId,BinaryCode,Name,CreatedBy,CreatedOn,IsActive)" +
                            "VALUES('" + ListView.TypeCopy + "','" + ListView.Code + "'," + TypeId +
                            ",'" + BinaryCode + "','" + ListView.Name + "',"
                            + Session["UserId"] + ",'" + DateTime.Now.ToString("yyyy-MM-dd") + "' ," + "'True'" + ")";

                        int ListId = Util.InsertSQL(SQL);
                        return RedirectToAction("Create", "List");

                    }
                    else
                    {
                        if (!exLogic.User.hasAccess("lOOKUP.EDIT")) return RedirectToAction("NoAccess", "Home");
                        string SQL = "UPDATE LUP_DRONE SET Type='" + ListView.TypeCopy + "',Code='" + ListView.Code +
                            "',Name='" + ListView.Name
                            + "',ModifiedBy=" + Session["UserId"] + ",ModifiedOn='" + DateTime.Now.ToString("yyyy-MM-dd") + "' where Id="+ID;
                        int ListId = Util.doSQL(SQL);
                        return RedirectToAction("Create", "List");
                    }

                   // return RedirectToAction("Listnames");



                }
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: List/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: List/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: List/Delete/5
        public ActionResult Delete([Bind(Prefix = "ID")]int LupID = 0)
        {

            try
            {
                if (!exLogic.User.hasAccess("lOOKUP.DELETE")) return RedirectToAction("NoAccess", "Home");

                String SQL = "";
                Response.ContentType = "text/json";


                //Delete the list from database if there is no DroneServiceParts are created
                int TypeID= Util.GetTypeIdFromId(LupID);
                SQL = "select COUNT(*) from M2M_DroneServiceParts where PartsId =" + TypeID;

                 if (Util.getDBInt(SQL) != 0)
                    return RedirectToAction("Create", "List");
                //Delete the list from database if there is no DroneParts are created
                SQL = "select COUNT(*) from M2M_DroneParts where PartsId =" + TypeID;
                if (Util.getDBInt(SQL) != 0)
                    return RedirectToAction("Create", "List");
                //Delete the list from database if there is no TypeofService are Created
                SQL = "select COUNT(*) from MSTR_DroneService where TypeOfServiceId =" + TypeID;
                      if (Util.getDBInt(SQL) != 0)
                    return RedirectToAction("Create", "List");

                SQL = "DELETE FROM [LUP_Drone] WHERE Id = " + LupID;
                Util.doSQL(SQL);

                return RedirectToAction("Create", "List");
                // return Util.jsonStat("OK");



            }
            catch
            {
                return RedirectToAction("Create", "List");
            }
            
        }

       
    }
}
