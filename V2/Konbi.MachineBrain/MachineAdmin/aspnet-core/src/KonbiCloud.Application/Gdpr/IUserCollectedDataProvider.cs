using System.Collections.Generic;
using System.Threading.Tasks;
using Abp;
using KonbiCloud.Dto;

namespace KonbiCloud.Gdpr
{
    public interface IUserCollectedDataProvider
    {
        Task<List<FileDto>> GetFiles(UserIdentifier user);
    }
}
