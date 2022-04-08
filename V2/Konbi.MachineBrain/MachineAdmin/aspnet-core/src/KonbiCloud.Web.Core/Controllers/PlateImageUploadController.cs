using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.IO.Extensions;
using Abp.UI;
using Abp.Web.Models;
using Microsoft.AspNetCore.Mvc;
using KonbiCloud.DemoUiComponents.Dto;
using KonbiCloud.Storage;
using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using KonbiCloud.Configuration;
using KonbiCloud.Common;

namespace KonbiCloud.Web.Controllers
{
    [AbpMvcAuthorize]
    public class PlateImageUploadController : KonbiCloudControllerBase
    {
        private readonly IBinaryObjectManager _binaryObjectManager;
        private readonly IConfigurationRoot _appConfiguration;

        public PlateImageUploadController(IBinaryObjectManager binaryObjectManager, IHostingEnvironment env)
        {
            _binaryObjectManager = binaryObjectManager;
            _appConfiguration = env.GetAppConfiguration();
        }

        [HttpPost]
        public async Task<List<string>> UploadFiles()
        {
            try
            {
                var files = Request.Form.Files;

                //Check input
                if (files == null)
                {
                    throw new UserFriendlyException(L("File_Empty_Error"));
                }

                List<string> filesOutput = new List<string>();
               // var serverUrl = _appConfiguration["App:ServerRootAddress"];

                foreach (var file in files)
                {
                    var fileExt = Path.GetExtension(file.FileName);

                    if (file.Length > 1048576) //1MB
                    {
                        throw new UserFriendlyException(L("File_SizeLimit_Error"));
                    }

                    byte[] fileBytes;
                    using (var stream = file.OpenReadStream())
                    {
                        fileBytes = stream.GetAllBytes();
                    }

                    var imgFolder = _appConfiguration[AppSettingNames.PlateImageFolder];
                    var image = SaveImageToFolder(fileBytes, "", fileExt, imgFolder);

                    //var fileObject = new BinaryObject(null, fileBytes);
                    //await _binaryObjectManager.SaveAsync(fileObject);

                    filesOutput.Add(image);
                }
                return filesOutput;
            }
            catch (UserFriendlyException ex)
            {
                Logger.Error(ex.Message);
                return null;
                //return Json(new AjaxResponse(new ErrorInfo(ex.Message)));
            }
        }

        public string SaveImageToFolder(byte[] data, string fileName, string fileExt, string configFolder)
        {
            string folderPath = "";
            try
            {
                using (var ms = new MemoryStream(data))
                {

                    folderPath = Path.Combine(Directory.GetCurrentDirectory(), Const.ImageFolder, configFolder);
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    var imageName = fileName;
                    if(fileName == "")
                    {
                        imageName = Guid.NewGuid().ToString() + fileExt;
                    }
                    var img = Image.FromStream(ms);
                    img.Save(Path.Combine(folderPath, imageName), ImageFormat.Jpeg);
                    return imageName;
                }
            }
            catch
            {
                Logger.Error($"Cannot save image: {folderPath}");
                return null;
            }
        }


        [HttpPost]
        public async Task<bool> ImportPlateUploadFiles()
        {
            try
            {
                var files = Request.Form.Files;
                if (files == null)
                {
                    throw new UserFriendlyException(L("File_Empty_Error"));
                }

                List<string> filesOutput = new List<string>();

                foreach (var file in files)
                {
                    //var fileExt = Path.GetExtension(file.FileName);
                    if (file.Length > 1048576) //1MB
                    {
                        throw new UserFriendlyException(L("File_SizeLimit_Error"));
                    }

                    byte[] fileBytes;
                    using (var stream = file.OpenReadStream())
                    {
                        fileBytes = stream.GetAllBytes();
                    }

                    var imgFolder = _appConfiguration[AppSettingNames.PlateImageFolder];
                    var image = SaveImageToFolder(fileBytes, file.FileName, "", imgFolder);

                    filesOutput.Add(image);
                }
                return true;
            }
            catch (UserFriendlyException ex)
            {
                Logger.Error(ex.Message);
                return false;
                //return Json(new AjaxResponse(new ErrorInfo(ex.Message)));
            }
        }


    }
}