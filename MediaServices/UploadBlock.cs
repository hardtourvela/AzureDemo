using Microsoft.WindowsAzure.MediaServices.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServices
{
    class UploadBlock
    {
        public static void UploadByBlock(string manifestName, List<Tuple<string, AssetCreationOptions>> filesToUpload)
        {
            CloudMediaContext context = Constants.GetContext();
            IIngestManifest manifest = context.IngestManifests.Create(manifestName);
        }

        private static List<IAsset> AddAssetsToManifest(CloudMediaContext context ,List<Tuple<string, AssetCreationOptions>> filesToUpload)
        {
            List<IAsset> lstToReturn = new List<IAsset>();

            foreach(Tuple<string,AssetCreationOptions> entry in filesToUpload)
            {
                lstToReturn.Add(context.Assets.Create(entry.Item1, entry.Item2));
            }

            return lstToReturn;
        }
    }
}
