using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Authorization;
using VirtoCommerce.CatalogModule.Web.Authorization;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Seo.Core.Models;
using Permissions = VirtoCommerce.CatalogModule.Core.ModuleConstants.Security.Permissions;

namespace VirtoCommerce.CatalogModule.Web.Controllers.Api
{
    [Route("api/catalog/products")]
    [Authorize]
    public class CatalogModuleProductsController(
        IItemService itemsService,
        ICategoryService categoryService,
        ICatalogService catalogService,
        ISkuGenerator skuGenerator,
        CatalogEntityAuthorizationService authorizationService,
        IPropertyUpdateManager updateManager,
        IOptions<MvcNewtonsoftJsonOptions> jsonOptions)
        : Controller
    {
        /// <summary>
        /// Gets product by id.
        /// </summary>
        /// <param name="id">Item id.</param>
        ///<param name="respGroup">Response group.</param>
        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<CatalogProduct>> GetProductById([FromRoute] string id, [FromQuery] string respGroup = null)
        {
            var product = await itemsService.GetNoCloneAsync(id, respGroup);
            if (product == null)
            {
                return NotFound();
            }

            if (!await authorizationService.AuthorizeAsync(User, product, Permissions.ProductsRead))
            {
                return Forbid();
            }

            return Ok(product);
        }

        /// <summary>
        /// Gets product by outer id.
        /// </summary>
        /// <remarks>Gets product by outer id (integration key) with full information loaded</remarks>
        /// <param name="outerId">Product outer id</param>
        /// <param name="responseGroup">>Response group</param>
        [HttpGet]
        [Route("outer/{outerId}")]
        public async Task<ActionResult<Catalog>> GetProductByOuterId([FromRoute] string outerId, [FromQuery] string responseGroup = null)
        {
            var product = await itemsService.GetByOuterIdNoCloneAsync(outerId, responseGroup);
            if (product == null)
            {
                return NotFound();
            }

            if (!await authorizationService.AuthorizeAsync(User, product, Permissions.ProductsRead))
            {
                return Forbid();
            }

            return Ok(product);
        }

        [HttpPost("~/api/catalog/{catalogId}/products-by-codes")]
        public async Task<ActionResult<CatalogProduct[]>> GetByCodes([FromRoute] string catalogId, [FromBody] List<string> codes, [FromQuery] string responseGroup)
        {
            var idsByCodes = await itemsService.GetIdsByCodes(catalogId, codes);

            return await GetProductByIds([.. idsByCodes.Values], responseGroup);
        }

        /// <summary>
        /// Gets products by ids
        /// </summary>
        /// <param name="ids">Item ids</param>
        ///<param name="respGroup">Response group.</param>
        [HttpGet]
        [Route("")]
        public async Task<ActionResult<CatalogProduct[]>> GetProductByIds([FromQuery] List<string> ids, [FromQuery] string respGroup = null)
        {
            var items = await itemsService.GetNoCloneAsync(ids, respGroup);
            if (items == null)
            {
                return NotFound();
            }

            if (!await authorizationService.AuthorizeAsync(User, items, Permissions.ProductsRead))
            {
                return Forbid();
            }
            //It is important to return serialized data by such way. Otherwise, you will have slow responses for large outputs.
            //https://github.com/dotnet/aspnetcore/issues/19646
            var result = JsonConvert.SerializeObject(items, jsonOptions.Value.SerializerSettings);

            return Content(result, "application/json");
        }

        /// <summary>
        /// Gets products by plenty ids
        /// </summary>
        /// <param name="ids">Item ids</param>
        /// <param name="respGroup">Response group.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("plenty")]
        public Task<ActionResult<CatalogProduct[]>> GetProductByPlentyIds([FromBody] List<string> ids, [FromQuery] string respGroup = null)
        {
            return GetProductByIds(ids, respGroup);
        }


        /// <summary>
        /// Gets the template for a new product (outside of category).
        /// </summary>
        /// <remarks>Use when need to create item belonging to catalog directly.</remarks>
        /// <param name="catalogId">The catalog id.</param>
        [HttpGet]
        [Route("~/api/catalog/{catalogId}/products/getnew")]
        public Task<ActionResult<CatalogProduct>> GetNewProductByCatalog(string catalogId)
        {
            return GetNewProductByCatalogAndCategory(catalogId, null);
        }


        /// <summary>
        /// Gets the template for a new product (inside category).
        /// </summary>
        /// <remarks>Use when need to create item belonging to catalog category.</remarks>
        /// <param name="catalogId">The catalog id.</param>
        /// <param name="categoryId">The category id.</param>
        [HttpGet]
        [Route("~/api/catalog/{catalogId}/categories/{categoryId}/products/getnew")]
        public async Task<ActionResult<CatalogProduct>> GetNewProductByCatalogAndCategory(string catalogId, string categoryId)
        {
            var result = AbstractTypeFactory<CatalogProduct>.TryCreateInstance();
            result.CategoryId = categoryId;
            result.CatalogId = catalogId;
            result.IsActive = true;
            result.SeoInfos = Array.Empty<SeoInfo>();

            Entity parent = null;
            if (catalogId != null)
            {
                parent = await catalogService.GetByIdAsync(catalogId);
            }
            if (categoryId != null)
            {
                parent = await categoryService.GetByIdAsync(categoryId, nameof(CategoryResponseGroup.WithProperties));
            }
            if (parent != null)
            {
                result.TryInheritFrom(parent);
            }

            if (result.Properties != null)
            {
                foreach (var property in result.Properties)
                {
                    property.Values = [];
                    property.IsReadOnly = property.Type != PropertyType.Product && property.Type != PropertyType.Variation;
                }
            }
            result.Code = skuGenerator.GenerateSku(result);

            return Ok(result);
        }


        /// <summary>
        /// Gets the template for a new variation.
        /// </summary>
        /// <param name="productId">The parent product id.</param>
        [HttpGet]
        [Route("{productId}/getnewvariation")]
        public async Task<ActionResult<CatalogProduct>> GetNewVariation(string productId)
        {
            var product = await itemsService.GetByIdAsync(productId);
            if (product == null)
            {
                return NotFound();
            }

            var newVariation = AbstractTypeFactory<CatalogProduct>.TryCreateInstance();

            newVariation.Name = product.Name;
            newVariation.CategoryId = product.CategoryId;
            newVariation.CatalogId = product.CatalogId;
            newVariation.MainProductId = product.MainProductId ?? productId;
            newVariation.Properties = product.Properties.Where(x => x.Type == PropertyType.Variation).ToList();

            foreach (var property in newVariation.Properties)
            {
                // Mark variation property as required
                if (property.Type == PropertyType.Variation)
                {
                    property.Required = true;
                    property.Values.Clear();
                }
            }
            newVariation.Code = skuGenerator.GenerateSku(newVariation);
            return Ok(newVariation);
        }

        [HttpGet]
        [Route("{productId}/clone")]
        public async Task<ActionResult<CatalogProduct>> CloneProduct(string productId)
        {
            var product = await itemsService.GetByIdAsync(productId);
            if (product == null)
            {
                return NotFound();
            }

            var copyProduct = (CatalogProduct)product.GetCopy();

            // Reset
            copyProduct.Id = null;
            copyProduct.CreatedDate = DateTime.UtcNow;
            copyProduct.CreatedBy = null;
            copyProduct.ModifiedDate = null;
            copyProduct.ModifiedBy = null;

            // Generate new SKUs and remove SEO records for product and its variations
            copyProduct.Code = skuGenerator.GenerateSku(product);
            copyProduct.SeoInfos.Clear();

            foreach (var variation in copyProduct.Variations)
            {
                variation.Id = null;
                variation.CreatedDate = DateTime.UtcNow;
                variation.CreatedBy = null;
                variation.ModifiedDate = null;
                variation.ModifiedBy = null;
                variation.Code = skuGenerator.GenerateSku(variation);
                variation.SeoInfos.Clear();
            }

            return Ok(copyProduct);
        }

        /// <summary>
        /// Updates only given properties
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <param name="language">Language for properties to change</param>
        /// <param name="productPatch">JSON object in a key-value format</param>
        /// <returns></returns>
        [HttpPatch]
        [Route("{productId}/{language}")]
        public async Task<ActionResult<CatalogProduct>> ProductPartialUpdate(string productId, string language, [FromBody] JObject productPatch)
        {
            var product = await itemsService.GetByIdAsync(productId);
            if (product == null)
            {
                return NotFound();
            }

            if (!await authorizationService.AuthorizeAsync(User, product, Permissions.ProductsUpdate))
            {
                return Forbid();
            }

            var result = await updateManager.TryChangeProductPropertyValues(product, productPatch, language);

            if (!result.Succeeded)
            {
                return BadRequest(result);
            }

            var existingProductIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { product.Id };
            await InnerSaveProducts([product], existingProductIds);

            return Ok(product);
        }

        /// <summary>
        /// Create/Update the specified product.
        /// </summary>
        /// <param name="product">The product.</param>
        [HttpPost]
        [Route("")]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        public async Task<ActionResult<CatalogProduct>> SaveProduct([FromBody] CatalogProduct product)
        {
            var productExists = await ProductExistsAsync(product);

            var permission = productExists
                ? Permissions.ProductsUpdate
                : Permissions.ProductsCreate;

            if (!await authorizationService.AuthorizeAsync(User, product, permission) ||
                !await authorizationService.AuthorizeAsync(User, product, new CustomPropertyRequirement()))
            {
                return Forbid();
            }

            var existingProductIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (productExists)
            {
                existingProductIds.Add(product.Id);
            }

            var result = (await InnerSaveProducts([product], existingProductIds)).FirstOrDefault();
            if (result != null)
            {
                return Ok(result);
            }
            return NoContent();
        }

        /// <summary>
        /// Create/Update the specified products.
        /// </summary>
        /// <param name="products">The products.</param>
        [HttpPost]
        [Route("batch")]
        public async Task<ActionResult> SaveProducts([FromBody] CatalogProduct[] products)
        {
            var existingProductIds = await GetExistingProductIdsAsync(products);

            var newProducts = products
                .Where(x => x.Id.IsNullOrEmpty() || !existingProductIds.Contains(x.Id))
                .ToArray();

            if (newProducts.Length > 0 &&
                !await authorizationService.AuthorizeAsync(User, newProducts, Permissions.ProductsCreate))
            {
                return Forbid();
            }

            var existingProducts = products
                .Where(x => !x.Id.IsNullOrEmpty() && existingProductIds.Contains(x.Id))
                .ToArray();

            if (existingProducts.Length > 0 &&
                !await authorizationService.AuthorizeAsync(User, existingProducts, Permissions.ProductsUpdate))
            {
                return Forbid();
            }

            if (!await authorizationService.AuthorizeAsync(User, products, new CustomPropertyRequirement()))
            {
                return Forbid();
            }

            await InnerSaveProducts(products, existingProductIds);

            return Ok();
        }


        /// <summary>
        /// Deletes the specified items by id.
        /// </summary>
        /// <param name="ids">The items ids.</param>
        [HttpDelete]
        [Route("")]
        public async Task<ActionResult> DeleteProduct([FromQuery] List<string> ids)
        {
            var products = await itemsService.GetNoCloneAsync(ids, nameof(ItemResponseGroup.ItemInfo));

            if (!await authorizationService.AuthorizeAsync(User, products, Permissions.ProductsDelete))
            {
                return Forbid();
            }

            await itemsService.DeleteAsync(ids);
            return Ok();
        }

        /// <summary>
        /// Partial update for the specified Product by id
        /// </summary>
        /// <param name="id">Product id</param>
        /// <param name="patchDocument">JsonPatchDocument object with fields to update</param>
        /// <returns></returns>
        [HttpPatch]
        [Route("{id}")]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> PatchProduct(string id, [FromBody] JsonPatchDocument<CatalogProduct> patchDocument)
        {
            if (patchDocument == null)
            {
                return BadRequest();
            }

            var product = await itemsService.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            if (!await authorizationService.AuthorizeAsync(User, product, Permissions.ProductsUpdate) ||
                !await authorizationService.AuthorizeAsync(User, product, new CustomPropertyRequirement()))
            {
                return Forbid();
            }

            patchDocument.ApplyTo(product, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await InnerSaveProducts([product]);

            return NoContent();
        }

        private async Task<CatalogProduct[]> InnerSaveProducts(CatalogProduct[] products, HashSet<string> existingProductIds = null)
        {
            var toSaveList = new List<CatalogProduct>();
            existingProductIds ??= await GetExistingProductIdsAsync(products);

            var catalogs = await catalogService.GetNoCloneAsync(products.Select(pr => pr.CatalogId).Distinct().ToList());
            foreach (var product in products)
            {
                if (!product.Name.IsNullOrEmpty())
                {
                    product.Name = product.Name.Trim();
                }

                var isNewProduct = product.Id.IsNullOrEmpty() || !existingProductIds.Contains(product.Id);
                if (isNewProduct && product.SeoInfos.IsNullOrEmpty())
                {
                    var slugUrl = GenerateProductDefaultSlugUrl(product);
                    if (!slugUrl.IsNullOrEmpty())
                    {
                        var catalog = catalogs.FirstOrDefault(x => x.Id.EqualsIgnoreCase(product.CatalogId));
                        var defaultLanguageCode = catalog?.Languages.First(x => x.IsDefault).LanguageCode;
                        var seoInfo = AbstractTypeFactory<SeoInfo>.TryCreateInstance();
                        seoInfo.LanguageCode = defaultLanguageCode;
                        seoInfo.SemanticUrl = slugUrl;
                        product.SeoInfos = [seoInfo];
                    }
                }

                toSaveList.Add(product);
            }

            if (!toSaveList.IsNullOrEmpty())
            {
                await itemsService.SaveChangesAsync([.. toSaveList]);
            }

            return [.. toSaveList];
        }

        private async Task<bool> ProductExistsAsync(CatalogProduct product)
        {
            return !product.Id.IsNullOrEmpty() &&
                   (await GetExistingProductIdsAsync([product])).Contains(product.Id);
        }

        private async Task<HashSet<string>> GetExistingProductIdsAsync(IList<CatalogProduct> products)
        {
            var ids = products
                .Where(x => !x.Id.IsNullOrEmpty())
                .Select(x => x.Id)
                .DistinctIgnoreCase()
                .ToArray();

            if (ids.Length == 0)
            {
                return [];
            }

            var existingProducts = await itemsService.GetNoCloneAsync(ids, nameof(ItemResponseGroup.ItemInfo));

            return existingProducts
                .Select(x => x.Id)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        private string GenerateProductDefaultSlugUrl(CatalogProduct product)
        {
            var retVal = new List<string>
            {
                product.Name
            };
            if (product.Properties != null)
            {
                //foreach (var property in product.Properties.Where(x => x.Type == PropertyType.Variation && x.Values != null))
                //{
                //    retVal.AddRange(property.Values.Select(x => x.PropertyName + "-" + x.Value));
                //}
            }
            return string.Join(" ", retVal).GenerateSlug();
        }
    }
}
