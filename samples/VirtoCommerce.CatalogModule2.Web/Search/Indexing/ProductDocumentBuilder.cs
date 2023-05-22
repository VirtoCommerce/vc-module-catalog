using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Search.Indexing;
using VirtoCommerce.CoreModule.Core.Outlines;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.CatalogModule2.Data.Search.Indexing
{
    public class ProductDocumentBuilder2 : ProductDocumentBuilder
    {
        public ProductDocumentBuilder2(ISettingsManager settingsManager, IItemService itemService, IProductSearchService productsSearchService) : base(settingsManager, itemService, productsSearchService)
        {
        }
        protected override IndexDocument CreateDocument(CatalogProduct product)
        {
            return base.CreateDocument(product);
        }
        protected override IEnumerable<string> ExpandOutline(Outline outline, bool getNameLatestItem)
        {
            return base.ExpandOutline(outline, getNameLatestItem);
        }
        public override Task<IList<IndexDocument>> GetDocumentsAsync(IList<string> documentIds)
        {
            return base.GetDocumentsAsync(documentIds);
        }
        protected override string[] GetOutlineStrings(IEnumerable<Outline> outlines, bool getNameLatestItem = false)
        {
            return base.GetOutlineStrings(outlines, getNameLatestItem);
        }
        protected override Task<CatalogProduct[]> GetProducts(IList<string> productIds)
        {
            return base.GetProducts(productIds);
        }
        protected override void IndexCustomProperties(IndexDocument document, ICollection<Property> properties, ICollection<PropertyType> contentPropertyTypes)
        {
            base.IndexCustomProperties(document, properties, contentPropertyTypes);
        }
        protected override void IndexIsProperty(IndexDocument document, string value)
        {
            base.IndexIsProperty(document, value);
        }
        protected override void IndexProductVariation(IndexDocument document, CatalogProduct variation)
        {
            base.IndexProductVariation(document, variation);
        }
    }
}
