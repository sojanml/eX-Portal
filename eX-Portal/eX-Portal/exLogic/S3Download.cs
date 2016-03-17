using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.S3;
using System.Configuration;
using Amazon.S3.Model;

namespace eX_Portal.exLogic {


  public static class S3Download {
    private const string PublicKeySetting = "AWSAccessKey";
    private const string PrivateKeySetting = "AWSSecretKey";
    private const string BucketNameSetting = "AWSBucket";

    public static String getURL(String TheKey) {
      String AWSAccessKey = ConfigurationManager.AppSettings[PublicKeySetting];
      String AWSSecretKey = ConfigurationManager.AppSettings[PrivateKeySetting];
      String BucketName = ConfigurationManager.AppSettings[BucketNameSetting];

      TheKey = TheKey.Replace("https://exponent-s3.s3-us-west-2.amazonaws.com/", "");
      
      AmazonS3Client client = new AmazonS3Client(
              AWSAccessKey,
              AWSSecretKey,
              Amazon.RegionEndpoint.USEast1
              );

      GetPreSignedUrlRequest request1 = new GetPreSignedUrlRequest() {
        BucketName = BucketName,
        Key = TheKey,
        Expires = DateTime.Now.AddMinutes(5)
      };

      //return "";
      return client.GetPreSignedURL(request1);

    }

  }
}