using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FileStorageUtils;
using eX_Portal.ViewModel;
using eX_Portal.Models;
using eX_Portal.exLogic;

namespace eX_Portal.Controllers {
  public class S3UploadController : Controller {
    public ExponentPortalEntities db = new ExponentPortalEntities();

    // GET: S3Upload
    public ActionResult Index() {
      var fileStorageProvider = new AmazonS3FileStorageProvider();

      var fileUploadViewModel = new S3Upload (
        fileStorageProvider.PublicKey,
        fileStorageProvider.PrivateKey,
        fileStorageProvider.BucketName,
        Url.Action("complete", "home", null, Request.Url.Scheme)
      );

      fileUploadViewModel.SetPolicy (
        fileStorageProvider.GetPolicyString (
          fileUploadViewModel.FileId, 
          fileUploadViewModel.RedirectUrl
        )
      );


      ViewBag.FormAction = fileUploadViewModel.FormAction;
      ViewBag.FormMethod = fileUploadViewModel.FormMethod;
      ViewBag.FormEnclosureType = fileUploadViewModel.FormEnclosureType;
      ViewBag.AWSAccessKey = fileUploadViewModel.AWSAccessKey;
      ViewBag.Acl = fileUploadViewModel.Acl;
      ViewBag.Base64EncodedPolicy = fileUploadViewModel.Base64EncodedPolicy;
      ViewBag.Signature = fileUploadViewModel.Signature;

      var DroneDoc = new DroneDocument();
      return View(DroneDoc);

    }//ActionResult Index()

        

    public ActionResult Complete() {
      return View();
    }

    [HttpPost]
    public String Upload(DroneDocument Doc) {
      //Doc.DocumentTitle = Doc.DocumentTitle.Trim();
      if(String.IsNullOrWhiteSpace(Doc.DocumentTitle)) {
        Doc.DocumentTitle = toTitle(Doc.S3Url);
      }
      if(String.IsNullOrEmpty(Doc.DocumentName)) {
        Doc.DocumentName = Doc.DocumentTitle;
      }
      String SQL = @"INSERT INTO [DroneDocuments] (
        [DroneID],
        [DocumentType],
        [DocumentName],
        [UploadedDate],
        [UploadedBy],
        [AccountID],
        [DocumentDate],
        [DocumentTitle],
        [DocumentDesc],
        [S3Url]
        ) VALUES (
        '" + Doc.DroneID.ToString() + @"',
        '" + Doc.DocumentType + @"',
        '" + Doc.DocumentName + @"',
        GETDATE(),
        '" + Util.getAccountID() + @"',
        '" + getDroneAccount(Doc.DroneID) + @"',
        '" + Util.toSQLDate(Doc.DocumentDate.ToString()) + @"',
        '" + Doc.DocumentTitle + @"',
        '" + Doc.DocumentDesc + @"',
        '" + Doc.S3Url + @"'
        )
      ";
      Doc.ID = Util.InsertSQL(SQL);
      return Url.Action("Document", "Drone", new { ID = Doc.ID });
    }

    public string getDroneAccount(int DroneID) {
      String SQL = "SELECT AccountID From MSTR_Drone WHERE DroneID=" + DroneID ;
      return Util.getDBVal(SQL);
    }

    private string toTitle(String S3url) {
      var SlashAt = S3url.LastIndexOf('/');
      var LastDot = S3url.LastIndexOf('.');
      var FileOnly = S3url.Substring(SlashAt + 1, LastDot- SlashAt);
      var UKeyEnd = FileOnly.IndexOf('_');
      return FileOnly.Substring(UKeyEnd + 1);
    }

        public ActionResult GCAApproval(int ID=0)

