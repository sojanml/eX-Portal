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
      var UKeyEnd = S3url.IndexOf('_');
      return FileOnly.Substring(UKeyEnd + 1);
    }

  }//class
}//namespace