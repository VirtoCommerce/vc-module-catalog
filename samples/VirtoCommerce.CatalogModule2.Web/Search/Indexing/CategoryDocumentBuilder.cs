using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Search.Indexing;
using VirtoCommerce.CoreModule.Core.Outlines;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.CatalogModule2.Data.Search.Indexing
{
    public class CategoryDocumentBuilder2 : CategoryDocumentBuilder
    {
        public CategoryDocumentBuilder2(ISettingsManager settingsManager, ICategoryService categoryService) : base(settingsManager, categoryService)
        {
        }
        protected override IndexDocument CreateDocument(Category category)
        {
            return base.CreateDocument(category);
        }
        protected override IEnumerable<string> ExpandOutline(Outline outline, bool getNameLatestItem)
        {
            return base.ExpandOutline(outline, getNameLatestItem);
        }
        protected override Task<Category[]> GetCategories(IList<string> categoryIds)
        {
            return base.GetCategories(categoryIds);
        }
        public override Task<IList<IndexDocument>> GetDocumentsAsync(IList<string> documentIds)
        {
            return base.GetDocumentsAsync(documentIds);
        }
        protected override string[] GetOutlineStrings(IEnumerable<Outline> outlines, bool getNameLatestItem = false)
        {
            return base.GetOutlineStrings(outlines, getNameLatestItem);
        }
        protected override void IndexCustomProperties(IndexDocument document, ICollection<Property> properties, ICollection<PropertyType> contentPropertyTypes)
        {
            base.IndexCustomProperties(document, properties, contentPropertyTypes);
        }
        protected override void IndexIsProperty(IndexDocument document, string value)
        {
            base.IndexIsProperty(document, value);
        }
        protected override bool IsActiveInTotal(Category category)
        {
            return base.IsActiveInTotal(category);
        }
    }
}
