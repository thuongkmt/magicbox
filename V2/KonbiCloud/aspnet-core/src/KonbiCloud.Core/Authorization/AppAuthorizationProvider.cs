using Abp.Authorization;
using Abp.Configuration.Startup;
using Abp.Localization;
using Abp.MultiTenancy;

namespace KonbiCloud.Authorization
{
    /// <summary>
    /// Application's authorization provider.
    /// Defines permissions for the application.
    /// See <see cref="AppPermissions"/> for all permission names.
    /// </summary>
    public class AppAuthorizationProvider : AuthorizationProvider
    {
        private readonly bool _isMultiTenancyEnabled;

        public AppAuthorizationProvider(bool isMultiTenancyEnabled)
        {
            _isMultiTenancyEnabled = isMultiTenancyEnabled;
        }

        public AppAuthorizationProvider(IMultiTenancyConfig multiTenancyConfig)
        {
            _isMultiTenancyEnabled = multiTenancyConfig.IsEnabled;
        }

        public override void SetPermissions(IPermissionDefinitionContext context)
        {
            //COMMON PERMISSIONS (FOR BOTH OF TENANTS AND HOST)

            var pages = context.GetPermissionOrNull(AppPermissions.Pages) ?? context.CreatePermission(AppPermissions.Pages, L("Pages"));

            var transactionDetails = pages.CreateChildPermission(AppPermissions.Pages_TransactionDetails, L("TransactionDetails"), multiTenancySides: MultiTenancySides.Tenant);
            transactionDetails.CreateChildPermission(AppPermissions.Pages_TransactionDetails_Create, L("CreateNewTransactionDetail"), multiTenancySides: MultiTenancySides.Tenant);
            transactionDetails.CreateChildPermission(AppPermissions.Pages_TransactionDetails_Edit, L("EditTransactionDetail"), multiTenancySides: MultiTenancySides.Tenant);
            transactionDetails.CreateChildPermission(AppPermissions.Pages_TransactionDetails_Delete, L("DeleteTransactionDetail"), multiTenancySides: MultiTenancySides.Tenant);

            var restockSessions = pages.CreateChildPermission(AppPermissions.Pages_RestockSessions, L("RestockSessions"));
            restockSessions.CreateChildPermission(AppPermissions.Pages_RestockSessions_Create, L("CreateNewRestockSession"));
            restockSessions.CreateChildPermission(AppPermissions.Pages_RestockSessions_Edit, L("EditRestockSession"));
            restockSessions.CreateChildPermission(AppPermissions.Pages_RestockSessions_Delete, L("DeleteRestockSession"));

            var inventories = pages.CreateChildPermission(AppPermissions.Pages_Inventories, L("Inventories"));
            inventories.CreateChildPermission(AppPermissions.Pages_Inventories_Create, L("CreateNewInventory"));
            inventories.CreateChildPermission(AppPermissions.Pages_Inventories_Edit, L("EditInventory"));
            inventories.CreateChildPermission(AppPermissions.Pages_Inventories_Delete, L("DeleteInventory"));

            var reports = pages.CreateChildPermission(AppPermissions.Pages_Reports, L("Reports"));

            var products = pages.CreateChildPermission(AppPermissions.Pages_Products, L("Products"));
            products.CreateChildPermission(AppPermissions.Pages_Products_Create, L("CreateNewProduct"));
            products.CreateChildPermission(AppPermissions.Pages_Products_Edit, L("EditProduct"));
            products.CreateChildPermission(AppPermissions.Pages_Products_Delete, L("DeleteProduct"));

            var productCategories = pages.CreateChildPermission(AppPermissions.Pages_ProductCategories, L("ProductCategories"));
            productCategories.CreateChildPermission(AppPermissions.Pages_ProductCategories_Create, L("CreateNewProductCategory"));
            productCategories.CreateChildPermission(AppPermissions.Pages_ProductCategories_Edit, L("EditProductCategory"));
            productCategories.CreateChildPermission(AppPermissions.Pages_ProductCategories_Delete, L("DeleteProductCategory"));

            var productTags = pages.CreateChildPermission(AppPermissions.Pages_ProductTags, L("ProductTags"));
            productTags.CreateChildPermission(AppPermissions.Pages_ProductTags_Delete, L("DeleteProductTag"));

            var productsMachinePrice = pages.CreateChildPermission(AppPermissions.Pages_ProductsMachinePrice, L("ProductsMachinePrice"));
            productsMachinePrice.CreateChildPermission(AppPermissions.Pages_ProductsMachinePrice_Edit, L("EditProductsMachinePrice"));

            pages.CreateChildPermission(AppPermissions.Pages_Machines, L("Machines"));
            pages.CreateChildPermission(AppPermissions.Pages_Restock, L("Restocks"));

            pages.CreateChildPermission(AppPermissions.Pages_SystemSetting, L("SystemSetting"));

            var alertSetting = pages.CreateChildPermission(AppPermissions.Pages_AlertSetting, L("AlertSetting"));
            alertSetting.CreateChildPermission(AppPermissions.Pages_AlertSetting_Edit, L("EditAlertSetting"));

            var transactions = pages.CreateChildPermission(AppPermissions.Pages_Transactions, L("Transactions"));
            transactions.CreateChildPermission(AppPermissions.Pages_Transactionss_Create, L("CreateTransaction"));

            pages.CreateChildPermission(AppPermissions.Pages_DemoUiComponents, L("DemoUiComponents"));

            var administration = pages.CreateChildPermission(AppPermissions.Pages_Administration, L("Administration"));

            var roles = administration.CreateChildPermission(AppPermissions.Pages_Administration_Roles, L("Roles"));
            roles.CreateChildPermission(AppPermissions.Pages_Administration_Roles_Create, L("CreatingNewRole"));
            roles.CreateChildPermission(AppPermissions.Pages_Administration_Roles_Edit, L("EditingRole"));
            roles.CreateChildPermission(AppPermissions.Pages_Administration_Roles_Delete, L("DeletingRole"));

            var users = administration.CreateChildPermission(AppPermissions.Pages_Administration_Users, L("Users"));
            users.CreateChildPermission(AppPermissions.Pages_Administration_Users_Create, L("CreatingNewUser"));
            users.CreateChildPermission(AppPermissions.Pages_Administration_Users_Edit, L("EditingUser"));
            users.CreateChildPermission(AppPermissions.Pages_Administration_Users_Delete, L("DeletingUser"));
            users.CreateChildPermission(AppPermissions.Pages_Administration_Users_ChangePermissions, L("ChangingPermissions"));
            users.CreateChildPermission(AppPermissions.Pages_Administration_Users_Impersonation, L("LoginForUsers"));

            var languages = administration.CreateChildPermission(AppPermissions.Pages_Administration_Languages, L("Languages"));
            languages.CreateChildPermission(AppPermissions.Pages_Administration_Languages_Create, L("CreatingNewLanguage"));
            languages.CreateChildPermission(AppPermissions.Pages_Administration_Languages_Edit, L("EditingLanguage"));
            languages.CreateChildPermission(AppPermissions.Pages_Administration_Languages_Delete, L("DeletingLanguages"));
            languages.CreateChildPermission(AppPermissions.Pages_Administration_Languages_ChangeTexts, L("ChangingTexts"));

            administration.CreateChildPermission(AppPermissions.Pages_Administration_AuditLogs, L("AuditLogs"));

            var organizationUnits = administration.CreateChildPermission(AppPermissions.Pages_Administration_OrganizationUnits, L("OrganizationUnits"));
            organizationUnits.CreateChildPermission(AppPermissions.Pages_Administration_OrganizationUnits_ManageOrganizationTree, L("ManagingOrganizationTree"));
            organizationUnits.CreateChildPermission(AppPermissions.Pages_Administration_OrganizationUnits_ManageMembers, L("ManagingMembers"));

            administration.CreateChildPermission(AppPermissions.Pages_Administration_UiCustomization, L("VisualSettings"));

            //TENANT-SPECIFIC PERMISSIONS

            pages.CreateChildPermission(AppPermissions.Pages_Tenant_Dashboard, L("Dashboard"), multiTenancySides: MultiTenancySides.Tenant);

            administration.CreateChildPermission(AppPermissions.Pages_Administration_Tenant_Settings, L("Settings"), multiTenancySides: MultiTenancySides.Tenant);
            administration.CreateChildPermission(AppPermissions.Pages_Administration_Tenant_SubscriptionManagement, L("Subscription"), multiTenancySides: MultiTenancySides.Tenant);

            //HOST-SPECIFIC PERMISSIONS

            var editions = pages.CreateChildPermission(AppPermissions.Pages_Editions, L("Editions"), multiTenancySides: MultiTenancySides.Host);
            editions.CreateChildPermission(AppPermissions.Pages_Editions_Create, L("CreatingNewEdition"), multiTenancySides: MultiTenancySides.Host);
            editions.CreateChildPermission(AppPermissions.Pages_Editions_Edit, L("EditingEdition"), multiTenancySides: MultiTenancySides.Host);
            editions.CreateChildPermission(AppPermissions.Pages_Editions_Delete, L("DeletingEdition"), multiTenancySides: MultiTenancySides.Host);

            var tenants = pages.CreateChildPermission(AppPermissions.Pages_Tenants, L("Tenants"), multiTenancySides: MultiTenancySides.Host);
            tenants.CreateChildPermission(AppPermissions.Pages_Tenants_Create, L("CreatingNewTenant"), multiTenancySides: MultiTenancySides.Host);
            tenants.CreateChildPermission(AppPermissions.Pages_Tenants_Edit, L("EditingTenant"), multiTenancySides: MultiTenancySides.Host);
            tenants.CreateChildPermission(AppPermissions.Pages_Tenants_ChangeFeatures, L("ChangingFeatures"), multiTenancySides: MultiTenancySides.Host);
            tenants.CreateChildPermission(AppPermissions.Pages_Tenants_Delete, L("DeletingTenant"), multiTenancySides: MultiTenancySides.Host);
            tenants.CreateChildPermission(AppPermissions.Pages_Tenants_Impersonation, L("LoginForTenants"), multiTenancySides: MultiTenancySides.Host);

            administration.CreateChildPermission(AppPermissions.Pages_Administration_Host_Settings, L("Settings"), multiTenancySides: MultiTenancySides.Host);
            administration.CreateChildPermission(AppPermissions.Pages_Administration_Host_Maintenance, L("Maintenance"), multiTenancySides: _isMultiTenancyEnabled ? MultiTenancySides.Host : MultiTenancySides.Tenant);
            administration.CreateChildPermission(AppPermissions.Pages_Administration_HangfireDashboard, L("HangfireDashboard"), multiTenancySides: _isMultiTenancyEnabled ? MultiTenancySides.Host : MultiTenancySides.Tenant);
            administration.CreateChildPermission(AppPermissions.Pages_Administration_Host_Dashboard, L("Dashboard"), multiTenancySides: MultiTenancySides.Host);
        }

        private static ILocalizableString L(string name)
        {
            return new LocalizableString(name, KonbiCloudConsts.LocalizationSourceName);
        }
    }
}