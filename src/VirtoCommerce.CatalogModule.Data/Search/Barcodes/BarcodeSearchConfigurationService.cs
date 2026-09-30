using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Search.Barcodes;
using VirtoCommerce.CatalogModule.Data.Search.Indexing;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;
using static VirtoCommerce.CatalogModule.Core.ModuleConstants.Settings.Search;

namespace VirtoCommerce.CatalogModule.Data.Search.Barcodes;

public class BarcodeSearchConfigurationService : IBarcodeSearchConfigurationService
{
    // Product fields that always carry a scannable code, offered first and in this order.
    private static readonly string[] _builtInFieldNames = ["code", "gtin", "manufacturerPartNumber"];

    // "sku" holds the same value as "code" and the filter syntax rewrites it to "code", so it is never a separate option.
    private const string SkuFieldName = "sku";

    private readonly IEnumerable<IndexDocumentConfiguration> _configurations;
    private readonly IPropertySearchService _propertySearchService;
    private readonly IStoreService _storeService;
    private readonly ILogger<BarcodeSearchConfigurationService> _logger;

    public BarcodeSearchConfigurationService(
        IEnumerable<IndexDocumentConfiguration> configurations,
        IPropertySearchService propertySearchService,
        IStoreService storeService,
        ILogger<BarcodeSearchConfigurationService> logger)
    {
        _configurations = configurations;
        _propertySearchService = propertySearchService;
        _storeService = storeService;
        _logger = logger;
    }

    public virtual async Task<BarcodeSearchSettings> GetSettingsAsync(string storeId)
    {
        var result = new BarcodeSearchSettings();

        if (string.IsNullOrEmpty(storeId))
        {
            return result;
        }

        var store = await _storeService.GetNoCloneAsync(storeId);
        if (store == null)
        {
            _logger.LogWarning("Store {StoreId} was not found; returning the default barcode search settings.", storeId);
            return result;
        }

        result.ScannerEnabled = store.Settings.GetValue<bool>(BarcodeScannerEnabled);
        result.Fields = DeserializeFields(store.Settings.GetValue<string>(BarcodeSearchFields));

        return result;
    }

    public virtual async Task SaveSettingsAsync(string storeId, BarcodeSearchSettings settings)
    {
        ArgumentException.ThrowIfNullOrEmpty(storeId);
        ArgumentNullException.ThrowIfNull(settings);

        var store = await _storeService.GetByIdAsync(storeId);
        if (store == null)
        {
            throw new BarcodeSearchStoreNotFoundException(storeId);
        }

        var fields = await ValidateAndNormalizeFieldsAsync(storeId, settings.Fields);

        SetSettingValue(store, BarcodeScannerEnabled, settings.ScannerEnabled);

        // Always serialize (empty -> "[]"). Writing null would not overwrite a previously stored value, leaving the
        // last selected field un-clearable (a "switch back to full text" save would silently no-op).
        SetSettingValue(store, BarcodeSearchFields, JsonConvert.SerializeObject(fields));

        await _storeService.SaveChangesAsync([store]);
    }

    public virtual async Task<IList<BarcodeSearchField>> GetAvailableFieldsAsync(string storeId)
    {
        var schema = await BuildProductSchemaAsync();
        var schemaFields = GetFilterableStringFields(schema);

        var result = new List<BarcodeSearchField>();
        var addedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var builtInFieldName in _builtInFieldNames)
        {
            if (schemaFields.TryGetValue(builtInFieldName, out var field) && addedNames.Add(field.Name))
            {
                result.Add(ToBarcodeSearchField(field, isProductField: true));
            }
        }

        foreach (var propertyName in await GetPropertyFieldNamesAsync())
        {
            if (propertyName.EqualsIgnoreCase(SkuFieldName) ||
                !schemaFields.TryGetValue(propertyName, out var field) ||
                !addedNames.Add(field.Name))
            {
                continue;
            }

            result.Add(ToBarcodeSearchField(field, isProductField: false));
        }

