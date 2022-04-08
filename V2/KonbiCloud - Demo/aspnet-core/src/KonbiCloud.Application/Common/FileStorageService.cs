using Abp;
using Abp.Dependency;
using KonbiCloud.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;

namespace KonbiCloud.Common
{
    public class FileStorageService : AbpServiceBase, IFileStorageService, ITransientDependency
    {
        //private readonly IConfiguration _configuration;
        private readonly IConfigurationRoot _configuration;

        public FileStorageService(IAppConfigurationAccessor configurationAccessor)
        {
            //this._configuration = configuration;
            this._configuration = configurationAccessor.Configuration;
        }

        public async Task<string> CreateOrReplace(string fileId, string fileExt, Stream fileStream)
        {
            // Parse the connection string and return a reference to the storage account.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_configuration["Azure:StorageConnectionString"]);
            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve a reference to a container.
            CloudBlobContainer container = blobClient.GetContainerReference("imagecontainer");

            // Create the container if it doesn't already exist.
            await container.CreateIfNotExistsAsync();
            await container.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

            // Retrieve reference to a blob named "myblob".
            var fileName = fileId + fileExt;
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName);

            await blockBlob.UploadFromStreamAsync(fileStream);
            return blockBlob.Uri.AbsoluteUri;
        }

        public string SaveImageToFolder(string imageUrl, string configFolder)
        {
            string relativePath = "";
            try
            {
                var newId = Guid.NewGuid();
                var base64Arr = imageUrl.Split(',');
                var base64 = base64Arr[1];
                byte[] data = Convert.FromBase64String(base64);
                using (var ms = new MemoryStream(data))
                {

                    var folderPath = Path.Combine(Directory.GetCurrentDirectory(), configFolder);
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }
                    relativePath = Path.Combine(configFolder, newId.ToString() + ".jpg");
                    var img = Image.FromStream(ms);
                    img.Save(Path.Combine(Directory.GetCurrentDirectory(), relativePath), ImageFormat.Jpeg);
                    return relativePath;
                }
            }
            catch
            {
                Logger.Error($"Cannot save image: {relativePath}");
                return null;
            }
        }

        public bool DeleteFile(string path)
        {
            try
            {
                var rootPath = Path.Combine(Directory.GetCurrentDirectory(), path);
                File.Delete(rootPath);
                return true;
            }
            catch
            {
                Logger.Error($"Cannot delete image: {path}");
                return false;
            }
        }
    }
}
