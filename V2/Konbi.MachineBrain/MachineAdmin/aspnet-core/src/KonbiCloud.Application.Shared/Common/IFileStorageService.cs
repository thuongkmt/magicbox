using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace KonbiCloud.Common
{
    public interface IFileStorageService
    {
        Task<string> CreateOrReplace(string fileId, string fileExt, Stream fileStream);
        string SaveImageToFolder(string imageUrl, string configFolder);
        bool DeleteFile(string path);
    }
}
