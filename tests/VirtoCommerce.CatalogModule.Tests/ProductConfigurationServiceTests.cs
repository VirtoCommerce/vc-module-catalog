using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using VirtoCommerce.AssetsModule.Core.Assets;
using VirtoCommerce.CatalogModule.Core.Model.Configuration;
using VirtoCommerce.CatalogModule.Data.Repositories;
using VirtoCommerce.CatalogModule.Data.Services;
using VirtoCommerce.Platform.Caching;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.Platform.Core.Events;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests;

public class ProductConfigurationServiceTests
{
    [Fact]
    public async Task SaveChangesAsync_MultipleDefaultOptionsInOneSection_KeepsOnlyFirstDefault()
    {
        // Arrange
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(x => x.CommitAsync()).ReturnsAsync(0);

        var repositoryMock = new Mock<ICatalogRepository>();
        repositoryMock.SetupGet(x => x.UnitOfWork).Returns(unitOfWorkMock.Object);

        var service = CreateProductConfigurationService(repositoryMock.Object);
        var configuration = CreateProductConfiguration();

        // Act
        await service.SaveChangesAsync([configuration]);

        // Assert
        var firstSectionDefaults = configuration.Sections[0].Options.Where(x => x.IsDefault).Select(x => x.Id).ToArray();
        Assert.Single(firstSectionDefaults);
        Assert.Equal("option-1", firstSectionDefaults[0]);
        Assert.True(configuration.Sections[1].Options.Single(x => x.Id == "option-5").IsDefault);
    }

    private static ProductConfigurationService CreateProductConfigurationService(ICatalogRepository repository)
    {
        return new ProductConfigurationService(
            () => repository,
            CreatePlatformMemoryCache(),
            Mock.Of<IEventPublisher>(),
            Mock.Of<IBlobUrlResolver>());
    }

    private static ProductConfiguration CreateProductConfiguration()
    {
        return new ProductConfiguration
        {
            ProductId = "product-1",
            Sections =
            [
                new ProductConfigurationSection
                {
                    Id = "section-1",
                    Name = "Section 1",
                    Type = "Product",
                    Options =
                    [
                        new ProductConfigurationOption { Id = "option-1", SectionId = "section-1", ProductId = "product-a", IsDefault = true },
                        new ProductConfigurationOption { Id = "option-2", SectionId = "section-1", ProductId = "product-b", IsDefault = true },
                        new ProductConfigurationOption { Id = "option-3", SectionId = "section-1", ProductId = "product-c" },
                    ],
                },
                new ProductConfigurationSection
                {
                    Id = "section-2",
                    Name = "Section 2",
                    Type = "Text",
                    Options =
                    [
                        new ProductConfigurationOption { Id = "option-4", SectionId = "section-2", Text = "One" },
                        new ProductConfigurationOption { Id = "option-5", SectionId = "section-2", Text = "Two", IsDefault = true },
                    ],
                },
            ],
        };
    }

    private static IPlatformMemoryCache CreatePlatformMemoryCache()
    {
        var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        return new PlatformMemoryCache(memoryCache, Options.Create(new CachingOptions()), Mock.Of<ILogger<PlatformMemoryCache>>());
    }
}
