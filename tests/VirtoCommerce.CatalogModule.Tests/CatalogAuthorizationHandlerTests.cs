using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Data.Authorization;
using VirtoCommerce.CatalogModule.Web.Authorization;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests
{
    public class CatalogAuthorizationHandlerTests
    {
        private readonly IOptions<MvcNewtonsoftJsonOptions> _jsonOptions;

        public CatalogAuthorizationHandlerTests()
        {
            AbstractTypeFactory<PermissionScope>.RegisterType<SelectedCatalogScope>();
            _jsonOptions = CreateJsonConverterWrapper();
            _jsonOptions.Value.SerializerSettings.Converters.Add(new PolymorphJsonConverter());
        }

        [Fact]
        public async Task Handle_Permission_With_Correct_Scope_Catalog()
        {
            var storeMock = CreateStoreServiceMock();

            storeMock
                .Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new Store
                {
                    Id = "testStore",
                    Catalog = "testCatalog"
                });

            var catalogAuthorizationHandler = CreateCatalogAuthorizationHandler(storeMock.Object);

            var dynamicAssociation = new DynamicAssociation();

            var context = CreateAuthorizationHandlerContext(
                "test:permission",
                "test:permission|[{\"catalogId\":\"testCatalog\",\"type\":\"SelectedCatalogScope\",\"label\":\"Electronics\",\"scope\":\"testCatalog\"}]",
                dynamicAssociation);

            await catalogAuthorizationHandler.HandleAsync(context);

            Assert.True(context.HasSucceeded);
        }

        [Fact]
        public async Task Handle_Permission_With_Incorrect_Scope_Catalog()
        {
            var storeMock = CreateStoreServiceMock();

            storeMock
                .Setup(x => x.GetByIdAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new Store
                {
                    Id = "testStore",
                    Catalog = "testCatalog1"
                });

            var catalogAuthorizationHandler = CreateCatalogAuthorizationHandler(storeMock.Object);

            var dynamicAssociation = new DynamicAssociation();

            var context = CreateAuthorizationHandlerContext(
                "test:permission",
                "test:permission|[{\"catalogId\":\"testCatalog\",\"type\":\"SelectedCatalogScope\",\"label\":\"Electronics\",\"scope\":\"testCatalog\"}]",
                dynamicAssociation);

            await catalogAuthorizationHandler.HandleAsync(context);

            Assert.False(context.HasSucceeded);
        }


        private IOptions<MvcNewtonsoftJsonOptions> CreateJsonConverterWrapper()
        {
            var result = new Mock<IOptions<MvcNewtonsoftJsonOptions>>();
            result.Setup(x => x.Value).Returns(new MvcNewtonsoftJsonOptions());

            return result.Object;
        }

        private static ICollection<IAuthorizationRequirement> CreateAuthorizationRequirements(string permission)
        {
            var result = new List<IAuthorizationRequirement>
            {
                new CatalogAuthorizationRequirement(permission)
            };

            return result;
        }

        private static ClaimsPrincipal CreateClaimsPtincipal(string permissionValue)
        {
            var claim = new Claim("permission", permissionValue);
            var claimsIdentity = new ClaimsIdentity(new[] { claim });
            var result = new ClaimsPrincipal(claimsIdentity);

            return result;
        }

        private static AuthorizationHandlerContext CreateAuthorizationHandlerContext(string permission, string permissionValue, object resource)
        {
            var requirements = CreateAuthorizationRequirements(permission);
            var claimsPrincipal = CreateClaimsPtincipal(permissionValue);

            var result = new AuthorizationHandlerContext(requirements, claimsPrincipal, resource);

            return result;
        }

        private static Mock<IStoreService> CreateStoreServiceMock()
        {
            var result = new Mock<IStoreService>();

            return result;
        }

        private CatalogAuthorizationHandler CreateCatalogAuthorizationHandler(IStoreService storeService)
        {
            var result = new CatalogAuthorizationHandler(_jsonOptions, storeService);

            return result;
        }
    }

    //TODO: Remove after adding this converters to VirtoCommerce.Platform.Core package
    public class PolymorphJsonConverter : JsonConverter
    {
        private static readonly Type[] _knowTypes = { typeof(ObjectSettingEntry), typeof(DynamicProperty), typeof(ApplicationUser), typeof(Role), typeof(PermissionScope) };

        public override bool CanWrite => false;
        public override bool CanRead => true;

        public override bool CanConvert(Type objectType)
        {
            return _knowTypes.Any(x => x.IsAssignableFrom(objectType));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            object result;
            var obj = JObject.Load(reader);
            if (typeof(PermissionScope).IsAssignableFrom(objectType))
            {
                var scopeType = objectType.Name;
                var pt = obj["type"] ?? obj["Type"];
                if (pt != null)
                {
                    scopeType = pt.Value<string>();
                }
                result = AbstractTypeFactory<PermissionScope>.TryCreateInstance(scopeType);
                if (result == null)
                {
                    throw new NotSupportedException("Unknown scopeType: " + scopeType);
                }
            }
            else
            {
                var tryCreateInstance = typeof(AbstractTypeFactory<>).MakeGenericType(objectType).GetMethods().FirstOrDefault(x => x.Name.EqualsInvariant("TryCreateInstance") && x.GetParameters().Length == 0);
                result = tryCreateInstance?.Invoke(null, null);
            }

            serializer.Populate(obj.CreateReader(), result);
            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
