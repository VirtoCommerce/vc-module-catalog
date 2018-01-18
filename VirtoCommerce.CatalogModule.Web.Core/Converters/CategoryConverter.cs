using System.Collections.Generic;
using System.Linq;
using Omu.ValueInjecter;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.Common;
using moduleModel = VirtoCommerce.Domain.Catalog.Model;
using webModel = VirtoCommerce.CatalogModule.Web.Model;

namespace VirtoCommerce.CatalogModule.Web.Converters
{
    public static class CategoryConverter
    {
        public static webModel.Category ToWebModel(this moduleModel.Category category, IBlobUrlResolver blobUrlResolver = null, bool convertProps = true)
        {
            var retVal = new webModel.Category();
            //Do not use omu.InjectFrom for performance reasons 
            retVal.Id = category.Id;
            retVal.IsActive = category.IsActive;
            retVal.IsVirtual = category.IsVirtual;
            retVal.Name = category.Name;
            retVal.ParentId = category.ParentId;
            retVal.Path = category.Path;
            retVal.TaxType = category.TaxType;
            retVal.CatalogId = category.CatalogId;
            retVal.Code = category.Code;
            retVal.CreatedBy = category.CreatedBy;
            retVal.CreatedDate = category.CreatedDate;
            retVal.ModifiedBy = category.ModifiedBy;
            retVal.ModifiedDate = category.ModifiedDate;
      
            retVal.SeoInfos = category.SeoInfos;
            if (!category.Outlines.IsNullOrEmpty())
            {
                //Minimize outline size
                retVal.Outlines = category.Outlines.Select(x => x.ToWebModel()).ToList();
            }

            //Init outline and path
            var parents = new List<moduleModel.Category>();
            if (category.Parents != null)
            {
                retVal.Outline = category.Outlines.FirstOrDefault().ToString();
                retVal.Path = string.Join("/", category.Parents.Select(x => x.Name));
            }

            //For virtual category links not needed
            if (!category.IsVirtual && category.Links != null)
            {
                retVal.Links = category.Links.Select(x => x.ToWebModel()).ToList();
            }

            //Need add property for each meta info
            retVal.Properties = new List<webModel.Property>();
            if (convertProps)
            {
                if (!category.Properties.IsNullOrEmpty())
                {
                    foreach (var property in category.Properties)
                    {
                        var webModelProperty = property.ToWebModel();
                        //Reset dict values to decrease response size
                        webModelProperty.DictionaryValues = null;
                        webModelProperty.Values = new List<webModel.PropertyValue>();
                        webModelProperty.IsManageable = true;
                        webModelProperty.IsReadOnly = property.Type != moduleModel.PropertyType.Category;
                        retVal.Properties.Add(webModelProperty);
                    }
                }

                //Populate property values
                if (category.PropertyValues != null)
                {
                    foreach (var propValue in category.PropertyValues.Select(x => x.ToWebModel()))
                    {
                        var property = retVal.Properties.FirstOrDefault(x => x.Id == propValue.PropertyId);
                        if (property == null)
                        {
                            property = retVal.Properties.FirstOrDefault(x => x.Name.EqualsInvariant(propValue.PropertyName));
                        }
                        if (property == null)
                        {
                            //Need add dummy property for each value without property
                            property = new webModel.Property(propValue, category.CatalogId, moduleModel.PropertyType.Category);
                            retVal.Properties.Add(property);
                        }
                        property.Values.Add(propValue);
                    }
                }
            }

            if (category.Images != null)
            {
                retVal.Images = category.Images.Select(x => x.ToWebModel(blobUrlResolver)).ToList();
            }

            return retVal;
        }

        public static moduleModel.Category ToModuleModel(this webModel.Category category)
        {
            var retVal = new moduleModel.Category();
            retVal.InjectFrom(category);
            retVal.SeoInfos = category.SeoInfos;

            if (category.Links != null)
            {
                retVal.Links = category.Links.Select(x => x.ToCoreModel()).ToList();
            }

            if (category.Properties != null)
            {
                retVal.PropertyValues = new List<moduleModel.PropertyValue>();
                foreach (var property in category.Properties)
                {
                    foreach (var propValue in property.Values)
                    {
                        propValue.ValueType = property.ValueType;
                        //Need populate required fields
                        propValue.PropertyId = property.Id;
                        propValue.PropertyName = property.Name;
                        retVal.PropertyValues.Add(propValue.ToCoreModel());
                    }
                }
            }

            if (category.Images != null)
            {
                retVal.Images = category.Images.Select(x => x.ToCoreModel()).ToList();
            }

            return retVal;
        }
    }
}
