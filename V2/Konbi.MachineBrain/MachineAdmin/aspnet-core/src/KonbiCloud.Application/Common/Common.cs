using KonbiCloud.Transactions;
using System;
using System.IO;
using System.IO.Compression;

namespace KonbiCloud.Common
{
    public static class Common
    {
        public static bool CompressImage(DetailTransaction newTran, string imgFolderPath, IDetailLogService detailLogService, bool isBegin = true)
        {
            var img = newTran.BeginTranImage;
            if (!isBegin)
            {
                img = newTran.EndTranImage;
            }
            if (string.IsNullOrEmpty(img) || img.Contains(Const.NoImage))
            {
                return true;
            }
            var imgPath = Path.Combine(imgFolderPath, img);
            if (File.Exists(imgPath))
            {
                try
                {
                    using (var outStream = new MemoryStream())
                    {
                        using (var archive = new ZipArchive(outStream, ZipArchiveMode.Create, true))
                        {
                            var fileInArchive = archive.CreateEntry(img, CompressionLevel.Optimal);
                            using (var entryStream = fileInArchive.Open())
                            using (var fileToCompressStream = new MemoryStream(File.ReadAllBytes(imgPath)))
                            {
                                fileToCompressStream.CopyTo(entryStream);
                            }
                        }
                        if (isBegin)
                        {
                            newTran.BeginTranImageByte = outStream.ToArray();
                        }
                        else
                        {
                            newTran.EndTranImageByte = outStream.ToArray();
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (isBegin)
                    {
                        detailLogService.Log($"Error when zipping begin transaction image: {img}");
                    }
                    else
                    {
                        detailLogService.Log($"Error when zipping end transaction image: {img}");
                    }

                    detailLogService.Log(ex.Message);
                    return false;
                }
                return true;
            }
            return true;
        }
    }
}
