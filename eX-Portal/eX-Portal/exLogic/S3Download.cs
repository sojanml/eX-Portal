﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.S3;
using System.Configuration;
using Amazon.S3.Model;
using System.Text;
using System.Security.Cryptography;
using System.Xml;

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




    public static string getStreamURL(string x_urlString) {
      int DurationMinutes = 10;
      string privateKeyId = "APKAIOYYYVDBFAMEFZ7Q"; // Guid.NewGuid().ToString("N");

      String newURL = "https://d1ielm2r49qz2s.cloudfront.net/" + x_urlString;
      String pathToPrivateKey = @"C:\Amazon-Keys\CloundFront-Private-Key-File.xml";

      TimeSpan timeSpanInterval = new TimeSpan(0, DurationMinutes, 0);
      DateTime Expires = DateTime.Now.Add(timeSpanInterval);
      int ExpiresInSec = getExpireInSeconds(Expires);
      // Create the policy statement.
      string strPolicy = CreatePolicyStatement(newURL, ExpiresInSec);

      // Read the policy into a byte buffer.
      byte[] bufferPolicy = Encoding.ASCII.GetBytes(strPolicy);

      // Initialize the SHA1CryptoServiceProvider object and hash the policy data.
      using (SHA1CryptoServiceProvider cryptoSHA1 = new SHA1CryptoServiceProvider()) {
        bufferPolicy = cryptoSHA1.ComputeHash(bufferPolicy);

        // Initialize the RSACryptoServiceProvider object.
        RSACryptoServiceProvider providerRSA = new RSACryptoServiceProvider();
        XmlDocument xmlPrivateKey = new XmlDocument();

        // Load the PrivateKey.xml file generated by ConvertPEMtoXML.
        xmlPrivateKey.Load(pathToPrivateKey);

        // Format the RSACryptoServiceProvider providerRSA and create the signature.
        providerRSA.FromXmlString(xmlPrivateKey.InnerXml);
        RSAPKCS1SignatureFormatter rsaFormatter = new RSAPKCS1SignatureFormatter(providerRSA);
        rsaFormatter.SetHashAlgorithm("SHA1");
        byte[] signedPolicyHash = rsaFormatter.CreateSignature(bufferPolicy);

        // Convert the signed policy to URL safe base 64 encoding.
        string strSignedPolicy = ToUrlSafeBase64String(signedPolicyHash);

        // Concatenate the URL, the timestamp, the signature, and the key pair ID to form the private URL.
        return newURL + "?Expires=" + ExpiresInSec + "&Signature=" + strSignedPolicy + "&Key-Pair-Id=" + privateKeyId;
      }
    }

    private static int getExpireInSeconds(DateTime endTime) {
      TimeSpan endTimeSpanFromNow = (endTime - DateTime.Now);
      TimeSpan intervalEnd =
         (DateTime.UtcNow.Add(endTimeSpanFromNow)) - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
      int endTimestamp = (int)intervalEnd.TotalSeconds;  // END_TIME
      return endTimestamp;
    }

    public static string CreatePolicyStatement(string resourceUrl, int EndTimeInSeconds) {
      // Replace variables in the policy statement.
      String strPolicy = "{\"Statement\":[" +
      "{\"Resource\":\"" + resourceUrl + "\"," +
      "\"Condition\":" +
      "{\"DateLessThan\":" +
      "{\"AWS:EpochTime\":" + EndTimeInSeconds.ToString() + "}" +
      "}}]}";
      return strPolicy;
    }//CreatePolicyStatement()

    public static string ToUrlSafeBase64String(byte[] bytes) {
      return System.Convert.ToBase64String(bytes)
          .Replace('+', '-')
          .Replace('=', '_')
          .Replace('/', '~');
    }//ToUrlSafeBase64String()

  }
}