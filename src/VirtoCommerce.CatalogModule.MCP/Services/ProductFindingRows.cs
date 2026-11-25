using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.MCP.ProductSetup;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.MCP.Models;
using VirtoCommerce.MCP.Core.Extensions;
using VirtoCommerce.MCP.Core.Models;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.MCP.Services;

// The findings of the product check as the tools return them, per product: the ones the user may read as rows, and a note
// per part for the others, which still decide whether the product can be bought, so every user gets the same answer.
public class ProductFindingRows(IProductSetupService setupService, CatalogAccess access)
{
    // A product's findings, split by what the user may read: rows most severe first, and the notes of the hidden ones.
    public virtual async Task<(IList<ProductFinding> Rows, IList<string> NotShown)> CreateAsync(ClaimsPrincipal user, CatalogProduct product,
        IEnumerable<McpFinding> findings, string backOfficeLink)
    {
        var parts = setupService.GetParts();
        var rows = new List<ProductFinding>();
        var hidden = new List<(IProductSetupPart Part, McpFinding Finding)>();
        var readable = new Dictionary<IProductSetupPart, bool>();

        foreach (var finding in findings.Where(x => x.ObjectId.EqualsIgnoreCase(product.Id)).OrderBySeverity())
        {
            var part = parts.FirstOrDefault(x => x.Name.EqualsIgnoreCase(finding.Category));

            // All the findings of a part show, or none do, so each part is decided once.
            if (part != null && !readable.TryGetValue(part, out var canRead))
            {
                canRead = await access.CanReadPartAsync(user, part, product);
                readable[part] = canRead;
            }

            if (part == null || readable[part])
            {
                rows.Add(Create(finding, product, backOfficeLink));
            }
            else
            {
                hidden.Add((part, finding));
            }
        }

        var notShown = hidden
            .GroupBy(x => x.Part)
            .Select(x => GetNotShownNote(x.Key, x.Select(y => y.Finding).ToList()))
            .ToList();

        return (rows, notShown);
    }

    // Whether the product can be bought, from every finding, shown or not.
    public virtual bool CanBeBought(CatalogProduct product, IEnumerable<McpFinding> findings)
    {
        return !findings.Any(x => x.ObjectId.EqualsIgnoreCase(product.Id) && setupService.BlocksSale(x));
    }

    public virtual ProductFinding Create(McpFinding finding, CatalogProduct product, string backOfficeLink)
    {
        ArgumentNullException.ThrowIfNull(finding);

        var row = AbstractTypeFactory<ProductFinding>.TryCreateInstance();
        CopyFinding(finding, row);
        row.Sku = product.Code;
        row.BlocksSale = setupService.BlocksSale(finding);
        row.Fix = GetFix(finding, backOfficeLink);

        return row;
    }

    // Every field of the part's finding: a test sets each one, so a field McpFinding gains is not left behind.
    protected virtual void CopyFinding(McpFinding finding, ProductFinding row)
    {
        row.Id = finding.Id;
        row.Severity = finding.Severity;
        row.Title = finding.Title;
        row.Why = finding.Why;
        row.Evidence = finding.Evidence;
        row.Fix = finding.Fix;
        row.Screen = finding.Screen;
        row.ManualOnly = finding.ManualOnly;
        row.Category = finding.Category;
        row.ObjectType = finding.ObjectType;
        row.ObjectId = finding.ObjectId;
        row.ObjectName = finding.ObjectName;
    }

    // The part's fix, then the tool that fixes it when there is one, then the back-office screen with its link.
    protected virtual string GetFix(McpFinding finding, string backOfficeLink)
    {
        var fix = new List<string>();

        if (!string.IsNullOrEmpty(finding.Fix))
        {
            fix.Add(finding.Fix);
        }

        var tool = finding.ManualOnly ? null : GetTool(finding);

        if (!string.IsNullOrEmpty(tool))
        {
            fix.Add($"Tool: {tool}.");
        }

        if (!string.IsNullOrEmpty(finding.Screen))
        {
            fix.Add($"Back office: {finding.Screen.WithLink(backOfficeLink)}.");
        }

        return string.Join(" ", fix);
    }

    // The tool that fixes a finding, or null when only the back office does.
    protected virtual string GetTool(McpFinding finding)
    {
        return null;
    }

    // "Price: 2 findings not shown, 1 blocking the sale (needs pricing:read)".
    protected virtual string GetNotShownNote(IProductSetupPart part, IList<McpFinding> findings)
    {
        var count = findings.Count == 1 ? "1 finding" : $"{findings.Count} findings";
        var blocking = findings.Count(setupService.BlocksSale);
        var blockingText = blocking > 0 ? $", {blocking} blocking the sale" : string.Empty;

        return $"{part.Title}: {count} not shown{blockingText} ({access.GetReadRequirement(part)})";
    }
}
