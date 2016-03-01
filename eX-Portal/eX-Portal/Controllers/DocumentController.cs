using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using eX_Portal.Models;
using eX_Portal.exLogic;

namespace eX_Portal.Controllers
{
    public class DocumentController : Controller
    {
        private ExponentPortalEntities ctx = new ExponentPortalEntities();

        
        public ActionResult PowerPlant()
        {
            
            String SQL= "SELECT  DroneDocuments.[DroneID],DroneDocuments.[DocumentType] as Type,DroneDocuments.[UploadedDate] ,DroneDocuments.[DocumentTitle] as Title, MSTR_Drone.DRONENAME as UAS  Count(*) Over() as _TotalRecords,DroneDocuments.[DroneID] as _PKey FROM[DroneDocuments]  Left join MSTR_Drone on  DroneDocuments.DroneID = MSTR_Drone.DroneID where DocumentType='Power Plants Surveillance' ";
            qView nView = new qView(SQL);
            nView.addMenu("Report", Url.Action("Index", "Drone", new { ID = "_PKey" }));
            if (Request.IsAjaxRequest())
            {
                Response.ContentType = "text/javascript";
                return PartialView("qViewData", nView);
            }
            else {
                return View(nView);
            }//if(IsAjaxRequest)

        }

        public ActionResult PowerLine()
        {
            String SQL = "SELECT  DroneDocuments.[DroneID]as UASID,DroneDocuments.[DocumentName]AS Name,DroneDocuments.[UploadedDate],DroneDocuments.[Documenttitle]as Title,MSTR_Drone.DRONENAME as UAS ,  Count(*) Over() as _TotalRecords,DroneDocuments.[DroneID] as _PKey FROM[DroneDocuments]  Left join MSTR_Drone on  DroneDocuments.DroneID = MSTR_Drone.DroneID where DocumentType='Power Line Inspection'";

            qView nView = new qView(SQL);
            nView.addMenu("Report", Url.Action("Index", "Drone", new { ID = "_PKey" }));
            if (Request.IsAjaxRequest())
            {
                Response.ContentType = "text/javascript";
                return PartialView("qViewData", nView);
            }
            else {
                return View(nView);
            }//if(IsAjaxRequest)


           
        }

        public ActionResult SpillDetection()
        {
            String SQL = "SELECT  DroneDocuments.[DroneID],DroneDocuments.[DocumentType] as Type,DroneDocuments.[UploadedDate],DroneDocuments.[DocumentTitle] as Title, MSTR_Drone.DRONENAME as UAS ,  Count(*) Over() as _TotalRecords,DroneDocuments.[DroneID] as _PKey FROM[DroneDocuments]  Left join MSTR_Drone on  DroneDocuments.DroneID = MSTR_Drone.DroneID where DocumentType='Red Tide and Oil Spill Detection' ";
            qView nView = new qView(SQL);
            nView.addMenu("Report", Url.Action("Index", "Drone", new { ID = "_PKey" }));
            if (Request.IsAjaxRequest())
            {
                Response.ContentType = "text/javascript";
                return PartialView("qViewData", nView);
            }
            else {
                return View(nView);
            }//if(IsAjaxRequest)

        }
        public ActionResult WaterSampling()
        {


            String SQL = "SELECT  DroneDocuments.[DroneID] as UASID,DroneDocuments.[DocumentType] as Type,DroneDocuments.[UploadedDate], MSTR_Drone.DRONENAME as UAS,DroneDocuments.[DocumentTitle]as Title ,  Count(*) Over() as _TotalRecords,DroneDocuments.[DroneID] as _PKey FROM[DroneDocuments]  Left join MSTR_Drone on  DroneDocuments.DroneID = MSTR_Drone.DroneID where DocumentType='Water Sampling'  ";
            qView nView = new qView(SQL);
            nView.addMenu("Report", Url.Action("Index", "Drone", new { ID = "_PKey" }));
            if (Request.IsAjaxRequest())
            {
                Response.ContentType = "text/javascript";
                return PartialView("qViewData", nView);
            }
            else {
                return View(nView);
            }//if(IsAjaxRequest)

        }

        public ActionResult Details([Bind(Prefix = "ID")] int DroneID)
        {

            //if (!exLogic.User.hasAccess("DRONE")) return RedirectToAction("NoAccess", "Home");
           // if (!exLogic.User.hasDrone(DroneID)) return RedirectToAction("NoAccess", "Home");

            ViewBag.Title = Util.getDroneName(DroneID);
            ViewBag.DroneID = DroneID;

            return View();
        }

    }
}