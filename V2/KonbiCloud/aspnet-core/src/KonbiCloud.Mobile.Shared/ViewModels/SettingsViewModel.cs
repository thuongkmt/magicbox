using Abp.Application.Services.Dto;
using KonbiCloud.ApiClient;
using KonbiCloud.ApiClient.Models;
using KonbiCloud.Authorization.Users;
using KonbiCloud.Commands;
using KonbiCloud.Core.DataStorage;
using KonbiCloud.Core.Threading;
using KonbiCloud.Localization;
using KonbiCloud.Models;
using KonbiCloud.Models.Users;
using KonbiCloud.ViewModels.Base;
using KonbiCloud.Views;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace KonbiCloud.ViewModels
{
    public class SettingsViewModel : XamarinViewModel
    {
        private readonly IUserAppService _userAppService;
        private readonly IApplicationContext _applicationContext;
        private readonly IDataStorageManager _dataStorageManager;
        private readonly IAccessTokenManager _accessTokenManager;
        private readonly AbpAuthenticateModel _abpAuthenticateModel;
        public ICommand LogoutCommand => AsyncCommand.Create(Logout);
        public ICommand SaveSettingsCommand => HttpRequestCommand.Create(SaveSettingsAsync);
        public ICommand PageAppearingCommand => HttpRequestCommand.Create(PageAppearingAsync);
        private string _cloudUrl = ApiUrlConfig.BaseUrl;
        public string CloudUrl
        {
            get => _cloudUrl;
            set
            {
                _cloudUrl = value;
                RaisePropertyChanged(() => CloudUrl);
            }
        }
        public string TenantName=> _applicationContext.LoginInfo.Tenant.Name;
        public string Username => _applicationContext.LoginInfo.User.UserName;
        private UserEditOrCreateModel _model;
        public UserEditOrCreateModel Model
        {
            get => _model;
            set
            {
                _model = value;
                RaisePropertyChanged(() => Model);
            }
        }
        public SettingsViewModel(IUserAppService userAppService, IApplicationContext applicationContext, IDataStorageManager dataStorageManager, IAccessTokenManager accessTokenManager, AbpAuthenticateModel abpAuthenticateModel)
        {
            _userAppService = userAppService;
            _applicationContext = applicationContext;
            _dataStorageManager = dataStorageManager;
            _accessTokenManager = accessTokenManager;
            _abpAuthenticateModel = abpAuthenticateModel;
        }
        private Task SaveSettingsAsync()
        {
            throw new NotImplementedException();
        }
        private async Task PageAppearingAsync()
        {
            
        }
        private async Task Logout()
        {
            Cloud cloud = App.Database.GetCloudUrlAsync().Result;
            cloud.NoLogout = false;
            await App.Database.SaveCloudUrlAsync(cloud);

            _accessTokenManager.Logout();
            _applicationContext.LoginInfo = null;
            _abpAuthenticateModel.Password = null;
            await NavigationService.SetMainPage<ConfigView>("From-Logout", clearNavigationHistory: true);
        }

    }
}