        {
            //to create gcaapproval
            if (!exLogic.User.hasAccess("FLIGHT.GCAAPPROVAL")) return RedirectToAction("NoAccess", "Home");
            var fileStorageProvider = new AmazonS3FileStorageProvider();

            var fileUploadViewModel = new S3Upload(
              fileStorageProvider.PublicKey,
              fileStorageProvider.PrivateKey,
              fileStorageProvider.BucketName,
              Url.Action("complete", "home", null, Request.Url.Scheme)
            );

            fileUploadViewModel.SetPolicy(
              fileStorageProvider.GetPolicyString(
                fileUploadViewModel.FileId,
                fileUploadViewModel.RedirectUrl
              )
            );


            ViewBag.FormAction = fileUploadViewModel.FormAction;
            ViewBag.FormMethod = fileUploadViewModel.FormMethod;
            ViewBag.FormEnclosureType = fileUploadViewModel.FormEnclosureType;
            ViewBag.AWSAccessKey = fileUploadViewModel.AWSAccessKey;
            ViewBag.Acl = fileUploadViewModel.Acl;
            ViewBag.Base64EncodedPolicy = fileUploadViewModel.Base64EncodedPolicy;
            ViewBag.Signature = fileUploadViewModel.Signature;

            var GCAApprovalDoc = new GCA_Approval();
            if (ID != 0)
            {
                GCAApprovalDoc.DroneID = ID;
            }

            if (ID != 0)
            {
                var olist = (from p in db.GCA_Approval where p.ApprovalID == ID select p).ToList();
                if (olist.Count > 0)
                {
                    GCAApprovalDoc.ApprovalID = olist[0].ApprovalID;
                    GCAApprovalDoc.DroneID = olist[0].DroneID;
                    GCAApprovalDoc.ApprovalName = olist[0].ApprovalName;
                    GCAApprovalDoc.Coordinates = olist[0].Coordinates;

                    GCAApprovalDoc.ApprovalDate = olist[0].ApprovalDate == null ? null : olist[0].ApprovalDate;
                    GCAApprovalDoc.StartDate = olist[0].StartDate == null ? null : olist[0].StartDate; 
                    GCAApprovalDoc.EndDate = olist[0].EndDate == null ? null : olist[0].EndDate;

                    GCAApprovalDoc.StartTime = olist[0].StartTime;
                    GCAApprovalDoc.EndTime = olist[0].EndTime;
                    GCAApprovalDoc.MinAltitude = olist[0].MinAltitude;

                    GCAApprovalDoc.MaxAltitude = olist[0].MaxAltitude;
                    GCAApprovalDoc.BoundaryInMeters = olist[0].BoundaryInMeters;
                }
            }
            return View(GCAApprovalDoc);

        }//ActionResult GCAApproval()


        public String DeleteGCAApproval(int? ID = 0)
        {
            //to create gcaapproval            
            //using (ExponentPortalEntities Context = new ExponentPortalEntities())
            //{
            //    GCA_Approval ApprovalDelete = Context.GCA_Approval.Find(ID);
            //    Context.GCA_Approval.Remove(ApprovalDelete);
            //    Context.SaveChanges();
            //}

            String SQL = "";
            Response.ContentType = "text/json";
            if (!exLogic.User.hasAccess("DRONE.DELETE"))
                return Util.jsonStat("ERROR", "Access Denied");
            
            SQL = "DELETE FROM [GCA_Approval] WHERE ApprovalID = " + ID;
            Util.doSQL(SQL);

            return Util.jsonStat("OK");

        }