        return result;
    }

    protected virtual Task<IndexDocument> BuildProductSchemaAsync()
    {
        return _configurations.BuildProductSchemaAsync();
    }

    // Short text catalog properties are indexed globally (one product index for all catalogs) under their lowercased
    // name, exactly like the schema builder does it.
    protected virtual async Task<IList<string>> GetPropertyFieldNamesAsync()
    {
        var result = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

        var searchCriteria = AbstractTypeFactory<PropertySearchCriteria>.TryCreateInstance();
        searchCriteria.PropertyTypes = [PropertyType.Product.ToString(), PropertyType.Variation.ToString()];
        searchCriteria.PropertyValueTypes = [PropertyValueType.ShortText];

        await foreach (var searchResult in _propertySearchService.SearchBatches(searchCriteria))
        {
            foreach (var property in searchResult.Results.Where(x => !string.IsNullOrEmpty(x?.Name)))
            {
                result.Add(property.Name.ToLowerInvariant());
            }
        }

        return [.. result];
    }

    private async Task<IList<string>> ValidateAndNormalizeFieldsAsync(string storeId, IList<string> fields)
    {
        var requestedNames = fields?
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .ToList() ?? [];

        if (requestedNames.Count == 0)
        {
            return [];
        }

        // An override of GetAvailableFieldsAsync may return the same name twice; the first one wins.
        var availableNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var availableField in await GetAvailableFieldsAsync(storeId))
        {
            availableNames.TryAdd(availableField.Name, availableField.Name);
        }

        var result = new List<string>();
        var addedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var unknownNames = new List<string>();

        foreach (var requestedName in requestedNames)
        {
            if (!availableNames.TryGetValue(requestedName, out var availableName))
            {
                unknownNames.Add(requestedName);
            }
            else if (addedNames.Add(availableName))
            {
                result.Add(availableName);
            }
        }

        if (unknownNames.Count > 0)
        {
            throw new ValidationException(
            [
                new ValidationFailure(nameof(BarcodeSearchSettings.Fields),
                    $"These fields cannot be used for barcode search: {string.Join(", ", unknownNames)}. Only the product code, GTIN, manufacturer part number and short text catalog properties that are defined in the catalog and present in the product index are allowed."),
            ]);
        }

        return result;
    }

    private static Dictionary<string, IndexDocumentField> GetFilterableStringFields(IndexDocument schema)
    {
        var result = new Dictionary<string, IndexDocumentField>(StringComparer.OrdinalIgnoreCase);

        foreach (var field in schema.Fields.Where(x => x.IsFilterable && x.ValueType == IndexDocumentFieldValueType.String))
        {
            result.TryAdd(field.Name, field);
        }

        return result;
    }

    private static BarcodeSearchField ToBarcodeSearchField(IndexDocumentField field, bool isProductField)
    {
        return new BarcodeSearchField
        {
            Name = field.Name,
            DataType = field.ValueType.ToString(),
            IsCollection = field.IsCollection,
            IsProductField = isProductField,
        };
    }

    private static void SetSettingValue(Store store, SettingDescriptor descriptor, object value)
    {
        var setting = store.Settings.FirstOrDefault(x => x.Name.EqualsIgnoreCase(descriptor.Name));

        if (setting == null)
        {
            setting = new ObjectSettingEntry(descriptor);
            store.Settings.Add(setting);
        }

        setting.Value = value;
    }

    private IList<string> DeserializeFields(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        try
        {
            List<string> fields = JsonConvert.DeserializeObject<List<string>>(value) ?? [];
            return fields.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        }
        catch (JsonException ex)
        {
            // A malformed stored value must not break product search: fall back to full-text matching.
            _logger.LogWarning(ex, "Failed to deserialize {Setting}; barcode search falls back to full-text mode.", BarcodeSearchFields.Name);
            return [];
        }
    }
}
