using Acr.UserDialogs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace KonbiCloud.Extensions
{
    public static class IUserDialogsExtensions
    {
        public static IDisposable ErrorToast(this IUserDialogs dialogs, string title, string message, TimeSpan duration)
        {
            return dialogs.Toast(new ToastConfig(message) { BackgroundColor = Color.Red, Duration = duration });
        }
    }
}
