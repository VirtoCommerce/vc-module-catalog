using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dataModel = VirtoCommerce.CatalogModule.Data.Model;
using coreModel = VirtoCommerce.Domain.Catalog.Model;
using Omu.ValueInjecter;
using VirtoCommerce.Platform.Data.Common;
using VirtoCommerce.Platform.Data.Common.ConventionInjections;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Converters
{
	public static class AssetConverter
	{
		/// <summary>
		/// Converting to model type
		/// </summary>
		/// <param name="catalogBase"></param>
		/// <returns></returns>
		public static coreModel.Image ToCoreModel(this dataModel.ImageEntity dbImage)
		{
			if (dbImage == null)
				throw new ArgumentNullException("dbImage");

			var retVal = new coreModel.Image();
            //Do not use omu.ValueInjector for performance purposes
            retVal.CreatedBy = dbImage.CreatedBy;
            retVal.CreatedDate = dbImage.CreatedDate;
            retVal.Id = dbImage.Id;
            retVal.Group = dbImage.Group;
            retVal.LanguageCode = dbImage.LanguageCode;
            retVal.ModifiedBy = dbImage.ModifiedBy;
            retVal.ModifiedDate = dbImage.ModifiedDate;
            retVal.Name = dbImage.Name;
            retVal.SortOrder = dbImage.SortOrder;
            retVal.Url = dbImage.Url;

			return retVal;

		}

		/// <summary>
		/// Converting to model type
		/// </summary>
		/// <param name="catalogBase"></param>
		/// <returns></returns>
		public static coreModel.Asset ToCoreModel(this dataModel.AssetEntity dbAsset)
		{
			if (dbAsset == null)
				throw new ArgumentNullException("dbAsset");

			var retVal = new coreModel.Asset();

			retVal.InjectFrom(dbAsset);

			return retVal;

		}
		/// <summary>
		/// Converting to foundation type
		/// </summary>
		public static dataModel.ImageEntity ToDataModel(this coreModel.Image image, PrimaryKeyResolvingMap pkMap)
		{
			if (image == null)
				throw new ArgumentNullException("image");

			var retVal = new dataModel.ImageEntity();
            pkMap.AddPair(image, retVal);
            retVal.InjectFrom(image);
	
			return retVal;
		}

		/// <summary>
		/// Converting to foundation type
		/// </summary>
		public static dataModel.AssetEntity ToDataModel(this coreModel.Asset asset, PrimaryKeyResolvingMap pkMap)
		{
			if (asset == null)
				throw new ArgumentNullException("asset");

			var retVal = new dataModel.AssetEntity();
            pkMap.AddPair(asset, retVal);
            retVal.InjectFrom(asset);
			return retVal;
		}

		/// <summary>
		/// Patch changes
		/// </summary>
		/// <param name="source"></param>
		/// <param name="target"></param>
		public static void Patch(this dataModel.AssetEntity source, dataModel.AssetEntity target)
		{
			if (target == null)
				throw new ArgumentNullException("target");
			
			var patchInjectionPolicy = new PatchInjection<dataModel.AssetEntity>(x => x.LanguageCode, x=> x.Name );
			target.InjectFrom(patchInjectionPolicy, source);

		}

		/// <summary>
		/// Patch changes
		/// </summary>
		/// <param name="source"></param>
		/// <param name="target"></param>
		public static void Patch(this dataModel.ImageEntity source, dataModel.ImageEntity target)
		{
			if (target == null)
				throw new ArgumentNullException("target");

			var patchInjectionPolicy = new PatchInjection<dataModel.ImageEntity>(x => x.LanguageCode, x => x.Name, x => x.SortOrder);
			target.InjectFrom(patchInjectionPolicy, source);

		}
	}
}
