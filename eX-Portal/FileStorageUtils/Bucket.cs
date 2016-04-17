using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

namespace FileStorageUtils
{

    public   class Bucket
    {
        AmazonS3FileStorageProvider am = new AmazonS3FileStorageProvider();
        AmazonS3Config config = new AmazonS3Config();
     

        public bool CreateBucket()
        {
            config.ServiceURL = "s3.amazonaws.com";

            AmazonS3 client = Amazon.AWSClientFactory.CreateAmazonS3Client(
            am.PublicKey,
            am.PrivateKey
            );

            ListBucketsResponse response = client.ListBuckets();

            bool found = false;
            foreach (S3Bucket bucket in response.Buckets)
            {
                if (bucket.BucketName == "Drone")
                {
                    found = true;
                    break;
                }
            }

            if (found == false)
            {
                client.PutBucket(new PutBucketRequest().WithBucketName("Drone"));
            }
            return found;
        }

    }

}