        [HttpPost]
        public string UploadGCA(GCA_Approval GCA)
        {
            //Doc.DocumentTitle = Doc.DocumentTitle.Trim();
            if (String.IsNullOrWhiteSpace(GCA.ApprovalName))
            {
                GCA.ApprovalName = toTitle(GCA.ApprovalFileUrl);
            }

            string[] Coord = GCA.Coordinates.Split(',');
           string Poly= GCA.Coordinates + ","+Coord[0];

            if (string.IsNullOrEmpty(GCA.BoundaryInMeters.ToString().Trim()))
                GCA.BoundaryInMeters = 0;

            string SQL = "SELECT Count(*) FROM [GCA_Approval] WHERE ApprovalID = " + GCA.ApprovalID;
            if (Util.getDBInt(SQL) != 0 && GCA.ApprovalID != 0)
            {
                //string UpdateRingQuery = "Update [GCA_Approval] set Polygon=Polygon.ReorientObject().MakeValid()  where Polygon.STArea()>999999 and ApprovalID=" + GCA.ApprovalID;
                //int res = Util.doSQL(SQL);
                //UpdateRingQuery = @"Update [GCA_Approval]  set 
                //        InnerBoundary=Polygon.STBuffer(BoundaryInMeters),
                //        InnerBoundaryCoord = Polygon.STBuffer(BoundaryInMeters).ToString() where ApprovalID=" + GCA.ApprovalID;

                string SQLQ = "Update [GCA_Approval]  set"+ 
                         "[ApprovalName] = '"+GCA.ApprovalName+"' "+
                        ",[ApprovalDate] = '" + Util.toSQLDate(Convert.ToDateTime(GCA.ApprovalDate)) + "' " +
                        ",[StartDate] = '" + Util.toSQLDate(Convert.ToDateTime(GCA.StartDate)) + "' " +
                        ",[EndDate] = '" + Util.toSQLDate(Convert.ToDateTime(GCA.EndDate)) + "' " +
                        ",[StartTime]= '" + GCA.StartTime + "' " +
                        ",[EndTime]= '" + GCA.EndTime + "' " +
                        ",[Coordinates]= '" + GCA.Coordinates + "' " +
                        ",[Polygon]= geography::STGeomFromText('POLYGON((" + Poly + @"))',4326).MakeValid()  " +
                        ",DroneID= '" + GCA.DroneID + "' " +
                        ",ApprovalFileUrl= '" + GCA.S3Url + "' " +
                        ",MinAltitude= '" + (GCA.MinAltitude == null ? 0 : GCA.MinAltitude) + "' " +
                        ",MaxAltitude= '" + (GCA.MaxAltitude == null ? 60 : GCA.MaxAltitude) + "' " +
                        ",BoundaryInMeters= '" + (GCA.BoundaryInMeters == null ? 0 : GCA.BoundaryInMeters) + "' " +
                        " WHERE ApprovalID = " + GCA.ApprovalID;
                int res = Util.doSQL(SQLQ);
            }
            else
            {
                SQL = @" insert into [GCA_Approval]
                         ([ApprovalName]
                        ,[ApprovalDate]
                        ,[StartDate]
                        ,[EndDate]
                        ,[StartTime]
                        ,[EndTime]
                        ,[Coordinates]
                        ,[Polygon]
                        ,DroneID
                        ,ApprovalFileUrl
                        ,MinAltitude
                        ,MaxAltitude
                        ,BoundaryInMeters)
                      values
                      ('" + GCA.ApprovalName + @"',
                      '" + Util.toSQLDate(Convert.ToDateTime(GCA.ApprovalDate)) + @"',
                      '" + Util.toSQLDate(Convert.ToDateTime(GCA.StartDate)) + @"',
                      '" + Util.toSQLDate(Convert.ToDateTime(GCA.EndDate)) + @"',
                      '" + GCA.StartTime + @"',
                      '" + GCA.EndTime + @"',
                      '" + GCA.Coordinates + @"',
                      geography::STGeomFromText('POLYGON((" + Poly + @"))',4326).MakeValid(),
                      " + GCA.DroneID + @",
                      '" + GCA.S3Url + @"',
                     " + (GCA.MinAltitude == null ? 0 : GCA.MinAltitude) + @",
                     " + (GCA.MaxAltitude == null ? 60 : GCA.MaxAltitude) + @",                      
                    -" + (GCA.BoundaryInMeters == null ? 0 : GCA.BoundaryInMeters) + @")";


                GCA.ApprovalID = Util.InsertSQL(SQL);

            }

            return "../../Approval/Index";// RedirectToAction("Index","Approval");
        }

    }//class
}//namespace