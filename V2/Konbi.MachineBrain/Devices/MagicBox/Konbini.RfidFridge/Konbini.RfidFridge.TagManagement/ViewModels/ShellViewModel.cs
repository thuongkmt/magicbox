using Caliburn.Micro;
using Konbini.RfidFridge.TagManagement.Entities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;
using System.Windows.Input;
using Konbini.RfidFridge.TagManagement.Views;
using Screen = Konbini.RfidFridge.TagManagement.Enums.Screen;
using Konbini.RfidFridge.TagManagement.Service;
using Konbini.RfidFridge.TagManagement.Data;
using System.Linq;

namespace Konbini.RfidFridge.TagManagement.ViewModels
{
    public sealed class ShellViewModel : StateViewModel
    {
        #region Properties

        public Screen CurrentScreen = Screen.None;
        public Dictionary<Enums.Screen, StateViewModel> AllViews = new Dictionary<Enums.Screen, StateViewModel>();
        public bool ToggleMenu { get; set; }
        public List<MenuItemModel> DisplayedMenuItemCollection { get; set; }
        private MenuItemModel _selectedMenuItem;
        private bool isAdminMode;
        private System.Windows.Visibility toggleButtonVisibility;

        public MenuItemModel SelectedMenuItem
        {
            get => _selectedMenuItem;

            set
            {
                if (value == null || value.Equals(_selectedMenuItem)) return;
                _selectedMenuItem = value;
                this.ActivateItem(_selectedMenuItem.ScreenName);
                NotifyOfPropertyChange(() => SelectedMenuItem);
            }
        }

        public bool IsAdminMode
        {
            get => isAdminMode; set
            {
                isAdminMode = value;
                NotifyOfPropertyChange(() => IsAdminMode);
                if (IsAdminMode)
                {
                    TopBarVisibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    TopBarVisibility = System.Windows.Visibility.Hidden;
                }
            }
        }

        public System.Windows.Visibility TopBarVisibility
        {
            get => toggleButtonVisibility; set
            {
                toggleButtonVisibility = value;
                NotifyOfPropertyChange(() => TopBarVisibility);
            }
        }

        #endregion Properties

        public ShellViewModel() : base(new EventAggregator())
        {
            RegisterViewModel();
            InitMenu();
            InitDefaultData();
        }

        public void InitDefaultData()
        {
            using (var context = new KDbContext())
            {
                var config = context.Settings.SingleOrDefault(x => x.Key == Enums.SettingKey.CloudUrl);
                if (config == null)
                {
                    context.Settings.Add(new Settings { Key = Enums.SettingKey.CloudUrl, Value = "http://localhost:22743/" });
                    context.SaveChanges();
                }
                config = context.Settings.SingleOrDefault(x => x.Key == Enums.SettingKey.UserName);
                if (config == null)
                {
                    context.Settings.Add(new Settings { Key = Enums.SettingKey.UserName, Value = "admin" });
                    context.SaveChanges();
                }
                config = context.Settings.SingleOrDefault(x => x.Key == Enums.SettingKey.Password);
                if (config == null)
                {
                    context.Settings.Add(new Settings { Key = Enums.SettingKey.Password, Value = "!23Qwe" });
                    context.SaveChanges();
                }
            }
        }


        public void InitMenu()
        {
            TopBarVisibility = System.Windows.Visibility.Hidden;
            DisplayedMenuItemCollection = new List<MenuItemModel>
            {
                new MenuItemModel
                {
                    DisplayName = "Tag Management",
                    ScreenName = Screen.Main,
                    IsDisplay =  true
                },
                 new MenuItemModel
                {
                    DisplayName = "Bulk Insert Tag",
                    ScreenName = Screen.BulkInsertTag,
                    IsDisplay =  true
                },
                new MenuItemModel
                {
                    DisplayName = "Setting",
                    ScreenName = Screen.Setting,
                    IsDisplay =  true
                }
            };
        }

        public void RegisterViewModel()
        {
            AllViews.Add(Screen.Main, new MainViewModel(this.EventAggregator, this));
            AllViews.Add(Screen.Setting, new SettingViewModel(this.EventAggregator));
            AllViews.Add(Screen.BulkInsertTag, new BulkInsertTagsViewModel(this.EventAggregator, this));
        }

        protected override void OnActivate()
        {
            SeriLogService.CreateLoggers();

            base.OnActivate();
            ActivateItem(Screen.BulkInsertTag);
        }

        public override void ActivateItem(object item)
        {
            if (item == null) return;
            var nextScreen = ((Screen?)item).Value;
            if ((nextScreen == Screen.None) || (nextScreen == CurrentScreen)) return;
            IoC.BuildUp(AllViews[nextScreen]);
            base.ActivateItem(AllViews[nextScreen]);
            CurrentScreen = nextScreen;
            Debug.WriteLine(CurrentScreen);
        }

        public void OnKeyPress(object sender, KeyEventArgs e)
        {
            EventAggregator.PublishOnUIThread(new KeyPressedMessage { Key = e.Key });
        }

        public override void Handle(AppMessage message)
        {
            switch (message)
            {
                case StateChangeMessage state:
                    CurrentState = state.State;
                    break;

                case ActiveScreenMessage screen:
                    ActivateItem(screen.Screen);
                    break;

                case KeyPressedMessage key:
                    Debug.WriteLine($"Key pressed: {key.Key}");
                    OnKeyPressed(key.Key);
                    break;
            }
        }

        private void OpenCustomerScreen()
        {
            try
            {
                //var window = IoC.Get<IWindowManager>();
                //var vm = new CustomerViewModel(EventAggregator);
                //window.ShowWindow(vm);
            }
            catch (System.InvalidOperationException)
            {
                // Skip this, to force close customer screen
            }
        }

        public void OnKeyPressed(Key key)
        {
            switch (key)
            {
                //case Key.Decimal:
                //    ToggleMenu = !ToggleMenu;
                //    NotifyOfPropertyChange(() => ToggleMenu);
                //    break;

                //case Key.NumPad0:
                //case Key.D0:
                //    ActivateItem(Screen.Main);
                //    break;

                //case Key.NumPad1:
                //case Key.D1:
                //    if (ToggleMenu)
                //        OpenCustomerScreen();
                //    break;

                //case Key.NumPad2:
                //case Key.D2:
                //    if (ToggleMenu)
                //        ActivateItem(Screen.Float);
                //    break;

                //case Key.NumPad3:
                //case Key.D3:
                //    if (ToggleMenu)
                //        ActivateItem(Screen.SalesLog);
                //    break;

                //case Key.NumPad4:
                //case Key.D4:
                //    if (ToggleMenu)
                //        ActivateItem(Screen.Setting);
                //    break;

            }
        }
    }
}