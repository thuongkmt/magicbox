using System.Collections.Generic;
using MvvmHelpers;
using KonbiCloud.Models.NavigationMenu;

namespace KonbiCloud.Services.Navigation
{
    public interface IMenuProvider
    {
        ObservableRangeCollection<NavigationMenuItem> GetAuthorizedMenuItems(Dictionary<string, string> grantedPermissions);
    }
}