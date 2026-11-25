using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VirtoCommerce.CatalogModule.Core.MCP.ProductSetup;
using VirtoCommerce.CatalogModule.Data.MCP.ProductSetup;
using VirtoCommerce.CatalogModule.MCP.Services;
using VirtoCommerce.CatalogModule.Web;
using VirtoCommerce.Platform.Core.Modularity;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests.MCP;

[Trait("Category", "Unit")]
public class CatalogModuleRegistrationTests
{
    // The product setup is registered with the MCP module installed or not, as Pricing and Inventory add their parts to it;
    // the tools' services only with it.
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Initialize_RegistersTheProductSetup_AndTheToolsOnlyWithTheMcpModule(bool mcpInstalled)
    {
        // Arrange
        var moduleService = new Mock<IModuleService>();
        moduleService.Setup(x => x.IsInstalled("VirtoCommerce.MCP")).Returns(mcpInstalled);
        var module = new Module { Configuration = new ConfigurationBuilder().Build(), ModuleService = moduleService.Object };
        var services = new ServiceCollection();

        // Act
        module.Initialize(services);

        // Assert
        Assert.Contains(services, x => x.ServiceType == typeof(IProductSetupService) && x.ImplementationType == typeof(ProductSetupService));
        Assert.Contains(services, x => x.ServiceType == typeof(IProductSetupPart) && x.ImplementationType == typeof(CatalogProductSetupPart));
        Assert.Equal(mcpInstalled, services.Any(x => x.ServiceType == typeof(CatalogAccess)));
    }
}
