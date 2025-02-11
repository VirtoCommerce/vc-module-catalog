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
        public static readonly string ConfigurationSectionTypeProduct = "Product";
        public static readonly string ConfigurationSectionTypeText = "Text";
        public static readonly string ConfigurationSectionTypeFile = "File";

        public static class Security
        {
            public static class Permissions
            {
                public const string Access = "catalog:access";
                public const string Create = "catalog:create";
                public const string Read = "catalog:read";
                public const string Update = "catalog:update";
                public const string Delete = "catalog:delete";
                public const string Export = "catalog:export";
                public const string Import = "catalog:import";
                public const string AddExternalImage = "catalog:add-external-image";
                public const string CatalogBrowseFiltersRead = "catalog:BrowseFilters:Read";
                public const string CatalogBrowseFiltersUpdate = "catalog:BrowseFilters:Update";
                public const string CategoryChange = "bulk-action:category:change";
                public const string PropertiesUpdate = "bulk-action:properties:update";
                public const string MeasuresAccess = "measures:access";
                public const string MeasuresCreate = "measures:create";
                public const string MeasuresRead = "measures:read";
                public const string MeasuresUpdate = "measures:update";
                public const string MeasuresDelete = "measures:delete";
                public const string CatalogMetadataPropertyEdit = "catalog:metadata-property:edit";
                public const string CatalogCustomPropertyEdit = "catalog:custom-property:edit";
                public const string CatalogDictionaryPropertyEdit = "catalog:dictionary-property:edit";
                public const string ConfigurationsRead = "catalog:configurations:read";
                public const string ConfigurationsUpdate = "catalog:configurations:update";
                public const string ConfigurationsDelete = "catalog:configurations:delete";

                public static string[] AllPermissions { get; } =
                {
                    Access,
                    Create,
                    Read,
                    Update,
                    Delete,
                    Export,
                    Import,
                    AddExternalImage,
                    CatalogBrowseFiltersRead,
                    CatalogBrowseFiltersUpdate,
                    CategoryChange,
                    PropertiesUpdate,
                    MeasuresAccess,
                    MeasuresRead,
                    MeasuresCreate,
                    MeasuresUpdate, MeasuresDelete,
                    CatalogMetadataPropertyEdit,
                    CatalogCustomPropertyEdit,
                    CatalogDictionaryPropertyEdit,
                    ConfigurationsRead,
                    ConfigurationsUpdate,
                    ConfigurationsDelete,
                };
            }
        }

        public static class Settings
        {
#pragma warning disable S3218
            public static class General
            {
                public static SettingDescriptor ImageCategories { get; } = new SettingDescriptor
                {
                    Name = "Catalog.ImageCategories",
                    GroupName = "Catalog|General",
                    ValueType = SettingValueType.ShortText,
                    IsDictionary = true,
                    AllowedValues = ["Images"]
                };

                public static SettingDescriptor AssociationGroups { get; } = new SettingDescriptor
                {
                    Name = "Catalog.AssociationGroups",
                    ValueType = SettingValueType.ShortText,
                    GroupName = "Catalog|General",
                    IsDictionary = true,
                    AllowedValues = ["Accessories", "Related Items"]
                };

                public static SettingDescriptor EditorialReviewTypes { get; } = new SettingDescriptor
                {
                    Name = "Catalog.EditorialReviewTypes",
                    ValueType = SettingValueType.ShortText,
                    GroupName = "Catalog|General",
                    IsDictionary = true,
                    DefaultValue = "QuickReview",
                    AllowedValues = ["QuickReview", "FullReview"]
                };

                public static SettingDescriptor CategoryDescriptionTypes { get; } = new SettingDescriptor
                {
                    Name = "Catalog.CategoryDescriptionTypes",
                    ValueType = SettingValueType.ShortText,
                    GroupName = "Catalog|General",
                    IsDictionary = true,
                    DefaultValue = "QuickReview",
                    AllowedValues = ["QuickReview", "FullReview"]
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
                        return
                               [
                                   ImageCategories,
                                   AssociationGroups,
                                   EditorialReviewTypes,
                                   CategoryDescriptionTypes,
                                   UseSeoDeduplication,
                                   EventBasedIndexation,
                               ];
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

                public static SettingDescriptor DefaultAggregationSize { get; } = new SettingDescriptor
                {
                    Name = "Catalog.Search.DefaultAggregationSize",
                    GroupName = "Catalog|Search",
                    ValueType = SettingValueType.Integer,
                    DefaultValue = 25
                };

                public static IEnumerable<SettingDescriptor> AllSettings
                {
                    get
                    {
                        yield return UseCatalogIndexedSearchInManager;
                        yield return UseFullObjectIndexStoring;
                        yield return IndexationDateProduct;
                        yield return IndexationDateCategory;
                        yield return DefaultAggregationSize;
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
#pragma warning restore S3218
        }

        public const string OutlineDelimiter = "___";
        public const string OperationLogVariationMarker = "MainProductId:";
        public const int MaxSEOTitleLength = 70;
    }
}
