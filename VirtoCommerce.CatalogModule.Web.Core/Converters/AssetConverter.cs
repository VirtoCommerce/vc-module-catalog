using System.Web;
using Omu.ValueInjecter;
using VirtoCommerce.Platform.Core.Assets;
using VirtoCommerce.Platform.Core.Common;
using coreModel = VirtoCommerce.Domain.Catalog.Model;
using webModel = VirtoCommerce.CatalogModule.Web.Model;

namespace VirtoCommerce.CatalogModule.Web.Converters
{
    public static class AssetConverter
    {
        public static webModel.Image ToWebModel(this coreModel.Image image, IBlobUrlResolver blobUrlResolver)
        {
            var retVal = new webModel.Image();
            //Do not use omu.InjectFrom for performance reasons 
            retVal.Group = image.Group;
            retVal.Id = image.Id;
            retVal.LanguageCode = image.LanguageCode;
            retVal.Name = image.Name;
            retVal.IsInherited = image.IsInherited;
            retVal.SortOrder = image.SortOrder;
           
            if (blobUrlResolver != null)
            {
                retVal.Url = blobUrlResolver.GetAbsoluteUrl(image.Url);
            }
            retVal.RelativeUrl = image.Url;
            return retVal;
        }

        public static webModel.Asset ToWebModel(this coreModel.Asset asset, IBlobUrlResolver blobUrlResolver)
        {
            var retVal = new webModel.Asset();
            retVal.InjectFrom(asset);
            if (asset.Name == null)
            {
                retVal.Name = HttpUtility.UrlDecode(System.IO.Path.GetFileName(asset.Url));
            }
            if (asset.MimeType == null)
            {
                retVal.MimeType = MimeTypeResolver.ResolveContentType(asset.Name);
            }
            if (blobUrlResolver != null)
            {
                retVal.Url = blobUrlResolver.GetAbsoluteUrl(asset.Url);
            }
            retVal.RelativeUrl = asset.Url;
            retVal.ReadableSize = retVal.Size.ToHumanReadableSize();
            return retVal;
        }

        public static coreModel.Image ToCoreModel(this webModel.Image image)
        {
            var retVal = new coreModel.Image();
            retVal.InjectFrom(image);
            retVal.Url = image.RelativeUrl;
            return retVal;
        }

        public static coreModel.Asset ToCoreModel(this webModel.Asset asset)
        {
            var retVal = new coreModel.Asset();
            retVal.InjectFrom(asset);
            retVal.Url = asset.RelativeUrl;
            return retVal;
        }
    }
}
