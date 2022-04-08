namespace KonbiCloud.Authorization
{
    /// <summary>
    /// Defines string constants for application's permission names.
    /// <see cref="AppAuthorizationProvider"/> for permission definitions.
    /// </summary>
    public static class AppPermissions
    {
        public const string Pages_TransactionDetails = "Pages.TransactionDetails";
        public const string Pages_TransactionDetails_Create = "Pages.TransactionDetails.Create";
        public const string Pages_TransactionDetails_Edit = "Pages.TransactionDetails.Edit";
        public const string Pages_TransactionDetails_Delete = "Pages.TransactionDetails.Delete";

        public const string Pages_RestockSessions = "Pages.RestockSessions";
        public const string Pages_RestockSessions_Create = "Pages.RestockSessions.Create";
        public const string Pages_RestockSessions_Edit = "Pages.RestockSessions.Edit";
        public const string Pages_RestockSessions_Delete = "Pages.RestockSessions.Delete";

        public const string Pages_Inventories = "Pages.Inventories";
        public const string Pages_Inventories_Create = "Pages.Inventories.Create";
        public const string Pages_Inventories_Edit = "Pages.Inventories.Edit";
        public const string Pages_Inventories_Delete = "Pages.Inventories.Delete";

        public const string Pages_Reports = "Pages.Reports";

        public const string Pages_Products = "Pages.Products";
        public const string Pages_Products_Create = "Pages.Products.Create";
        public const string Pages_Products_Edit = "Pages.Products.Edit";
        public const string Pages_Products_Delete = "Pages.Products.Delete";

        public const string Pages_ProductCategories = "Pages.ProductCategories";
        public const string Pages_ProductCategories_Create = "Pages.ProductCategories.Create";
        public const string Pages_ProductCategories_Edit = "Pages.ProductCategories.Edit";
        public const string Pages_ProductCategories_Delete = "Pages.ProductCategories.Delete";

        public const string Pages_ProductsMachinePrice = "Pages.ProductsMachinePrice";
        public const string Pages_ProductsMachinePrice_Edit = "Pages.ProductsMachinePrice.Edit";

        public const string Pages_ProductTags = "Pages.ProductTags";
        public const string Pages_ProductTags_Delete = "Pages.ProductTags.Delete";

        public const string Pages_Sessions = "Pages.Sessions";
        public const string Pages_Sessions_Create = "Pages.Sessions.Create";
        public const string Pages_Sessions_Edit = "Pages.Sessions.Edit";
        public const string Pages_Sessions_Delete = "Pages.Sessions.Delete";
        public const string Pages_Sessions_Export = "Pages.Sessions.Export";
        public const string Pages_Sessions_Sync = "Pages.Sessions.Sync";

        public const string Pages_Machines = "Pages.Machines";

        public const string Pages_Transactions = "Pages.Transactions";
        public const string Pages_Transactionss_Create = "Pages.Transactions.Create";

        public const string Pages_SystemSetting = "Pages.SystemSetting";
        public const string Pages_AlertSetting = "Pages.AlertSetting";
        public const string Pages_AlertSetting_Edit = "Pages.AlertSetting.Edit";
        public const string Pages_Restock = "Pages.Restocks";

        //COMMON PERMISSIONS (FOR BOTH OF TENANTS AND HOST)

        public const string Pages = "Pages";

        public const string Pages_DemoUiComponents = "Pages.DemoUiComponents";
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

    }
}