using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Acr.UserDialogs;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using MvvmCross;
using MvvmCross.Core.ViewModels;
using MvvmCross.Core.Views;
using MvvmCross.Droid.Platform;
using MvvmCross.Droid.Views;
using MvvmCross.Platform.Platform;
using Plugin.Permissions;
using Plugin.Settings;

namespace KonbiCloud
{
    public class Setup : MvxAndroidSetup
    {
        public Setup(Context applicationContext) : base(applicationContext)
        {
            //SystemSound_Android.Initialization(applicationContext);
        }

        protected override IMvxApplication CreateApp()
        {
            return new BleMvxApplication();
        }

        protected override IMvxTrace CreateDebugTrace()
        {
            return new DebugTrace();
        }

        //protected override IMvxAndroidViewPresenter CreateViewPresenter()
        //{
        //    var presenter = new MvxFormsDroidPagePresenter();
        //    Mvx.RegisterSingleton<IMvxViewPresenter>(presenter);
        //    return presenter;
        //}

        protected override void InitializeIoC()
        {
            base.InitializeIoC();

            Mvx.IoCProvider.RegisterSingleton(() => UserDialogs.Instance);
            Mvx.IoCProvider.RegisterSingleton(() => CrossSettings.Current);
            Mvx.IoCProvider.RegisterSingleton(() => CrossPermissions.Current);
        }
    }
}