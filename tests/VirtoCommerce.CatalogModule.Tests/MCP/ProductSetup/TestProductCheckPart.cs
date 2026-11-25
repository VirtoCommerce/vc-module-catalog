using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.MCP.ProductSetup;
using VirtoCommerce.MCP.Core.Models;

namespace VirtoCommerce.CatalogModule.Tests.MCP.ProductSetup;

// A part of another module that checks each product: one finding of the given id per product, or a failure.
public class TestProductCheckPart(string name, string findingId = null, string severity = McpFindingSeverities.Critical, bool fails = false)
    : IProductSetupCheck
{
    public string Name { get; } = name;

    public string Title => $"Test part {Name}";

    public McpSetupPermissions Permissions { get; set; } = new() { Read = $"{name}:read" };

    public Task<IList<McpFinding>> CheckAsync(ProductSetupContext context)
    {
        if (fails)
        {
            throw new InvalidOperationException("The part's own message, which no answer may carry.");
        }

        IList<McpFinding> findings = findingId == null
            ? []
            : context.Products.Select(x => new McpFinding { Id = findingId, Severity = severity, Title = $"{findingId} of {x.Code}", ObjectId = x.Id, Category = "another-category" }).ToList();

        return Task.FromResult(findings);
    }
}
