using System.Collections.Generic;
using KonbiCloud.Authorization.Users.Dto;
using KonbiCloud.Dto;

namespace KonbiCloud.Authorization.Users.Exporting
{
    public interface IUserListExcelExporter
    {
        FileDto ExportToFile(List<UserListDto> userListDtos);
    }
}