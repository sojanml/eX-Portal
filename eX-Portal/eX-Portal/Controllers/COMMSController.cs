using eX_Portal.Models;
using eX_Portal.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eX_Portal.Controllers
{
    public class COMMSController : Controller
    {
        public ExponentPortalEntities ctx = new ExponentPortalEntities();
        // GET: COMMS
        public ActionResult Index()
        {
            return View();
        }

        // GET: COMMS/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: COMMS/Create
        public ActionResult Create()
        {
            CommViewModel cvm = new CommViewModel();
            cvm.GetPilotMsgs(exLogic.Util.getLoginUserID(),null);


            return View(cvm);
        }

        // POST: COMMS/Create
        [HttpPost]
        public ActionResult Create(CommViewModel cvm)
        {
            try
            {
                // TODO: Add insert logic here
                if(cvm.Message.Trim().Length>0)
                {
                    MSTR_Comms Comms = new MSTR_Comms();
                    Comms.Message = cvm.Message;
                    Comms.CreatedBy = exLogic.Util.getLoginUserID();
                    ctx.MSTR_Comms.Add(Comms);
                    ctx.SaveChanges();

                    List<MSTR_User> UserList = ctx.MSTR_User.Where(x => x.AccountId == 1).ToList();

                    foreach(MSTR_User Us in UserList)
                    { 
                        CommsDetail Cmd = new CommsDetail();
                        Cmd.FromID = exLogic.Util.getLoginUserID();
                        Cmd.ToID = Us.UserId;
                        Cmd.MessageID = Comms.MessageID;
                        Cmd.Status = "NEW";
                        Cmd.CreatedBy= exLogic.Util.getLoginUserID();
                        ctx.CommsDetail.Add(Cmd);

                    }
                    ctx.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            catch(Exception Ex)
            {
                return View();
            }
        }

        [HttpPost]
        public JsonResult CreateMessage(string Message)
        {
            try
            {
                // TODO: Add insert logic here
                if (Message.Trim().Length > 0)
                {
                    MSTR_Comms Comms = new MSTR_Comms();
                    Comms.Message = Message;
                    Comms.CreatedBy = exLogic.Util.getLoginUserID();
                    ctx.MSTR_Comms.Add(Comms);
                    ctx.SaveChanges();

                    List<MSTR_User> UserList = ctx.MSTR_User.Where(x => x.AccountId == 1).ToList();

                    foreach (MSTR_User Us in UserList)
                    {
                        CommsDetail Cmd = new CommsDetail();
                        Cmd.FromID = exLogic.Util.getLoginUserID();
                        Cmd.ToID = Us.UserId;
                        Cmd.MessageID = Comms.MessageID;
                        Cmd.Status = "NEW";
                        Cmd.CreatedBy = exLogic.Util.getLoginUserID();
                        ctx.CommsDetail.Add(Cmd);

                    }
                    ctx.SaveChanges();
                }
                return Json("OK");
            }
            catch (Exception Ex)
            {
                return Json("Unsuccessful");
            }
        }

        // GET: COMMS/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: COMMS/Edit/5
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

        // GET: COMMS/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: COMMS/Delete/5
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


        public JsonResult GetPilotMessages(int PilotID, DateTime? FilterDate )
        {
            CommViewModel cvm = new CommViewModel();
            try { 
            
            cvm.GetPilotMsgs(exLogic.Util.getLoginUserID(), FilterDate);
            return Json(cvm.CommsPilotMsgs, JsonRequestBehavior.AllowGet);
            }
            catch (Exception Ex)
            {
                return Json(cvm.CommsPilotMsgs, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
