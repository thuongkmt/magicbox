using Abp.MultiTenancy;
using Acr.UserDialogs;
using KonbiCloud.ApiClient;
using KonbiCloud.Authorization.Accounts;
using KonbiCloud.Authorization.Accounts.Dto;
using KonbiCloud.Commands;
using KonbiCloud.Core.DataStorage;
using KonbiCloud.Core.Threading;
using KonbiCloud.Localization;
using KonbiCloud.Localization.Resources;
using KonbiCloud.Models;
using KonbiCloud.Services.Account;
using KonbiCloud.ViewModels.Base;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace KonbiCloud.ViewModels
{
    public class ConfigViewModel : XamarinViewModel
    {
        public ICommand LoginUserCommand => HttpRequestCommand.Create(LoginUserAsync);
        public ICommand PageAppearingCommand => HttpRequestCommand.Create(PageAppearingAsync);
        public string CurrentTenancyNameOrDefault => _applicationContext.CurrentTenant != null
            ? _applicationContext.CurrentTenant.TenancyName
            : L.Localize("NotSelected");

        private readonly IAccountAppService _accountAppService;
        private readonly IApplicationContext _applicationContext;
        private readonly IDataStorageManager _dataStorageManager;
        private readonly IAccountService _accountService;
        private bool _isLoginEnabled;
        private string _cloudUrl;
        private string _tenancyName;
        private string _navigationData;
        private bool _isAutoLoggingIn;
        private bool _isInitialized;
        private Cloud cloudLocal;

        public ConfigViewModel(
            IAccountAppService accountAppService,
            IApplicationContext applicationContext,
            IDataStorageManager dataStorageManager,
            IAccountService accountService
            )
        {
            _accountAppService = accountAppService;
            _applicationContext = applicationContext;
            _dataStorageManager = dataStorageManager;
            _accountService = accountService;
            _isInitialized = false;
            _tenancyName = _applicationContext.CurrentTenant != null ? _applicationContext.CurrentTenant.TenancyName : "";
        }
        private bool IsFromLogout()
        {
            return _navigationData == "From-Logout";
        }

        private void PopulateCredentialsFromStorage()
        {
            UserName = _dataStorageManager.Retrieve(DataStorageKey.Username, "");
            TenancyName = _dataStorageManager.Retrieve(DataStorageKey.TenancyName, "");
            var tenantId = _dataStorageManager.Retrieve<int?>(DataStorageKey.TenantId, null);

            if (tenantId == null)
            {
                _applicationContext.SetAsHost();
            }
            else
            {
                _applicationContext.SetAsTenant(TenancyName, tenantId.Value);
            }

            SetPassword();
            RaisePropertyChanged(() => CurrentTenancyNameOrDefault);
        }

        private async Task PageAppearingAsync()
        {
            cloudLocal = App.Database.GetCloudUrlAsync().Result;
            if (cloudLocal != null)
            {
                CloudUrl = cloudLocal.CloudUrl;
                TenancyName = cloudLocal.TenantName;
                UserName = cloudLocal.UserName;
                Password = cloudLocal.Password;
                if (cloudLocal.LoginSuccess && cloudLocal.NoLogout)
                {
                    await UserConfigurationManager.GetIfNeedsAsync();
                    await SetBusyAsync(async () =>
                    {
                        await AutoLoginIfRequired();
                    });                    
                }
                //ApiUrlConfig.ChangeBaseUrl(cloudLocal.CloudUrl);
            }
            else
            {
                CloudUrl = ApiUrlConfig.BaseUrl;
            }
            //await UserConfigurationManager.GetIfNeedsAsync();
            //PopulateCredentialsFromStorage();
           // await AutoLoginIfRequired();
        }

        public override Task InitializeAsync(object navigationData)
        {
            _navigationData = (string)navigationData;
            _isInitialized = true;
            return Task.CompletedTask;
        }
        public string CloudUrl
        {
            get => _cloudUrl;
            set
            {
                _cloudUrl = value;
                SetLoginButtonEnabled();
                RaisePropertyChanged(() => CloudUrl);
            }
        }
        public string TenancyName
        {
            get => _tenancyName;
            set
            {
                _tenancyName = value;
                RaisePropertyChanged(() => TenancyName);
            }
        }

        public string UserName
        {
            get => _accountService.AbpAuthenticateModel.UserNameOrEmailAddress;
            set
            {
                _accountService.AbpAuthenticateModel.UserNameOrEmailAddress = value;
                SetLoginButtonEnabled();
                RaisePropertyChanged(() => UserName);
            }
        }

        public string Password
        {
            get => _accountService.AbpAuthenticateModel.Password;
            set
            {
                _accountService.AbpAuthenticateModel.Password = value;
                SetLoginButtonEnabled();
                RaisePropertyChanged(() => Password);
            }
        }

        public void SetLoginButtonEnabled()
        {
            IsLoginEnabled = !string.IsNullOrWhiteSpace(UserName) && !string.IsNullOrWhiteSpace(Password) && !string.IsNullOrWhiteSpace(CloudUrl);
        }

        public bool IsLoginEnabled
        {
            get => _isLoginEnabled;
            set
            {
                _isLoginEnabled = value;
                RaisePropertyChanged(() => IsLoginEnabled);
            }
        }

        public bool IsAutoLoggingIn
        {
            get => _isAutoLoggingIn;
            set
            {
                _isAutoLoggingIn = value;
                RaisePropertyChanged(() => IsAutoLoggingIn);
            }
        }

        private void SetPassword()
        {
            if (IsFromLogout())
            {
                Password = null;
                _dataStorageManager.RemoveIfExists(DataStorageKey.Password);
            }
            else
            {
                Password = _dataStorageManager.Retrieve(DataStorageKey.Password, "", true);
            }
        }

        private async Task AutoLoginIfRequired()
        {
            if (Password == null)
            {
                return;
            }

            IsAutoLoggingIn = true;
            await SetBusyAsync(async () =>
            {
                await LoginUserAsync();
                IsAutoLoggingIn = false;
            }, LocalTranslation.Authenticating);
        }

        private async Task LoginUserAsync()
        {            
            if (!string.IsNullOrEmpty(CloudUrl))
            {
                IsLoginEnabled = false;
                ApiUrlConfig.ChangeBaseUrl(CloudUrl);

                if (cloudLocal == null)
                {
                    cloudLocal = new Cloud() { CloudUrl = CloudUrl, TenantName = TenancyName, UserName = UserName, Password = Password };
                }
                else
                {
                    cloudLocal.CloudUrl = CloudUrl;
                    cloudLocal.TenantName = TenancyName;
                    cloudLocal.UserName = UserName;
                    cloudLocal.Password = Password;
                }
                cloudLocal.LoginSuccess = false;
                cloudLocal.NoLogout = true;
                //load config
                await UserConfigurationManager.GetIfNeedsAsync();                

                if (string.IsNullOrEmpty(TenancyName))
                {
                    _applicationContext.SetAsHost();
                    //ApiUrlConfig.ResetBaseUrl();
                    RaisePropertyChanged(() => TenancyName);
                }
                else
                {
                    await WebRequestExecuter.Execute(
                        async () => await _accountAppService.IsTenantAvailable(
                            new IsTenantAvailableInput { TenancyName = TenancyName }),
                        result => IsTenantAvailableExecuted(result, TenancyName)
                    );
                }

                ////await dataStorageService.StoreTenantInfoAsync(_applicationContext.CurrentTenant);
                SetLoginButtonEnabled();

                await SetBusyAsync(async () =>
                {
                    await _accountService.LoginUserAsync();                    
                });

                cloudLocal.LoginSuccess = true;
                await App.Database.SaveCloudUrlAsync(cloudLocal);
            }            
        }      

        private async Task IsTenantAvailableExecuted(IsTenantAvailableOutput result, string tenancyName)
        {
            var tenantAvailableResult = result;

            switch (tenantAvailableResult.State)
            {
                case TenantAvailabilityState.Available:
                    _applicationContext.SetAsTenant(tenancyName, tenantAvailableResult.TenantId.Value);
                    //ApiUrlConfig.ChangeBaseUrl(tenantAvailableResult.ServerRootAddress);
                    RaisePropertyChanged(() => CurrentTenancyNameOrDefault);
                    break;
                case TenantAvailabilityState.InActive:
                    UserDialogs.Instance.HideLoading();
                    await UserDialogs.Instance.AlertAsync(L.Localize("TenantIsNotActive", tenancyName));
                    break;
                case TenantAvailabilityState.NotFound:
                    UserDialogs.Instance.HideLoading();
                    await UserDialogs.Instance.AlertAsync(L.Localize("ThereIsNoTenantDefinedWithName{0}", tenancyName));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
