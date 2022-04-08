﻿using KonbiCloud.Transactions.Dtos;
using KonbiCloud.Transactions;
using KonbiCloud.Restock.Dtos;
using KonbiCloud.Restock;
using KonbiCloud.Inventories.Dtos;
using KonbiCloud.Inventories;
using KonbiCloud.Products.Dtos;
using KonbiCloud.Products;
using KonbiCloud.Machines.Dtos;
using KonbiCloud.Machines;
using Abp.Application.Editions;
using Abp.Application.Features;
using Abp.Auditing;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.EntityHistory;
using Abp.Localization;
using Abp.Notifications;
using Abp.Organizations;
using Abp.UI.Inputs;
using AutoMapper;
using KonbiCloud.Auditing.Dto;
using KonbiCloud.Authorization.Accounts.Dto;
using KonbiCloud.Authorization.Permissions.Dto;
using KonbiCloud.Authorization.Roles;
using KonbiCloud.Authorization.Roles.Dto;
using KonbiCloud.Authorization.Users;
using KonbiCloud.Authorization.Users.Dto;
using KonbiCloud.Authorization.Users.Profile.Dto;
using KonbiCloud.Chat;
using KonbiCloud.Chat.Dto;
using KonbiCloud.Editions;
using KonbiCloud.Editions.Dto;
using KonbiCloud.Friendships;
using KonbiCloud.Friendships.Cache;
using KonbiCloud.Friendships.Dto;
using KonbiCloud.Localization.Dto;
using KonbiCloud.MultiTenancy;
using KonbiCloud.MultiTenancy.Dto;
using KonbiCloud.MultiTenancy.HostDashboard.Dto;
using KonbiCloud.MultiTenancy.Payments;
using KonbiCloud.MultiTenancy.Payments.Dto;
using KonbiCloud.Notifications.Dto;
using KonbiCloud.Organizations.Dto;
using KonbiCloud.Sessions.Dto;
using KonbiCloud.TemperatureLogs;
using KonbiCloud.TemperatureLogs.Dtos;
using KonbiCloud.MachineLoadout;
using System;

