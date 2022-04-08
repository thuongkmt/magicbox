namespace KonbiCloud.Authorization
{
    /// <summary>
    /// Defines string constants for application's permission names.
    /// <see cref="AppAuthorizationProvider"/> for permission definitions.
    /// </summary>
    public static class AppPermissions
    {
        public const string Pages_RestockSessions = "Pages.RestockSessions";
        public const string Pages_RestockSessions_Create = "Pages.RestockSessions.Create";
        public const string Pages_RestockSessions_Edit = "Pages.RestockSessions.Edit";
        public const string Pages_RestockSessions_Delete = "Pages.RestockSessions.Delete";

        public const string Pages_CreditHistories = "Pages.CreditHistories";
        public const string Pages_CreditHistories_Create = "Pages.CreditHistories.Create";
        public const string Pages_CreditHistories_Edit = "Pages.CreditHistories.Edit";
        public const string Pages_CreditHistories_Delete = "Pages.CreditHistories.Delete";

        public const string Pages_UserCredits = "Pages.UserCredits";
        public const string Pages_UserCredits_Create = "Pages.UserCredits.Create";
        public const string Pages_UserCredits_Edit = "Pages.UserCredits.Edit";
        public const string Pages_UserCredits_Delete = "Pages.UserCredits.Delete";

        public const string Pages_Inventories = "Pages.Inventories";
        public const string Pages_Inventories_Create = "Pages.Inventories.Create";
        public const string Pages_Inventories_Edit = "Pages.Inventories.Edit";
        public const string Pages_Inventories_Delete = "Pages.Inventories.Delete";
        public const string Pages_Inventories_Topup = "Pages.Inventories.Topup";
        public const string Pages_Inventories_Restock = "Pages.Inventories.Restock";

        public const string Pages_ProductCategories = "Pages.ProductCategories";
        public const string Pages_ProductCategories_Create = "Pages.ProductCategories.Create";
        public const string Pages_ProductCategories_Edit = "Pages.ProductCategories.Edit";
        public const string Pages_ProductCategories_Delete = "Pages.ProductCategories.Delete";
        public const string Pages_ProductCategories_Sync = "Pages.ProductCategories.Sync";

        public const string Pages_Products = "Pages.Products";
        public const string Pages_Products_Create = "Pages.Products.Create";
        public const string Pages_Products_Edit = "Pages.Products.Edit";
        public const string Pages_Products_Delete = "Pages.Products.Delete";
        public const string Pages_SyncProducts = "Pages.Products.Sync";

        public const string Pages_Sessions = "Pages.Sessions";
        public const string Pages_Sessions_Create = "Pages.Sessions.Create";
        public const string Pages_Sessions_Edit = "Pages.Sessions.Edit";
        public const string Pages_Sessions_Delete = "Pages.Sessions.Delete";
        public const string Pages_Sessions_Export = "Pages.Sessions.Export";
        public const string Pages_Sessions_Sync = "Pages.Sessions.Sync";

        public const string Pages_Discs = "Pages.Discs";
        public const string Pages_Discs_Create = "Pages.Discs.Create";
        public const string Pages_Discs_Edit = "Pages.Discs.Edit";
        public const string Pages_Discs_Delete = "Pages.Discs.Delete";
        public const string Pages_Discs_Sync = "Pages.Discs.Sync";

        public const string Pages_Plates = "Pages.Plates";
        public const string Pages_Plates_Create = "Pages.Plates.Create";
        public const string Pages_Plates_Edit = "Pages.Plates.Edit";
        public const string Pages_Plates_Delete = "Pages.Plates.Delete";
        public const string Pages_Plates_Inventory_Manager = "Pages.Plates.Inventory_Manager";
        public const string Pages_Plates_ImportCSV = "Pages.Plates.ImportCSV";
        public const string Pages_Plates_ExportCSV = "Pages.Plates.ExportCSV";

        public const string Pages_PlateCategories = "Pages.PlateCategories";
        public const string Pages_PlateCategories_Create = "Pages.PlateCategories.Create";
        public const string Pages_PlateCategories_Edit = "Pages.PlateCategories.Edit";
        public const string Pages_PlateCategories_Delete = "Pages.PlateCategories.Delete";
        public const string Pages_PlateCategories_Sync = "Pages.PlateCategories.Sync";

        public const string Pages_PlateMenus = "Pages.PlateMenus";
        public const string Pages_PlateMenus_Create = "Pages.PlateMenus.Create";
        public const string Pages_PlateMenus_Edit = "Pages.PlateMenus.Edit";
        public const string Pages_PlateMenus_Delete = "Pages.PlateMenus.Delete";
        public const string Pages_PlateMenus_Import = "Pages.PlateMenus.Import";
        public const string Pages_PlateMenus_Export = "Pages.PlateMenus.Export";
        public const string Pages_PlateMenus_Sync = "Pages.PlateMenus.Sync";
        public const string Pages_PlateMenus_Replicate = "Pages.PlateMenus.Replicate";
        public const string Pages_PlateMenus_Generate = "Pages.PlateMenus.Generate";

        public const string Pages_Transactions = "Pages.Transactions";
        public const string Pages_Credit = "Pages.Credit";
        public const string Pages_BlackListCards = "Pages.BlackListCards";
        public const string Pages_Employees = "Pages.Employees";

        public const string Pages_DeviceSetting = "Pages.DeviceSetting";

        public const string Pages_Trays = "Pages.Trays";
        public const string Pages_Trays_Create = "Pages.Trays.Create";
        public const string Pages_Trays_Edit = "Pages.Trays.Edit";
        public const string Pages_Trays_Delete = "Pages.Trays.Delete";
        public const string Pages_Trays_Sync = "Pages.Trays.Sync";

        public const string Pages_SystemSetting = "Pages.SystemSetting";
        public const string Pages_AlertSetting = "Pages.AlertSetting";
        public const string Pages_AlertSetting_Edit = "Pages.AlertSetting.Edit";

        public const string Pages_System = "Pages.System";
        public const string Pages_System_Setting = "Pages.System.Setting";
        public const string Pages_System_Diagnostic = "Pages.System.Diagnostic";

        public const string Pages_Machine_Setting = "Pages.Machine.Setting";
        public const string Page_Temperature = "Pages.Temperature";
        //COMMON PERMISSIONS (FOR BOTH OF TENANTS AND HOST)

        public const string Pages = "Pages";

        public const string Pages_DemoUiComponents= "Pages.DemoUiComponents";
        public const string Pages_Administration = "Pages.Administration";

        public const string Pages_Administration_Roles = "Pages.Administration.Roles";
        public const string Pages_Administration_Roles_Create = "Pages.Administration.Roles.Create";
        public const string Pages_Administration_Roles_Edit = "Pages.Administration.Roles.Edit";
        public const string Pages_Administration_Roles_Delete = "Pages.Administration.Roles.Delete";

        public const string Pages_Administration_Users = "Pages.Administration.Users";
        public const string Pages_Administration_Users_Create = "Pages.Administration.Users.Create";
        public const string Pages_Administration_Users_Edit = "Pages.Administration.Users.Edit";
        public const string Pages_Administration_Users_Delete = "Pages.Administration.Users.Delete";
        public const string Pages_Administration_Users_ChangePermissions = "Pages.Administration.Users.ChangePermissions";
        public const string Pages_Administration_Users_Impersonation = "Pages.Administration.Users.Impersonation";

        public const string Pages_Administration_Languages = "Pages.Administration.Languages";
        public const string Pages_Administration_Languages_Create = "Pages.Administration.Languages.Create";
        public const string Pages_Administration_Languages_Edit = "Pages.Administration.Languages.Edit";
        public const string Pages_Administration_Languages_Delete = "Pages.Administration.Languages.Delete";
        public const string Pages_Administration_Languages_ChangeTexts = "Pages.Administration.Languages.ChangeTexts";

        public const string Pages_Administration_AuditLogs = "Pages.Administration.AuditLogs";

        public const string Pages_Administration_OrganizationUnits = "Pages.Administration.OrganizationUnits";
        public const string Pages_Administration_OrganizationUnits_ManageOrganizationTree = "Pages.Administration.OrganizationUnits.ManageOrganizationTree";
        public const string Pages_Administration_OrganizationUnits_ManageMembers = "Pages.Administration.OrganizationUnits.ManageMembers";

        public const string Pages_Administration_HangfireDashboard = "Pages.Administration.HangfireDashboard";

        public const string Pages_Administration_UiCustomization = "Pages.Administration.UiCustomization";
        public const string Pages_Administration_SyncSetting = "Pages.Administration.SyncSetting";

        //TENANT-SPECIFIC PERMISSIONS

        public const string Pages_Tenant_Dashboard = "Pages.Tenant.Dashboard";

        public const string Pages_Administration_Tenant_Settings = "Pages.Administration.Tenant.Settings";

        public const string Pages_Administration_Tenant_SubscriptionManagement = "Pages.Administration.Tenant.SubscriptionManagement";

        //HOST-SPECIFIC PERMISSIONS

        public const string Pages_Editions = "Pages.Editions";
        public const string Pages_Editions_Create = "Pages.Editions.Create";
        public const string Pages_Editions_Edit = "Pages.Editions.Edit";
        public const string Pages_Editions_Delete = "Pages.Editions.Delete";

        public const string Pages_Tenants = "Pages.Tenants";
        public const string Pages_Tenants_Create = "Pages.Tenants.Create";
        public const string Pages_Tenants_Edit = "Pages.Tenants.Edit";
        public const string Pages_Tenants_ChangeFeatures = "Pages.Tenants.ChangeFeatures";
        public const string Pages_Tenants_Delete = "Pages.Tenants.Delete";
        public const string Pages_Tenants_Impersonation = "Pages.Tenants.Impersonation";

        public const string Pages_Administration_Host_Maintenance = "Pages.Administration.Host.Maintenance";
        public const string Pages_Administration_Host_Settings = "Pages.Administration.Host.Settings";
        public const string Pages_Administration_Host_Dashboard = "Pages.Administration.Host.Dashboard";

        public const string Pages_Reports = "Pages.Reports";

    }
}