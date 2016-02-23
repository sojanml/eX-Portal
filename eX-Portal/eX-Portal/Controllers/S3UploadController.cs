using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FileStorageUtils;
using eX_Portal.ViewModel;
using eX_Portal.Models;

namespace eX_Portal.Controllers {
  public class S3UploadController : Controller {
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



  }//class
}//namespace