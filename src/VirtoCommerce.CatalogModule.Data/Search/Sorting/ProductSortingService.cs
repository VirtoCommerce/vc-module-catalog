using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using VirtoCommerce.CatalogModule.Core.Search.Sorting;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.Services;
using static VirtoCommerce.CatalogModule.Core.ModuleConstants.Settings.Search;

namespace VirtoCommerce.CatalogModule.Data.Search.Sorting;

public class ProductSortingService : IProductSortingService
{
    private static readonly JsonSerializerSettings _jsonSettings = new()
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        NullValueHandling = NullValueHandling.Ignore,
        Formatting = Formatting.None,
    };

    private readonly IProductSortingResolverRegistry _registry;
    private readonly IStoreService _storeService;
    private readonly ILogger<ProductSortingService> _logger;

    public ProductSortingService(
        IProductSortingResolverRegistry registry,
        IStoreService storeService,
        ILogger<ProductSortingService> logger)
    {
        _registry = registry;
        _storeService = storeService;
        _logger = logger;
    }

    // Eagerly resolves EVERY sorting's expression+clauses (calls each resolver's GetSortExpression). This is needed
    // by the admin UI, which renders each sorting's clauses/expression. The storefront over-fetches here — its
    // GraphQL surface (sort_definitions: id/name/isDefault/selected) needs no expression, and only the *selected*
    // sorting's expression is applied to the search — but resolving all is kept for simplicity since one shared
    // method serves both callers. This is fine as long as GetSortExpression stays cheap (it runs for all sortings on
    // every product search); a computed/expensive resolver should guard its heavy path on `context.Sort == Code`.
    public Task<IList<ProductSorting>> GetSortingsAsync(ProductSortingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return GetOrderingsInternalAsync(context);
    }

    private async Task<IList<ProductSorting>> GetOrderingsInternalAsync(ProductSortingContext context)
    {
        var entries = await GetStoredEntriesAsync(context.StoreId);
        var entriesByCode = entries
            .Where(x => !string.IsNullOrWhiteSpace(x?.Code))
            .GroupBy(x => x.Code, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);

        var result = new List<ProductSorting>();
        var usedCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // 1. Built-in / module-contributed sortings (the registry is the source of truth for what exists).
        AddResolverSortings(result, usedCodes, entriesByCode, context);

        // 2. Admin-authored custom sortings (stored entries with no backing resolver). Orphan deltas are ignored.
        AddCustomSortings(result, usedCodes, entries);

        var ordered = result
            .OrderBy(x => x.Order)
            .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        // Default = first visible sorting (no separate IsDefault flag; the top of the list surfaces on empty sort).
        MarkDefault(ordered);

        return ordered;
    }

    private void AddResolverSortings(
        List<ProductSorting> result,
        HashSet<string> usedCodes,
        Dictionary<string, ProductSortingEntry> entriesByCode,
        ProductSortingContext context)
    {
        foreach (var resolver in _registry.GetAllResolvers())
        {
            entriesByCode.TryGetValue(resolver.Code, out var entry);
            result.Add(BuildEffectiveForResolver(resolver, entry, context));
            usedCodes.Add(resolver.Code);
        }
    }

    private static void AddCustomSortings(
        List<ProductSorting> result,
        HashSet<string> usedCodes,
        IList<ProductSortingEntry> entries)
    {
        var maxOrder = result.Count > 0 ? result.Max(x => x.Order) : 0;

        foreach (var entry in entries.Where(x => !string.IsNullOrWhiteSpace(x?.Code) && !usedCodes.Contains(x.Code)))
        {
            if (entry.IsCustom && HasUsableClause(entry.Clauses))
            {
                result.Add(BuildEffectiveForCustom(entry, ++maxOrder));
                usedCodes.Add(entry.Code);
            }
        }
    }

    private static void MarkDefault(List<ProductSorting> ordered)
    {
        var defaultSorting = ordered.FirstOrDefault(x => x.IsVisible);
        if (defaultSorting != null)
        {
            defaultSorting.IsDefault = true;
        }
    }

    public async Task SaveSortingsAsync(string storeId, IList<ProductSorting> sortings)
    {
        ArgumentException.ThrowIfNullOrEmpty(storeId);
        sortings ??= [];

        Validate(sortings);

        var store = await _storeService.GetByIdAsync(storeId);
        if (store == null)
        {
            return;
        }

        var context = new ProductSortingContext { StoreId = storeId };
        var entries = sortings
            .Select(sorting => BuildEntry(sorting, context))
            .Where(entry => entry != null)
            .ToList();

        var serialized = entries.Count > 0 ? JsonConvert.SerializeObject(entries, _jsonSettings) : null;

        var setting = store.Settings.FirstOrDefault(x => x.Name.EqualsIgnoreCase(ProductSortings.Name));
        if (setting == null)
        {
            setting = new ObjectSettingEntry(ProductSortings);
            store.Settings.Add(setting);
        }
        setting.Value = serialized;

        await _storeService.SaveChangesAsync([store]);
    }

    private ProductSortingEntry BuildEntry(ProductSorting sorting, ProductSortingContext context)
    {
        // Backing resolver -> persist only the fields that differ from the code default (may be null = no change);
        // otherwise persist the full admin-authored definition.
        var resolver = _registry.GetResolver(sorting.Code);

        return resolver != null
            ? BuildDelta(resolver, sorting, context)
            : BuildCustomEntry(sorting);
    }

    private static ProductSortingEntry BuildCustomEntry(ProductSorting sorting)
    {
        // Empty collections are persisted as null so the JSON stays sparse (NullValueHandling.Ignore).
        return new ProductSortingEntry
        {
            Code = sorting.Code,
            Order = sorting.Order,
            IsVisible = sorting.IsVisible,
            Name = sorting.Name,
            LocalizedNames = NormalizeLocalizedNames(sorting.LocalizedNames) is { Count: > 0 } names ? names : null,
            Clauses = NormalizeClauses(sorting.Clauses) is { Count: > 0 } clauses ? clauses : null,
            IsCustom = true,
        };
    }

    protected virtual async Task<IList<ProductSortingEntry>> GetStoredEntriesAsync(string storeId)
    {
        if (string.IsNullOrEmpty(storeId))
        {
            return [];
        }

        var store = await _storeService.GetNoCloneAsync(storeId);
        var serialized = store?.Settings.GetValue<string>(ProductSortings);

        return Deserialize(serialized);
    }

    private List<ProductSortingEntry> Deserialize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        try
        {
            return JsonConvert.DeserializeObject<List<ProductSortingEntry>>(value, _jsonSettings) ?? [];
        }
        catch (JsonException ex)
        {
            // Malformed stored value must not break sorting/search: fall back to the code defaults.
            _logger.LogWarning(ex, "Failed to deserialize {Setting}; falling back to default sortings.", ProductSortings.Name);
            return [];
        }
    }

    private static ProductSorting BuildEffectiveForResolver(
        IProductSortingResolver resolver,
        ProductSortingEntry entry,
        ProductSortingContext context)
    {
        var info = resolver.Info ?? new ProductSortingInfo();

        var sorting = new ProductSorting
        {
            Code = resolver.Code,
            IsCustom = false,
            AllowOverride = info.AllowOverride,
            IsExpressionEditable = info.IsExpressionEditable,
            LocalizedNames = NormalizeLocalizedNames(entry?.LocalizedNames),
        };

        ApplyDisplayFields(sorting, info, entry);
        ApplyClauses(sorting, resolver, info, entry, context);

        return sorting;
    }

    private static void ApplyDisplayFields(ProductSorting sorting, ProductSortingInfo info, ProductSortingEntry entry)
    {
        var canOverride = info.AllowOverride && entry != null;
        sorting.Name = canOverride && !string.IsNullOrEmpty(entry.Name) ? entry.Name : info.Name;
        sorting.Order = canOverride && entry.Order.HasValue ? entry.Order.Value : info.Order;
        sorting.IsVisible = canOverride && entry.IsVisible.HasValue ? entry.IsVisible.Value : info.IsVisible;
    }

    private static void ApplyClauses(
        ProductSorting sorting,
        IProductSortingResolver resolver,
        ProductSortingInfo info,
        ProductSortingEntry entry,
        ProductSortingContext context)
    {
        if (info.IsExpressionEditable && entry?.Clauses != null && entry.Clauses.Count > 0)
        {
            sorting.Clauses = NormalizeClauses(entry.Clauses);
            sorting.SortExpression = sorting.Clauses.ToSortExpression();
        }
        else
        {
            var expression = resolver.GetSortExpression(context);
            sorting.SortExpression = expression;
            sorting.Clauses = ParseClauses(expression);
        }
    }

    private static ProductSorting BuildEffectiveForCustom(ProductSortingEntry entry, int fallbackOrder)
    {
        var clauses = NormalizeClauses(entry.Clauses);

        return new ProductSorting
        {
            Code = entry.Code,
            Name = entry.Name,
            LocalizedNames = NormalizeLocalizedNames(entry.LocalizedNames),
            Order = entry.Order ?? fallbackOrder,
            IsVisible = entry.IsVisible ?? true,
            IsExpressionEditable = true,
            AllowOverride = true,
            IsCustom = true,
            Clauses = clauses,
            SortExpression = clauses.ToSortExpression(),
        };
    }

    private static ProductSortingEntry BuildDelta(
        IProductSortingResolver resolver,
        ProductSorting sorting,
        ProductSortingContext context)
    {
        var info = resolver.Info ?? new ProductSortingInfo();
        var entry = new ProductSortingEntry { Code = resolver.Code };

        // Evaluate every delta (no short-circuit) so each changed field is captured, then keep the entry only if something changed.
        var hasDisplayDelta = ApplyDisplayDelta(entry, info, sorting);
        var hasLocalizedDelta = ApplyLocalizedNamesDelta(entry, sorting);
        var hasClausesDelta = ApplyClausesDelta(entry, info, sorting, resolver, context);

        return hasDisplayDelta || hasLocalizedDelta || hasClausesDelta ? entry : null;
    }

    private static bool ApplyDisplayDelta(ProductSortingEntry entry, ProductSortingInfo info, ProductSorting sorting)
    {
        if (!info.AllowOverride)
        {
            return false;
        }

        var hasDelta = false;

        if (sorting.Order != info.Order)
        {
            entry.Order = sorting.Order;
            hasDelta = true;
        }

        if (sorting.IsVisible != info.IsVisible)
        {
            entry.IsVisible = sorting.IsVisible;
            hasDelta = true;
        }

        if (!string.IsNullOrEmpty(sorting.Name) && !string.Equals(sorting.Name, info.Name, StringComparison.Ordinal))
        {
            entry.Name = sorting.Name;
            hasDelta = true;
        }

        return hasDelta;
    }

    private static bool ApplyLocalizedNamesDelta(ProductSortingEntry entry, ProductSorting sorting)
    {
        var localizedNames = NormalizeLocalizedNames(sorting.LocalizedNames);
        if (localizedNames.Count == 0)
        {
            return false;
        }

        entry.LocalizedNames = localizedNames;
        return true;
    }

    private static bool ApplyClausesDelta(
        ProductSortingEntry entry,
        ProductSortingInfo info,
        ProductSorting sorting,
        IProductSortingResolver resolver,
        ProductSortingContext context)
    {
        if (!info.IsExpressionEditable)
        {
            return false;
        }

        var clauses = NormalizeClauses(sorting.Clauses);
        var expression = clauses.ToSortExpression();
        var defaultExpression = resolver.GetSortExpression(context);

        if (string.IsNullOrEmpty(expression) || string.Equals(expression, defaultExpression, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        entry.Clauses = clauses;
        return true;
    }

    private void Validate(IList<ProductSorting> sortings)
    {
        var seenCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var sorting in sortings)
        {
            if (string.IsNullOrWhiteSpace(sorting.Code))
            {
                throw new InvalidOperationException("A sorting code is required.");
            }

            if (!seenCodes.Add(sorting.Code))
            {
                throw new InvalidOperationException($"Duplicate sorting code '{sorting.Code}'.");
            }

            if (string.IsNullOrWhiteSpace(sorting.Name))
            {
                throw new InvalidOperationException($"Sorting '{sorting.Code}' must have a name.");
            }

            // Custom sortings (no backing resolver) must define at least one clause.
            if (_registry.GetResolver(sorting.Code) == null && !HasUsableClause(sorting.Clauses))
            {
                throw new InvalidOperationException($"Sorting '{sorting.Code}' must define at least one clause.");
            }
        }
    }

    private static List<SortClause> ParseClauses(string expression)
    {
        var clauses = new List<SortClause>();

        if (!string.IsNullOrWhiteSpace(expression))
        {
            foreach (var sortInfo in SortInfo.Parse(expression))
            {
                clauses.Add(new SortClause
                {
                    Field = sortInfo.SortColumn,
                    IsDescending = sortInfo.SortDirection == SortDirection.Descending,
                });
            }
        }

        return clauses;
    }

    private static List<SortClause> NormalizeClauses(IEnumerable<SortClause> clauses)
    {
        return clauses?
            .Where(x => !string.IsNullOrWhiteSpace(x?.Field))
            .Select(x => new SortClause { Field = x.Field.Trim(), IsDescending = x.IsDescending })
            .ToList() ?? [];
    }

    private static bool HasUsableClause(IEnumerable<SortClause> clauses)
    {
        return clauses != null && clauses.Any(c => !string.IsNullOrWhiteSpace(c?.Field));
    }

    private static Dictionary<string, string> NormalizeLocalizedNames(IDictionary<string, string> localizedNames)
    {
        return localizedNames?
            .Where(x => !string.IsNullOrWhiteSpace(x.Key) && !string.IsNullOrWhiteSpace(x.Value))
            .ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase)
            ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }
}
