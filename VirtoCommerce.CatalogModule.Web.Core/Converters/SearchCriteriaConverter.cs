using System.Web;
using Omu.ValueInjecter;
using System.Linq;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.Common;
using coreModel = VirtoCommerce.Domain.Catalog.Model;
using webModel = VirtoCommerce.CatalogModule.Web.Model;

namespace VirtoCommerce.CatalogModule.Web.Converters
{
    public static class SearchCriteriaConverter
    {
        public static coreModel.SearchCriteria ToCoreModel(this webModel.SearchCriteria criteria)
        {
            var retVal = new coreModel.SearchCriteria();

            retVal.InjectFrom(criteria);

            retVal.ResponseGroup = criteria.ResponseGroup;
            retVal.CategoryIds = criteria.CategoryIds;
            retVal.CatalogIds = criteria.CatalogIds;
            retVal.PricelistIds = criteria.PricelistIds;
            retVal.Terms = criteria.Terms;
            retVal.Facets = criteria.Facets;
            retVal.ProductTypes = criteria.ProductTypes;
            retVal.VendorIds = criteria.VendorIds;

            if(!criteria.PropertyValues.IsNullOrEmpty())
            {
                retVal.PropertyValues = criteria.PropertyValues.Select(x => x.ToCoreModel()).ToArray();
            }

            return retVal;
      
        }     
    }
}