namespace KonbiCloud
{
    internal static class CustomDtoMapper
    {
        public static void CreateMappings(IMapperConfigurationExpression configuration)
        {
            configuration.CreateMap<TransactionDetailDto, TransactionDetail>().ReverseMap();
            configuration.CreateMap<CreateOrEditRestockSessionDto, RestockSession>().ReverseMap();
            configuration.CreateMap<RestockSessionDto, RestockSession>().ReverseMap();
            configuration.CreateMap<CreateOrEditInventoryDto, InventoryItem>();
            configuration.CreateMap<InventoryItem, InventoryDto>();
            configuration.CreateMap<CreateOrEditProductDto, Product>();
            configuration.CreateMap<Product, ProductDto>();
            configuration.CreateMap<CreateOrEditProductCategoryDto, ProductCategory>();
            configuration.CreateMap<ProductCategory, ProductCategoryDto>();
            configuration.CreateMap<CreateOrEditSessionDto, Session>();
            configuration.CreateMap<Session, SessionDto>();
            //Inputs
            configuration.CreateMap<CheckboxInputType, FeatureInputTypeDto>();
            configuration.CreateMap<SingleLineStringInputType, FeatureInputTypeDto>();
            configuration.CreateMap<ComboboxInputType, FeatureInputTypeDto>();
            configuration.CreateMap<IInputType, FeatureInputTypeDto>()
                .Include<CheckboxInputType, FeatureInputTypeDto>()
                .Include<SingleLineStringInputType, FeatureInputTypeDto>()
                .Include<ComboboxInputType, FeatureInputTypeDto>();
            configuration.CreateMap<StaticLocalizableComboboxItemSource, LocalizableComboboxItemSourceDto>();
            configuration.CreateMap<ILocalizableComboboxItemSource, LocalizableComboboxItemSourceDto>()
                .Include<StaticLocalizableComboboxItemSource, LocalizableComboboxItemSourceDto>();
            configuration.CreateMap<LocalizableComboboxItem, LocalizableComboboxItemDto>();
            configuration.CreateMap<ILocalizableComboboxItem, LocalizableComboboxItemDto>()
                .Include<LocalizableComboboxItem, LocalizableComboboxItemDto>();

            //Chat
            configuration.CreateMap<ChatMessage, ChatMessageDto>();
            configuration.CreateMap<ChatMessage, ChatMessageExportDto>();

            //Feature
            configuration.CreateMap<FlatFeatureSelectDto, Feature>().ReverseMap();
            configuration.CreateMap<Feature, FlatFeatureDto>();

            //Role
            configuration.CreateMap<RoleEditDto, Role>().ReverseMap();
            configuration.CreateMap<Role, RoleListDto>();
            configuration.CreateMap<UserRole, UserListRoleDto>();

            //Edition
            configuration.CreateMap<EditionEditDto, SubscribableEdition>().ReverseMap();
            configuration.CreateMap<EditionSelectDto, SubscribableEdition>().ReverseMap();
            configuration.CreateMap<SubscribableEdition, EditionInfoDto>();

            configuration.CreateMap<Edition, EditionInfoDto>().Include<SubscribableEdition, EditionInfoDto>();

            configuration.CreateMap<Edition, EditionListDto>();
            configuration.CreateMap<Edition, EditionEditDto>();
            configuration.CreateMap<Edition, SubscribableEdition>();
            configuration.CreateMap<Edition, EditionSelectDto>();

            //Payment
            configuration.CreateMap<SubscriptionPaymentDto, SubscriptionPayment>().ReverseMap();
            configuration.CreateMap<SubscriptionPaymentListDto, SubscriptionPayment>().ReverseMap();
            configuration.CreateMap<SubscriptionPayment, SubscriptionPaymentInfoDto>();

            //Permission
            configuration.CreateMap<Permission, FlatPermissionDto>();
            configuration.CreateMap<Permission, FlatPermissionWithLevelDto>();

            //Language
            configuration.CreateMap<ApplicationLanguage, ApplicationLanguageEditDto>();
            configuration.CreateMap<ApplicationLanguage, ApplicationLanguageListDto>();
            configuration.CreateMap<NotificationDefinition, NotificationSubscriptionWithDisplayNameDto>();
            configuration.CreateMap<ApplicationLanguage, ApplicationLanguageEditDto>()
                .ForMember(ldto => ldto.IsEnabled, options => options.MapFrom(l => !l.IsDisabled));

            //Tenant
            configuration.CreateMap<Tenant, RecentTenant>();
            configuration.CreateMap<Tenant, TenantLoginInfoDto>();
            configuration.CreateMap<Tenant, TenantListDto>();
            configuration.CreateMap<TenantEditDto, Tenant>().ReverseMap();
            configuration.CreateMap<CurrentTenantInfoDto, Tenant>().ReverseMap();

            //User
            configuration.CreateMap<User, UserEditDto>()
                .ForMember(dto => dto.Password, options => options.Ignore())
                .ReverseMap()
                .ForMember(user => user.Password, options => options.Ignore());
            configuration.CreateMap<User, UserLoginInfoDto>();
            configuration.CreateMap<User, UserListDto>();
            configuration.CreateMap<User, ChatUserDto>();
            configuration.CreateMap<User, OrganizationUnitUserListDto>();
            configuration.CreateMap<CurrentUserProfileEditDto, User>().ReverseMap();
            configuration.CreateMap<UserLoginAttemptDto, UserLoginAttempt>().ReverseMap();

            //AuditLog
            configuration.CreateMap<AuditLog, AuditLogListDto>();
            configuration.CreateMap<EntityChange, EntityChangeListDto>();

            //Friendship
            configuration.CreateMap<Friendship, FriendDto>();
            configuration.CreateMap<FriendCacheItem, FriendDto>();

            //OrganizationUnit
            configuration.CreateMap<OrganizationUnit, OrganizationUnitDto>();

            // TrungPQ: TemperatureLogs.
            configuration.CreateMap<TemperatureLog, TemperatureLogListDto>();

            // TrungPQ: ProductMachinePrices.
            configuration.CreateMap<ProductMachinePrice, ProductMachinePriceListDto>();

            configuration.CreateMap<LoadoutItem, LoadoutItemDto>()
                .ForMember(ldto => ldto.MachineId, options => options.MapFrom(l => l.Machine.Id))
                .ForMember(ldto => ldto.ProductId, options => options.MapFrom(l => l.Product == null ? Guid.Empty : l.Product.Id))
                .ForMember(ldto => ldto.ImageUrl, options => options.MapFrom(l => l.Product == null ? null : l.Product.ImageUrl))
                .ForMember(ldto => ldto.ProductName, options => options.MapFrom(l => l.Product == null ? null : l.Product.Name));
        }
    }
}