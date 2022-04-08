namespace KonbiCloud.Configuration
{
    public static class AppSettingNames
    {
        public const string UiTheme = "App.UiTheme";
        public const string Currency = "Currency";
        public const string CurrencySymbol = "CurrencySymbol";
        public const string CloudName = "CloudName";
        //public const string IotHubConnectionString = "IotHubConnectionString";
        //public const string IotHubUri = "IotHubUri";
        public const string AZURE_REDIS_STORAGE = "Azure:RedisStorageName";
        public const string ShowEmployeeManagement = "ShowEmployeeManagement";
        public const string TenantId = "RfidFridgeSetting.System.Cloud.TenantId";
        public const string MachineId = "RfidFridgeSetting.Machine.Id";

        public const string MachineName = "RfidFridgeSetting.Machine.Name";
        public const string CloudApiUrl = "RfidFridgeSetting.System.Cloud.CloudApiUrl";
        public const string SyncServerUrl = "SyncServerUrl";
        public const string ImageFolder = "ImageFolder";
        public const string MdbCashlessPort = "MdbCashlessPort";
        public const string PlateImageFolder = "PlateImageFolder";
        public const string AllowPushDishToServer = "AllowPushDishToServer";
        public const string TransactionImageFolder = "TransactionImageFolder";
        public const string ScanningStableAfterMiliseconds = "ScanningStableAfterMiliseconds";
        public const string UseCloud = "RfidFridgeSetting.System.Cloud.UseCloud";

        public const string PreventSellingSamePlateInASession = "PreventSellingSamePlateInASession";

        public const string SlackUrl = "RfidFridgeSetting.System.Slack.URL";
        public const string AlertChannel = "RfidFridgeSetting.System.Slack.AlertChannel";
        public const string Username = "RfidFridgeSetting.System.Slack.Username";
        public const string ServerName = "RfidFridgeSetting.System.Slack.ServerName";
        public const string StopSaleMessage = "RfidFridgeSetting.Alert.Messages.StopSaleMessage";

        public const string NormalTemperature = "RfidFridgeSetting.System.Temperature.NormalTemperature";
        public const string EnableStopSale = "RfidFridgeSetting.Machine.EnableStopSale";
        public const string StopSaleTimeSpan = "RfidFridgeSetting.Machine.StopSaleTimeSpan";
        //RabbitMq configurations
        public const string RabbitMqServer = "RfidFridgeSetting.System.Cloud.RabbitMqServer";
        public const string RabbitMqUser = "RfidFridgeSetting.System.Cloud.RabbitMqUser";
        public const string RabbitMqPassword = "RfidFridgeSetting.System.Cloud.RabbitMqPassword";

        public const string RemoveTagAfterSold = "RfidFridgeSetting.System.Inventory.RemoveTagAfterSold";
    }
}
