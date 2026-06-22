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

public class ProductSearchOrderService : IProductSearchOrderService
{
    private static readonly JsonSerializerSettings _jsonSettings = new()
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        NullValueHandling = NullValueHandling.Ignore,
        Formatting = Formatting.None,
    };

    private readonly IProductSearchOrderResolverRegistry _registry;
    private readonly IStoreService _storeService;
    private readonly ILogger<ProductSearchOrderService> _logger;

    public ProductSearchOrderService(
        IProductSearchOrderResolverRegistry registry,
        IStoreService storeService,
        ILogger<ProductSearchOrderService> logger)
    {
        _registry = registry;
        _storeService = storeService;
        _logger = logger;
    }

    public async Task<IList<ProductSearchOrdering>> GetOrderingsAsync(ProductSearchOrderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var entries = await GetStoredEntriesAsync(context.StoreId);
        var entriesByCode = entries
            .Where(x => !string.IsNullOrWhiteSpace(x?.Code))
            .GroupBy(x => x.Code, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);

        var result = new List<ProductSearchOrdering>();
        var usedCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // 1. Built-in / module-contributed orderings (the registry is the source of truth for what exists).
        foreach (var resolver in _registry.GetAllResolvers())
        {
            entriesByCode.TryGetValue(resolver.Code, out var entry);
            result.Add(BuildEffectiveForResolver(resolver, entry, context));
            usedCodes.Add(resolver.Code);
        }

        // 2. Admin-authored custom orderings (stored entries with no backing resolver). Orphan deltas are ignored.
        var maxOrder = result.Count > 0 ? result.Max(x => x.Order) : 0;
        foreach (var entry in entries.Where(x => !string.IsNullOrWhiteSpace(x?.Code) && !usedCodes.Contains(x.Code)))
        {
            if (entry.IsCustom && entry.Clauses != null && entry.Clauses.Any(c => !string.IsNullOrWhiteSpace(c?.Field)))
            {
                result.Add(BuildEffectiveForCustom(entry, ++maxOrder));
                usedCodes.Add(entry.Code);
            }
        }

        var ordered = result
            .OrderBy(x => x.Order)
            .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        // Default = first visible ordering (no separate IsDefault flag; the top of the list surfaces on empty sort).
        var defaultOrdering = ordered.FirstOrDefault(x => x.IsVisible);
        if (defaultOrdering != null)
        {
            defaultOrdering.IsDefault = true;
        }

        return ordered;
    }

    public async Task<string> GetSortExpressionAsync(ProductSearchOrderContext context, string sort)
    {
        var orderings = await GetOrderingsAsync(context);
        var selected = orderings.FindSelected(sort);

        // Known ordering -> its effective expression; raw/unknown token -> passthrough to the engine.
        return selected?.SortExpression ?? sort;
    }

    public async Task SaveOrderingsAsync(string storeId, IList<ProductSearchOrdering> orderings)
    {
        ArgumentException.ThrowIfNullOrEmpty(storeId);
        orderings ??= [];

        Validate(orderings);

        var store = await _storeService.GetByIdAsync(storeId);
        if (store == null)
        {
            return;
        }

        var context = new ProductSearchOrderContext { StoreId = storeId };
        var entries = new List<ProductSearchOrderingEntry>();

        foreach (var ordering in orderings)
        {
            var resolver = _registry.GetResolver(ordering.Code);
            if (resolver == null)
            {
                // No backing resolver -> persist the full admin-authored definition.
                entries.Add(new ProductSearchOrderingEntry
                {
                    Code = ordering.Code,
                    Order = ordering.Order,
                    IsVisible = ordering.IsVisible,
                    Name = ordering.Name,
                    LocalizedNames = NormalizeLocalizedNames(ordering.LocalizedNames),
                    Clauses = NormalizeClauses(ordering.Clauses),
                    IsCustom = true,
                });
            }
            else
            {
                // Backing resolver -> persist only the fields that differ from the code default.
                var delta = BuildDelta(resolver, ordering, context);
                if (delta != null)
                {
                    entries.Add(delta);
                }
            }
        }

        var serialized = entries.Count > 0 ? JsonConvert.SerializeObject(entries, _jsonSettings) : null;

        var setting = store.Settings.FirstOrDefault(x => x.Name.EqualsIgnoreCase(SortDefinitions.Name));
        if (setting == null)
        {
            setting = new ObjectSettingEntry(SortDefinitions);
            store.Settings.Add(setting);
        }
        setting.Value = serialized;

        await _storeService.SaveChangesAsync([store]);
    }


    protected virtual async Task<IList<ProductSearchOrderingEntry>> GetStoredEntriesAsync(string storeId)
    {
        if (string.IsNullOrEmpty(storeId))
        {
            return [];
        }

        var store = await _storeService.GetNoCloneAsync(storeId);
        var serialized = store?.Settings.GetValue<string>(SortDefinitions);

        return Deserialize(serialized);
    }

    private IList<ProductSearchOrderingEntry> Deserialize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        try
        {
            return JsonConvert.DeserializeObject<List<ProductSearchOrderingEntry>>(value, _jsonSettings) ?? [];
        }
        catch (JsonException ex)
        {
            // Malformed stored value must not break sorting/search: fall back to the code defaults.
            _logger.LogWarning(ex, "Failed to deserialize {Setting}; falling back to default orderings.", SortDefinitions.Name);
            return [];
        }
    }

    private static ProductSearchOrdering BuildEffectiveForResolver(
        IProductSearchOrderResolver resolver,
        ProductSearchOrderingEntry entry,
        ProductSearchOrderContext context)
    {
        var info = resolver.Info ?? new ProductSearchOrderInfo();

        var ordering = new ProductSearchOrdering
        {
            Code = resolver.Code,
            IsCustom = false,
            AllowOverride = info.AllowOverride,
            IsExpressionEditable = info.IsExpressionEditable,
            LocalizedNames = CloneLocalizedNames(entry?.LocalizedNames),
        };

        var canOverride = info.AllowOverride && entry != null;
        ordering.Name = canOverride && !string.IsNullOrEmpty(entry.Name) ? entry.Name : info.Name;
        ordering.Order = canOverride && entry.Order.HasValue ? entry.Order.Value : info.Order;
        ordering.IsVisible = canOverride && entry.IsVisible.HasValue ? entry.IsVisible.Value : info.IsVisible;

        if (info.IsExpressionEditable && entry?.Clauses != null && entry.Clauses.Count > 0)
        {
            ordering.Clauses = CloneClauses(entry.Clauses);
            ordering.SortExpression = ordering.Clauses.ToSortExpression();
        }
        else
        {
            var expression = resolver.GetSortExpression(context);
            ordering.SortExpression = expression;
            ordering.Clauses = ParseClauses(expression);
        }

        return ordering;
    }

    private static ProductSearchOrdering BuildEffectiveForCustom(ProductSearchOrderingEntry entry, int fallbackOrder)
    {
        var clauses = CloneClauses(entry.Clauses);

        return new ProductSearchOrdering
        {
            Code = entry.Code,
            Name = entry.Name,
            LocalizedNames = CloneLocalizedNames(entry.LocalizedNames),
            Order = entry.Order ?? fallbackOrder,
            IsVisible = entry.IsVisible ?? true,
            IsExpressionEditable = true,
            AllowOverride = true,
            IsCustom = true,
            Clauses = clauses,
            SortExpression = clauses.ToSortExpression(),
        };
    }

    private static ProductSearchOrderingEntry BuildDelta(
        IProductSearchOrderResolver resolver,
        ProductSearchOrdering ordering,
        ProductSearchOrderContext context)
    {
        var info = resolver.Info ?? new ProductSearchOrderInfo();
        var entry = new ProductSearchOrderingEntry { Code = resolver.Code };
        var hasDelta = false;

        if (info.AllowOverride)
        {
            if (ordering.Order != info.Order)
            {
                entry.Order = ordering.Order;
                hasDelta = true;
            }

            if (ordering.IsVisible != info.IsVisible)
            {
                entry.IsVisible = ordering.IsVisible;
                hasDelta = true;
            }

            if (!string.IsNullOrEmpty(ordering.Name) && !string.Equals(ordering.Name, info.Name, StringComparison.Ordinal))
            {
                entry.Name = ordering.Name;
                hasDelta = true;
            }
        }

        var localizedNames = NormalizeLocalizedNames(ordering.LocalizedNames);
        if (localizedNames != null)
        {
            entry.LocalizedNames = localizedNames;
            hasDelta = true;
        }

        if (info.IsExpressionEditable)
        {
            var clauses = NormalizeClauses(ordering.Clauses);
            var expression = clauses.ToSortExpression();
            var defaultExpression = resolver.GetSortExpression(context);

            if (!string.IsNullOrEmpty(expression) && !string.Equals(expression, defaultExpression, StringComparison.OrdinalIgnoreCase))
            {
                entry.Clauses = clauses;
                hasDelta = true;
            }
        }

        return hasDelta ? entry : null;
    }

    private void Validate(IList<ProductSearchOrdering> orderings)
    {
        var seenCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var ordering in orderings)
        {
            if (string.IsNullOrWhiteSpace(ordering.Code))
            {
                throw new InvalidOperationException("A sort ordering code is required.");
            }

            if (!seenCodes.Add(ordering.Code))
            {
                throw new InvalidOperationException($"Duplicate sort ordering code '{ordering.Code}'.");
            }

            // Custom orderings (no backing resolver) must define at least one clause.
            if (_registry.GetResolver(ordering.Code) == null &&
                (ordering.Clauses == null || !ordering.Clauses.Any(c => !string.IsNullOrWhiteSpace(c?.Field))))
            {
                throw new InvalidOperationException($"Sort ordering '{ordering.Code}' must define at least one clause.");
            }
        }
    }

    private static IList<SortClause> ParseClauses(string expression)
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

    private static IList<SortClause> NormalizeClauses(IEnumerable<SortClause> clauses)
    {
        var result = clauses?
            .Where(x => !string.IsNullOrWhiteSpace(x?.Field))
            .Select(x => new SortClause { Field = x.Field.Trim(), IsDescending = x.IsDescending })
            .ToList();

        return result is { Count: > 0 } ? result : null;
    }

    private static IList<SortClause> CloneClauses(IEnumerable<SortClause> clauses)
    {
        return clauses?
            .Where(x => !string.IsNullOrWhiteSpace(x?.Field))
            .Select(x => new SortClause { Field = x.Field, IsDescending = x.IsDescending })
            .ToList() ?? [];
    }

    private static IDictionary<string, string> NormalizeLocalizedNames(IDictionary<string, string> localizedNames)
    {
        var result = localizedNames?
            .Where(x => !string.IsNullOrWhiteSpace(x.Key) && !string.IsNullOrWhiteSpace(x.Value))
            .ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);

        return result is { Count: > 0 } ? result : null;
    }

    private static IDictionary<string, string> CloneLocalizedNames(IDictionary<string, string> localizedNames)
    {
        return localizedNames != null
            ? new Dictionary<string, string>(localizedNames, StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }
}
