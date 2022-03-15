using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.CatalogModule.Core
{
    [ExcludeFromCodeCoverage]
    public static class ModuleConstants
    {
        public static class Security
        {
            public static class Permissions
            {
                public const string Access = "catalog:access",
                  Create = "catalog:create",
                  Read = "catalog:read",
                  Update = "catalog:update",
                  Delete = "catalog:delete",
                  Export = "catalog:export",
                  Import = "catalog:import",
                  CatalogBrowseFiltersRead = "catalog:BrowseFilters:Read",
                  CatalogBrowseFiltersUpdate = "catalog:BrowseFilters:Update",
                  CategoryChange = "bulk-action:category:change",
                  PropertiesUpdate = "bulk-action:properties:update";

                public static string[] AllPermissions { get; } = { Access, Create, Read, Update, Delete, Export, Import, CatalogBrowseFiltersRead, CatalogBrowseFiltersUpdate, CategoryChange, PropertiesUpdate };
            }
        }

        public static class Settings
        {
            public static class General
            {
                public static SettingDescriptor CopyIDMenuItem { get; } = new SettingDescriptor
                {
                    Name = "Catalog.AllowToCopyID",
                    GroupName = "Catalog|General",
                    ValueType = SettingValueType.Boolean,
                    DefaultValue = false
                };

                public static SettingDescriptor ImageCategories { get; } = new SettingDescriptor
                {
                    Name = "Catalog.ImageCategories",
                    GroupName = "Catalog|General",
                    ValueType = SettingValueType.ShortText,
                    IsDictionary = true,
                    AllowedValues = new[] { "Images" }
                };

                public static SettingDescriptor AssociationGroups { get; } = new SettingDescriptor
                {
                    Name = "Catalog.AssociationGroups",
                    ValueType = SettingValueType.ShortText,
                    GroupName = "Catalog|General",
                    IsDictionary = true,
                    AllowedValues = new[] { "Accessories", "Related Items" }
                };

                public static SettingDescriptor EditorialReviewTypes { get; } = new SettingDescriptor
                {
                    Name = "Catalog.EditorialReviewTypes",
                    ValueType = SettingValueType.ShortText,
                    GroupName = "Catalog|General",
                    IsDictionary = true,
                    DefaultValue = "QuickReview",
                    AllowedValues = new[] { "QuickReview", "FullReview" }
                };

                public static SettingDescriptor CategoryDescriptionTypes { get; } = new SettingDescriptor
                {
                    Name = "Catalog.CategoryDescriptionTypes",
                    ValueType = SettingValueType.ShortText,
                    GroupName = "Catalog|General",
                    IsDictionary = true,
                    DefaultValue = "QuickReview",
                    AllowedValues = new[] { "QuickReview", "FullReview" }
                };

                public static SettingDescriptor UseSeoDeduplication { get; } = new SettingDescriptor
                {
                    Name = "Catalog.UseSeoDeduplication",
                    GroupName = "Catalog|General",
                    ValueType = SettingValueType.Boolean,
                    DefaultValue = false
                };

                public static SettingDescriptor EventBasedIndexation { get; } = new SettingDescriptor
                {
                    Name = "Catalog.Search.EventBasedIndexation.Enable",
                    GroupName = "Catalog|General",
                    ValueType = SettingValueType.Boolean,
                    DefaultValue = false
                };

                public static IEnumerable<SettingDescriptor> AllSettings
                {
                    get
                    {
                        return new List<SettingDescriptor>
                               {
                                   ImageCategories,
                                   AssociationGroups,
                                   EditorialReviewTypes,
                                   CategoryDescriptionTypes,
                                   EventBasedIndexation,
                                   CopyIDMenuItem
                               };
                    }
                }
            }

            public static class Search
            {
                public static SettingDescriptor UseCatalogIndexedSearchInManager { get; } = new SettingDescriptor
                {
                    Name = "Catalog.Search.UseCatalogIndexedSearchInManager",
                    GroupName = "Catalog|Search",
                    ValueType = SettingValueType.Boolean,
                    DefaultValue = true
                };

                public static SettingDescriptor UseFullObjectIndexStoring { get; } = new SettingDescriptor
                {
                    Name = "Catalog.Search.UseFullObjectIndexStoring",
                    GroupName = "Catalog|Search",
                    ValueType = SettingValueType.Boolean,
                    DefaultValue = false
                };

                public static SettingDescriptor IndexationDateProduct { get; } = new SettingDescriptor
                {
                    Name = "VirtoCommerce.Search.IndexingJobs.IndexationDate.Product",
                    GroupName = "Catalog|Search",
                    ValueType = SettingValueType.DateTime,
                    DefaultValue = default(DateTime)
                };

                public static SettingDescriptor IndexationDateCategory { get; } = new SettingDescriptor
                {
                    Name = "VirtoCommerce.Search.IndexingJobs.IndexationDate.Category",
                    GroupName = "Catalog|Search",
                    ValueType = SettingValueType.DateTime,
                    DefaultValue = default(DateTime)
                };

                public static IEnumerable<SettingDescriptor> AllSettings
                {
                    get
                    {
                        yield return UseCatalogIndexedSearchInManager;
                        yield return UseFullObjectIndexStoring;
                        yield return IndexationDateProduct;
                        yield return IndexationDateCategory;
                    }
                }
            }

            public static IEnumerable<SettingDescriptor> AllSettings
            {
                get
                {
                    return General.AllSettings.Concat(Search.AllSettings);
                }
            }
        }

        public const string OutlineDelimiter = "___";
    }
}
