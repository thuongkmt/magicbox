using System;
using System.Net;
using System.Threading.Tasks;
using Abp.Auditing;
using Microsoft.AspNetCore.Mvc;
using KonbiCloud.Dto;
using KonbiCloud.Storage;
using Abp.UI;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using KonbiCloud.Configuration;
using System.IO;
using KonbiCloud.Common;
using System.Drawing.Imaging;
using System.Drawing;
using Abp.IO.Extensions;
using Abp.Domain.Repositories;
using KonbiCloud.Resources;
using KonbiCloud.Web.Helpers;

namespace KonbiCloud.Web.Controllers
{
    public class FileController : KonbiCloudControllerBase
    {
        private readonly ITempFileCacheManager _tempFileCacheManager;
        private readonly IBinaryObjectManager _binaryObjectManager;
        private readonly IConfigurationRoot _appConfiguration;
        private readonly IRepository<Resource, Guid> _resourceRepository;

        public FileController(ITempFileCacheManager tempFileCacheManager, 
            IBinaryObjectManager binaryObjectManager, 
            IHostingEnvironment env,
            IRepository<Resource, Guid> resourceRepository)
        {
            _tempFileCacheManager = tempFileCacheManager;
            _binaryObjectManager = binaryObjectManager;
            _appConfiguration = env.GetAppConfiguration();
            _resourceRepository = resourceRepository;
        }

        [DisableAuditing]
        public ActionResult DownloadTempFile(FileDto file)
        {
            var fileBytes = _tempFileCacheManager.GetFile(file.FileToken);
            if (fileBytes == null)
            {
                return NotFound(L("RequestedFileDoesNotExists"));
            }

            return File(fileBytes, file.FileType, file.FileName);
        }

        [DisableAuditing]
        public async Task<ActionResult> DownloadBinaryFile(Guid id, string contentType, string fileName)
        {
            var fileObject = await _binaryObjectManager.GetOrNullAsync(id);
            if (fileObject == null)
            {
                return StatusCode((int)HttpStatusCode.NotFound);
            }

            return File(fileObject.Bytes, contentType, fileName);
        }


        [HttpPost]
        public async Task<IActionResult> Upload(int? thumbWidth, int? thumbHeight)
        {
            try
            {
                int? tenantId = null;
                var files = Request.Form.Files;
                if(files.Count == 0)
                {
                    return BadRequest();
                }

                if (AbpSession.TenantId != null)
                {
                    tenantId = (int?)AbpSession.TenantId;
                }

                foreach (var file in files)
                {
                    var thumbnail = string.Empty;
                    var fileName = file.SaveFileTo(Path.Combine(Directory.GetCurrentDirectory(), Const.ImageFolder), out thumbnail, thumbWidth ?? 0, thumbHeight ?? 0);
                    await _resourceRepository.InsertAsync(new Resource() { FileName = fileName, Thumbnail = thumbnail, TenantId = tenantId });
                }
                return Ok();
            }
            catch (UserFriendlyException ex)
            {
                Logger.Error(ex.Message);
            }

            return BadRequest();
        }
    }
}