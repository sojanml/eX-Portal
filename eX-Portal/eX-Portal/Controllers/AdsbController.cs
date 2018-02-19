﻿using eX_Portal.Models;
using eX_Portal.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eX_Portal.Controllers {
    public class AdsbController : Controller {
        // GET: Adsb
        private String DSN = System.Configuration.ConfigurationManager.ConnectionStrings["ADSB_DB"].ToString();
        public ExponentPortalEntities ctx = new ExponentPortalEntities();
        [OutputCache(Duration = 2)]
        public JsonResult Index(Exponent.ADSB.ADSBQuery QueryData) {
            var ADSB = new Exponent.ADSB.Live();
            var Data = ADSB.FlightStat(DSN, false, QueryData);
            return Json(Data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Summary(int LastProcessedID = 0, Double TimezoneOffset = 0) {
            var ADSB = new Exponent.ADSB.Live();
            var Data = ADSB.GetSummary(DSN, LastProcessedID, 20, TimezoneOffset);
            return Json(Data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Distance(Exponent.ADSB.ADSBQuery QueryData) {
            var ADSB = new Exponent.ADSB.Live();
            var Data = ADSB.GetFlightStatus(DSN, QueryData);
            return Json(Data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Dashboard() {
            Exponent.ADSB.ADSBQuery Params = new Exponent.ADSB.ADSBQuery();
            using (SqlConnection CN = new SqlConnection(DSN)) {
                CN.Open();
                Params.GetDefaults(CN);
                CN.Close();
            }

            return View(Params);
        }

        [HttpGet]
        public ActionResult FullScreen(Exponent.ADSB.ADSBQuery Params) {
            using (SqlConnection CN = new SqlConnection(DSN)) {
                CN.Open();
                Params.GetDefaults(CN);
                CN.Close();
            }

            ViewBag.Organisations = GetOrganisations();
            ViewBag.Pilots = GetPilot(0);
            return View(Params);
        }

        [HttpGet]
        public ActionResult NoFlyZone()
        {

            List<MSTR_NoFlyZone> NoFlyZoneList = ctx.MSTR_NoFlyZone.Where(x => x.DisplayType == "Dynamic" && x.IsDeleted == false).ToList();

            return Json(NoFlyZoneList, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveZone(MSTR_NoFlyZone Zone)
        {
            MSTR_NoFlyZone ezone = new MSTR_NoFlyZone();
            if (Zone.ID != 0)
            {
                ezone = ctx.MSTR_NoFlyZone.Where(x => x.ID == Zone.ID).FirstOrDefault();
                ezone.Name = Zone.Name;
                ezone.Coordinates = Zone.Coordinates;
                ezone.FillColour = "Orange";
                ezone.StartDate = Zone.StartDate;
                ezone.EndDate = Zone.EndDate;
                ezone.StartTime = Zone.StartTime;
                ezone.EndTime = Zone.EndTime;
                ezone.ZoneDescription = Zone.ZoneDescription;
                ezone.DisplayType = "Dynamic";
                ezone.Message = Zone.Message;
                ezone.IsDeleted = false;
                ctx.SaveChanges();
            }
            else
            {
                ezone = Zone;
                ezone.FillColour = "Orange";
                ezone.DisplayType = "Dynamic";
                ezone.IsDeleted = false;

                ctx.MSTR_NoFlyZone.Add(ezone);
                ctx.SaveChanges();
            }
            List<MSTR_NoFlyZone> NoFlyZoneList = ctx.MSTR_NoFlyZone.ToList();

            return Json(ezone.ID, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public JsonResult RemoveZone(MSTR_NoFlyZone Zone)
        {
            MSTR_NoFlyZone ezone = new MSTR_NoFlyZone();
            if (Zone.ID != 0)
            {
                ezone = ctx.MSTR_NoFlyZone.Where(x => x.ID == Zone.ID).FirstOrDefault();
                ezone.IsDeleted = true;
                ctx.SaveChanges();
            }



            return Json(ezone.ID, JsonRequestBehavior.AllowGet);

        }

        public IEnumerable<SelectListItem> GetOrganisations()
        {
            List<SelectListItem> Organisations = new List<SelectListItem>();

            Organisations = ctx.MSTR_Account
                            .Select(x => new SelectListItem
                            {
                                Value = x.AccountId.ToString(),
                                Text = x.Name.ToString()
                            }).ToList();

            Organisations.Insert(0, new SelectListItem() { Value = "0", Text = "All" });


            return Organisations;
        }

        public JsonResult GetPilot(int AccountID)
        {
            List<SelectListItem> Pilots = new List<SelectListItem>();
            //  Pilots.Add(new SelectListItem() { Value = "0", Text = "All" });
            Pilots = ctx.MSTR_User.Where(x => x.AccountId == AccountID)
                                                        .Select(x => new SelectListItem
                                                        {
                                                            Value = x.UserId.ToString(),
                                                            Text = x.FirstName.ToString()
                                                        }).ToList();
            Pilots.Insert(0, new SelectListItem() { Value = "0", Text = "All Pilots" });


            return Json(Pilots, JsonRequestBehavior.AllowGet);
        }

        public List<int?> GetActivePilots()
        {
            DateTime dt = DateTime.Now.AddMinutes(-1);
            List<int?> ActiveFlights = ctx.MSTR_Drone.Where(x => x.FlightTime > dt).Select(x => x.LastFlightID).ToList();


            var ActivePilots = ctx.DroneFlight.Where(x => ActiveFlights.Contains(x.ID)).Select(x => x.PilotID ).ToList();
            return ActivePilots;

        }

        [HttpPost]
        public JsonResult SendMessage(CommsSender Comm)
        {
            if(Comm.OrganizationID>0)
            {
                if(Comm.PilotID>0)
                {
                    MSTR_Comms mcom = new MSTR_Comms();
                    mcom.CreatedBy = 1;
                    mcom.Message = Comm.Message;
                    ctx.MSTR_Comms.Add(mcom);
                    ctx.SaveChanges();

                    CommsDetail cdet = new CommsDetail();
                    cdet.FromID = exLogic.Util.getLoginUserID();
                    cdet.ToID = Comm.PilotID;
                    cdet.MessageID = mcom.MessageID;
                    cdet.Status = "NEW";
                    cdet.StatusUpdatedOn = DateTime.Now;
                    cdet.CreatedBy= exLogic.Util.getLoginUserID();
                    ctx.CommsDetail.Add(cdet);
                    ctx.SaveChanges();


                }
            }
            else if(Comm.OrganizationID==0)
            {
                if (Comm.ActivePilot == true)
                {
                    List<int?> actPilots = GetActivePilots();
                    foreach (int pil in actPilots)
                    {
                        MSTR_Comms mcom = new MSTR_Comms();
                        mcom.CreatedBy = exLogic.Util.getLoginUserID();
                        mcom.Message = Comm.Message;
                        
                        ctx.MSTR_Comms.Add(mcom);
                        ctx.SaveChanges();

                        CommsDetail cdet = new CommsDetail();
                        cdet.FromID = exLogic.Util.getLoginUserID();
                        cdet.ToID = pil;
                        cdet.MessageID = mcom.MessageID;
                        cdet.Status = "NEW";
                        cdet.StatusUpdatedOn = DateTime.Now;
                        cdet.CreatedBy = exLogic.Util.getLoginUserID();
                        ctx.CommsDetail.Add(cdet);
                        ctx.SaveChanges();
                    }
                }

                
                else
                {
                    List<int> mPils = ctx.MSTR_User.Where(x => x.AccountId != 1 && x.IsPilot == true).Select(x => x.UserId).ToList();
                    foreach (int pil in mPils)
                    {
                        MSTR_Comms mcom = new MSTR_Comms();
                        mcom.CreatedBy = exLogic.Util.getLoginUserID();
                        mcom.Message = Comm.Message;
                        ctx.MSTR_Comms.Add(mcom);
                        ctx.SaveChanges();

                        CommsDetail cdet = new CommsDetail();
                        cdet.FromID = exLogic.Util.getLoginUserID();
                        cdet.ToID = pil;
                        cdet.MessageID = mcom.MessageID;
                        cdet.Status = "NEW";
                        cdet.StatusUpdatedOn = DateTime.Now;
                        cdet.CreatedBy = exLogic.Util.getLoginUserID();
                        ctx.CommsDetail.Add(cdet);
                        ctx.SaveChanges();
                    }
                }
               
            }

            return Json("OK");
        }
       

    }
    

}