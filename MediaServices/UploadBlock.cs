using Microsoft.WindowsAzure.MediaServices.Client;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaServices
{
    class UploadBlock
    {
        static object consoleWriteLock;

        public static void UploadByBlock(string manifestName, AssetCreationOptions options, string[] files)
        {
            CloudMediaContext context = CloudContextHelper.GetContext();
            IIngestManifest manifest = context.IngestManifests.Create(manifestName);

            IAsset asset = context.Assets.Create(manifestName + "_Asset", options);

            IIngestManifestAsset bulkAsset = manifest.IngestManifestAssets.Create(asset, files);

            UploadBlobFile(manifest.BlobStorageUriForUpload, files);

            MonitorBulkManifest(manifest.Id);
        }

        static void UploadBlobFile(string destBlobURI, string[] fileNames)
        {
            foreach (string fileName in fileNames)
            {
                UploadBlobFile(destBlobURI, fileName);
            }
        }

        static void UploadBlobFile(string destBlobURI, string filename)
        {
            Task copytask = new Task(() =>
            {
                var storageaccount = new CloudStorageAccount(new StorageCredentials(CloudContextHelper.STAccount, CloudContextHelper.STKey1), true);
                CloudBlobClient blobClient = storageaccount.CreateCloudBlobClient();
                CloudBlobContainer blobContainer = blobClient.GetContainerReference(destBlobURI);

                string[] splitfilename = filename.Split('\\');
                var blob = blobContainer.GetBlockBlobReference(splitfilename[splitfilename.Length - 1]);

                using (var stream = File.OpenRead(filename))
                    blob.UploadFromStream(stream);

                lock (consoleWriteLock)
                {
                    Console.WriteLine("Upload for {0} completed.", filename);
                }
            });

            copytask.Start();
        }

        static void MonitorBulkManifest(string manifestID)
        {
            bool bContinue = true;

            if (consoleWriteLock == null)
                consoleWriteLock = new object();

            while (bContinue)
            {
                CloudMediaContext context = CloudContextHelper.GetContext();
                IIngestManifest manifest = context.IngestManifests.Where(m => m.Id == manifestID).FirstOrDefault();

                if (manifest != null)
                {
                    lock (consoleWriteLock)
                    {
                        Console.WriteLine("\nWaiting on all file uploads.");
                        Console.WriteLine("PendingFilesCount  : {0}", manifest.Statistics.PendingFilesCount);
                        Console.WriteLine("FinishedFilesCount : {0}", manifest.Statistics.FinishedFilesCount);
                        Console.WriteLine("{0}% complete.\n", (float)manifest.Statistics.FinishedFilesCount / (float)(manifest.Statistics.FinishedFilesCount + manifest.Statistics.PendingFilesCount) * 100);

                        if (manifest.Statistics.PendingFilesCount == 0)
                        {
                            Console.WriteLine("Completed\n");
                            bContinue = false;
                        }
                    }

                    if (manifest.Statistics.FinishedFilesCount < manifest.Statistics.PendingFilesCount)
                        Thread.Sleep(60000);
                }
                else // Manifest is null
                    bContinue = false;
            }
        }
    }
}
