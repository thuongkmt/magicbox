import { AbpHttpInterceptor } from '@abp/abpHttpInterceptor';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { NgModule } from '@angular/core';
import * as ApiServiceProxies from './service-proxies';
//migrate from old cloud
import * as MachineApiServiceProxies from './machine-service-proxies';
// import * as InventoryApiServiceProxies from './inventory-service-proxies';
import * as TransactionServiceProxy from './transaction-service-proxies';
import * as EmployeeServiceProxy from './employee-service-proxies';
import * as DashboardServiceProxy from './dashboard-service-proxies';
import * as SystemConfigServiceProxy from './system-config-service-proxies';
import * as GetStartedServiceProxy from './get-started-service-proxies';
import * as UploadServiceProxy from './upload-service-proxies';
import { TemperatureLogsServiceProxy } from './temperature-logs-service-proxies';
import { ProductMachinePriceServiceProxy } from './product-machine-price-service-proxies';
import { AlertSettingAppServicesServiceProxy } from '@shared/service-proxies/alert-configuration-service-proxies';
import { ProductsServiceProxy } from './product-service-proxies';
import { CategoryServiceProxy } from './category-service-proxies';
import { MachineLoadoutServiceProxy } from './machineLoadout-service-proxies';
import { CustomerKonbiWalletServiceServiceProxy } from './customers-wallet-service-proxies';

@NgModule({
    providers: [
        ApiServiceProxies.RestockSessionsServiceProxy,        
        // InventoryApiServiceProxies.InventoryServiceProxy,        
        ApiServiceProxies.TopupReportServiceProxy,     
        ApiServiceProxies.ProductTagsServiceProxy, 
        ApiServiceProxies.SessionsServiceProxy,        
        ApiServiceProxies.AuditLogServiceProxy,
        ApiServiceProxies.CachingServiceProxy,
        ApiServiceProxies.ChatServiceProxy,
        ApiServiceProxies.CommonLookupServiceProxy,
        ApiServiceProxies.EditionServiceProxy,
        ApiServiceProxies.FriendshipServiceProxy,
        ApiServiceProxies.HostSettingsServiceProxy,
        ApiServiceProxies.InstallServiceProxy,
        ApiServiceProxies.LanguageServiceProxy,
        ApiServiceProxies.NotificationServiceProxy,
        ApiServiceProxies.OrganizationUnitServiceProxy,
        ApiServiceProxies.PermissionServiceProxy,
        ApiServiceProxies.ProfileServiceProxy,
        ApiServiceProxies.RoleServiceProxy,
        ApiServiceProxies.SessionServiceProxy,
        ApiServiceProxies.TenantServiceProxy,
        ApiServiceProxies.TenantDashboardServiceProxy,
        ApiServiceProxies.TenantSettingsServiceProxy,
        ApiServiceProxies.TimingServiceProxy,
        ApiServiceProxies.UserServiceProxy,
        ApiServiceProxies.UserLinkServiceProxy,
        ApiServiceProxies.UserLoginServiceProxy,
        ApiServiceProxies.WebLogServiceProxy,
        ApiServiceProxies.AccountServiceProxy,
        ApiServiceProxies.TokenAuthServiceProxy,
        ApiServiceProxies.TenantRegistrationServiceProxy,
        ApiServiceProxies.HostDashboardServiceProxy,
        ApiServiceProxies.PaymentServiceProxy,
        ApiServiceProxies.DemoUiComponentsServiceProxy,
        ApiServiceProxies.InvoiceServiceProxy,
        ApiServiceProxies.SubscriptionServiceProxy,
        ApiServiceProxies.InstallServiceProxy,
        ApiServiceProxies.UiCustomizationSettingsServiceProxy,
        ApiServiceProxies.InventoriesServiceProxy,
        { provide: HTTP_INTERCEPTORS, useClass: AbpHttpInterceptor, multi: true },
        //migrate from old cloud
        MachineApiServiceProxies.MachineServiceProxy,
        ApiServiceProxies.TransactionServiceProxy,
        EmployeeServiceProxy.EmployeeServiceProxy,
        DashboardServiceProxy.DashboardServiceProxy,
        SystemConfigServiceProxy.SystemConfigServiceProxy,
        GetStartedServiceProxy.GetStartedServiceProxy,
        ApiServiceProxies.ResourceServiceProxy,
        UploadServiceProxy.UploadServiceProxy,
        TemperatureLogsServiceProxy,
        ProductMachinePriceServiceProxy,
        AlertSettingAppServicesServiceProxy,
        ProductsServiceProxy,
        CategoryServiceProxy,
        MachineLoadoutServiceProxy,
        CustomerKonbiWalletServiceServiceProxy
    ]
})
export class ServiceProxyModule { }
