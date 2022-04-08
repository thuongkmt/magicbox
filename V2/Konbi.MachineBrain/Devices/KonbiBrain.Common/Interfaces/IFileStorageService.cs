using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonbiBrain.Common.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> CreateOrReplace(string fileId, string fileExt, Stream fileStream);
        Task<string> Download(string localImagePath, string fileName, string cloudImagePath);
    }
}
