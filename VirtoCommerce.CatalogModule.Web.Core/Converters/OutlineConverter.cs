using System.Collections.Generic;
using System.Linq;
using Omu.ValueInjecter;
using VirtoCommerce.Domain.Commerce.Model;
using moduleModel = VirtoCommerce.Domain.Catalog.Model;
using webModel = VirtoCommerce.CatalogModule.Web.Model;

namespace VirtoCommerce.CatalogModule.Web.Converters
{
    public static class OutlineConverter
    {
        public static moduleModel.Outline ToWebModel(this moduleModel.Outline outline)
        {
            var result = new moduleModel.Outline();
            result.Items = new List<moduleModel.OutlineItem>();
            foreach (var outlineItem in outline.Items)
            {
                var newOutlineItem = new moduleModel.OutlineItem();
                newOutlineItem.Id = outlineItem.Id;
                newOutlineItem.HasVirtualParent = outlineItem.HasVirtualParent;
                newOutlineItem.SeoObjectType = outlineItem.SeoObjectType;
                if (outlineItem.SeoInfos != null)
                {
                    newOutlineItem.SeoInfos = outlineItem.SeoInfos.Select(x => new SeoInfo { IsActive = x.IsActive, LanguageCode = x.LanguageCode, SemanticUrl = x.SemanticUrl, StoreId = x.StoreId }).ToList();
                }
                result.Items.Add(newOutlineItem);
            }
            return result;
        }
    }
}
