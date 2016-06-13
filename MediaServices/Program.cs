using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MediaServices.Client;
using System.Configuration;
using System.IO;

namespace MediaServices
{
    class Program
    {
        static void Main(string[] args)
        {
            //UploadSingleFile.CreateAssetAndUploadSingleFile(AssetCreationOptions.None, "testUploadFile.txt");

            //UploadMultipleFiles.CreateAssetAndUploadMultipleFiles(AssetCreationOptions.None, "C:\\Users\\hongk\\Documents");

            UploadBlock.UploadByBlock("testManifest", AssetCreationOptions.None, Directory.GetFiles("C:\\Users\\hongk\\Documents"));

            Console.Read();
        }
    }
}
