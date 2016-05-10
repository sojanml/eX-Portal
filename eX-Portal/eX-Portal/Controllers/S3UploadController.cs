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

        public ActionResult GCAApproval()
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
            return View(GCAApprovalDoc);

        }//ActionResult GCAApproval()


        [HttpPost]
        public String UploadGCA(GCA_Approval GCA)
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

           


            string SQL = @" insert into [GCA_Approval]
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
                      ('" + GCA.ApprovalName+@"',
                      '"+GCA.ApprovalDate+@"',
                      '"+GCA.StartDate+@"',
                      '"+GCA.EndDate+ @"',
                      '"+GCA.StartTime+ @"',
                      '"+GCA.EndTime+ @"',
                      '" + GCA.Coordinates + @"',
                      geography::STGeomFromText('POLYGON(("+ Poly+@"))',4326).MakeValid(),
                      "+GCA.DroneID+@",
                      '"+GCA.S3Url+ @"',
                     " + GCA.MinAltitude + @",
                     " + GCA.MaxAltitude + @",                      
                    -" + GCA.BoundaryInMeters+@")";

            GCA.ApprovalID = Util.InsertSQL(SQL);

     /**   string UpdateRingQuery = "Update [GCA_Approval] set Polygon=Polygon.ReorientObject().MakeValid()  where Polygon.STArea()>999999 and ApprovalID=" + GCA.ApprovalID;

            int res= Util.doSQL(SQL);

            UpdateRingQuery = @"Update [GCA_Approval]  set 
                            InnerBoundary=Polygon.STBuffer(BoundaryInMeters),
                            InnerBoundaryCoord = Polygon.STBuffer(BoundaryInMeters).ToString() where ApprovalID=" + GCA.ApprovalID;

       **/   




            return null;
        }

    }//class
}//namespace