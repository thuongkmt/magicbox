using Abp.IO.Extensions;
using Microsoft.AspNetCore.Http;
using System;
using System.Drawing;
using System.IO;

namespace KonbiCloud.Web.Helpers
{
    public static class FileHelper
    {
        public static string SaveFileTo(this IFormFile file, string path, out string thumbnail, int width = 0, int height = 0)
        {
            var fileExt = Path.GetExtension(file.FileName);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            byte[] fileBytes;
            using (var stream = file.OpenReadStream())
            {
                fileBytes = stream.GetAllBytes();
            }

            using (var memoryStream = new MemoryStream(fileBytes))
            {
                var image = Image.FromStream(memoryStream);

                var fileName = string.Format("{0}{1}", Guid.NewGuid(), fileExt);
                thumbnail = string.Format("thumb_{0}", fileName);
                image.Save(Path.Combine(path, fileName));

                if (width > 0 && height > 0)
                {
                    Image thumb = image.GetThumbnailImage(width, height, () => false, IntPtr.Zero);
                    thumb.Save(Path.Combine(path, thumbnail));
                }
                return fileName;
            }
        }
    }
}
