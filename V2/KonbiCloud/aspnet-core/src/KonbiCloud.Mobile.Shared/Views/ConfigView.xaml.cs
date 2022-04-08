using System;
using Xamarin.Forms;

namespace KonbiCloud.Views
{
	public partial class ConfigView : ContentPage, IXamarinView
    {
		public ConfigView()
		{
			InitializeComponent ();
			SetControlFocusses();
		}

		private void SetControlFocusses()
		{
			CloudUrlEntry.Completed += (s, e) =>
			{
				if (string.IsNullOrEmpty(TenantNameEntry.Text))
				{
					TenantNameEntry.Focus();
				}
				else
				{
					ExecuteLoginCommand();
				}
			};

			TenantNameEntry.Completed += (s, e) =>
			{
				if (string.IsNullOrEmpty(UsernameEntry.Text))
				{
					UsernameEntry.Focus();
				}
				else
				{
					ExecuteLoginCommand();
				}
			};

			UsernameEntry.Completed += (s, e) =>
			{
				if (string.IsNullOrEmpty(PasswordEntry.Text))
				{
					PasswordEntry.Focus();
				}
				else
				{
					ExecuteLoginCommand();
				}
			};

			PasswordEntry.Completed += (s, e) =>
			{
				ExecuteLoginCommand();
			};
		}

		private void ExecuteLoginCommand()
		{
			if (LoginButton.IsEnabled)
			{
				LoginButton.Command.Execute(null);
			}
		}
	}
}