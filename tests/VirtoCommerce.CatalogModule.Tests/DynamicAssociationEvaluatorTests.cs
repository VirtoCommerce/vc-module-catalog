using System.Threading.Tasks;
using Moq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.DynamicAssociations;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Services;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests
{
    public class DynamicAssociationEvaluatorTests
    {
        private readonly Mock<IStoreService> _storeServiceMock;
        private readonly Mock<IDynamicAssociationConditionSelector> _dynamicAssociationConditionSelectorMock;
        private readonly Mock<IDynamicAssociationConditionEvaluator> _dynamicAssociationConditionEvaluatorMock;
        private readonly Mock<IItemService> _itemServiceMock;

        private readonly DynamicAssociationEvaluationContext _evaluationContext = new DynamicAssociationEvaluationContext();

        public DynamicAssociationEvaluatorTests()
        {
            _storeServiceMock = CreateStoreServiceMock();
            _dynamicAssociationConditionSelectorMock = CreateDynamicAssociationConditionSelectorMock();
            _dynamicAssociationConditionEvaluatorMock = CreateDynamicAssociationConditionEvaluatorMock();
            _itemServiceMock = CreateItemServiceMock();
        }

        [Theory]
        [InlineData(null, new string[0])]
        [InlineData(new string[0], new string[0])]
        public async Task EvaluateDynamicAssociations_ProductsToMatchEmpty_EmptyCollectionResult(string[] productsToMatch, string[] expectedResult)
        {
            // Arrange
            var evaluator = CreateDynamicAssociationEvaluator();
            _evaluationContext.ProductsToMatch = productsToMatch;

            // Act
            var result = await evaluator.EvaluateDynamicAssociationsAsync(_evaluationContext);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public async Task EvaluateDynamicAssociations_DynamicRuleNotFound_EmptyCollectionResult()
        {
            // Arrange
            _evaluationContext.ProductsToMatch = new[] { string.Empty, };

            _storeServiceMock
                .Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new Store());

            _itemServiceMock
                .Setup(x => x.GetByIdsAsync(It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new[]
                {
                    new CatalogProduct(), 
                });

            var evaluator = CreateDynamicAssociationEvaluator();

            // Act
            var result = await evaluator.EvaluateDynamicAssociationsAsync(_evaluationContext);

            // Assert
            _dynamicAssociationConditionEvaluatorMock
                .Verify(x => x.EvaluateDynamicAssociationConditionAsync(It.IsAny<DynamicAssociationConditionEvaluationRequest>()), Times.Never);

            Assert.Empty(result);
        }

        [Fact]
        public async Task EvaluateDynamicAssociations()
        {
            // Arrange
            _evaluationContext.ProductsToMatch = new[] { string.Empty, };

            _storeServiceMock
                .Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new Store());

            _itemServiceMock
                .Setup(x => x.GetByIdsAsync(It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new[]
                {
                    new CatalogProduct(),
                });

            _dynamicAssociationConditionSelectorMock
                .Setup(x => x.GetDynamicAssociationConditionAsync(It.IsAny<DynamicAssociationEvaluationContext>(), It.IsAny<CatalogProduct>()))
                .ReturnsAsync(new DynamicAssociationConditionEvaluationRequest());

            var evaluator = CreateDynamicAssociationEvaluator();

            // Act
            var result = await evaluator.EvaluateDynamicAssociationsAsync(_evaluationContext);

            // Assert
            Assert.Empty(result);

            _dynamicAssociationConditionEvaluatorMock
                .Verify(
                    x => x.EvaluateDynamicAssociationConditionAsync(It.IsAny<DynamicAssociationConditionEvaluationRequest>()),
                    Times.AtLeastOnce
                    );
        }


        private static Mock<IStoreService> CreateStoreServiceMock()
        {
            var result = new Mock<IStoreService>();

            return result;
        }

        private static Mock<IDynamicAssociationConditionSelector> CreateDynamicAssociationConditionSelectorMock()
        {
            var result = new Mock<IDynamicAssociationConditionSelector>();

            return result;
        }

        private static Mock<IItemService> CreateItemServiceMock()
        {
            var result = new Mock<IItemService>();

            return result;
        }

        private static Mock<IDynamicAssociationConditionEvaluator> CreateDynamicAssociationConditionEvaluatorMock()
        {
            var result = new Mock<IDynamicAssociationConditionEvaluator>();

            return result;
        }

        private IDynamicAssociationEvaluator CreateDynamicAssociationEvaluator()
        {
            var result = new DynamicAssociationEvaluator(
                _storeServiceMock.Object,
                _dynamicAssociationConditionSelectorMock.Object,
                _itemServiceMock.Object,
                _dynamicAssociationConditionEvaluatorMock.Object
                );

            return result;
        }
    }
}
