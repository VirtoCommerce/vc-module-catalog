using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dataModel = VirtoCommerce.CatalogModule.Data.Model;
using coreModel = VirtoCommerce.Domain.Catalog.Model;
using Omu.ValueInjecter;

namespace VirtoCommerce.CatalogModule.Data.Converters
{
	public static class LinkConverter
	{
		/// <summary>
		/// Converting to model type
		/// </summary>
		/// <param name="catalogBase"></param>
		/// <returns></returns>
		public static coreModel.CategoryLink ToCoreModel(this dataModel.CategoryItemRelation categoryItemRelation, dataModel.Catalog[] allCatalogs, dataModel.Category[] allCategories)
		{
			if (categoryItemRelation == null)
				throw new ArgumentNullException("categoryItemRelation");

			var retVal = new coreModel.CategoryLink
			{
				 CategoryId = categoryItemRelation.CategoryId,
				 CatalogId = categoryItemRelation.CatalogId,
                 Priority = categoryItemRelation.Priority                 
			};
            retVal.Catalog = allCatalogs.First(x => x.Id == categoryItemRelation.CatalogId).ToCoreModel(false);
            if (categoryItemRelation.CategoryId != null)
            {
                retVal.Category = allCategories.First(x => x.Id == categoryItemRelation.CategoryId)
                                               .ToCoreModel(allCatalogs, allCategories, convertProps: false);
            }
          
            return retVal;
		}

		/// <summary>
		/// Converting to foundation type
		/// </summary>
		/// <param name="catalog"></param>
		/// <returns></returns>
		public static coreModel.CategoryLink ToCoreModel(this dataModel.CategoryRelation linkedCategory, dataModel.Catalog[] allCatalogs, dataModel.Category[] allCategories)
		{
			if (linkedCategory == null)
				throw new ArgumentNullException("linkedCategory");

			var retVal = new coreModel.CategoryLink();
		
			retVal.CategoryId = linkedCategory.TargetCategoryId;
			retVal.CatalogId = linkedCategory.TargetCatalogId;
            retVal.Catalog = allCatalogs.First(x => x.Id == linkedCategory.TargetCatalogId).ToCoreModel(false);
            if (linkedCategory.TargetCategoryId != null)
            {
                retVal.Category = allCategories.First(x => x.Id == linkedCategory.TargetCategoryId)
                                               .ToCoreModel(allCatalogs, allCategories, convertProps: false);
            }
            return retVal;
		}

		/// <summary>
		/// Converting to foundation type
		/// </summary>
		/// <param name="catalog"></param>
		/// <returns></returns>
		public static dataModel.CategoryItemRelation ToDataModel(this coreModel.CategoryLink categoryLink, coreModel.CatalogProduct product)
		{
			var retVal = new dataModel.CategoryItemRelation
			{
				 CategoryId = categoryLink.CategoryId,
				 ItemId = product.Id,
				 CatalogId = categoryLink.CatalogId,
				 Priority = categoryLink.Priority
            };
			return retVal;
		}

		/// <summary>
		/// Converting to foundation type
		/// </summary>
		/// <param name="catalog"></param>
		/// <returns></returns>
		public static dataModel.CategoryRelation ToDataModel(this coreModel.CategoryLink categoryLink, coreModel.Category category)
		{
			var retVal = new dataModel.CategoryRelation
			{
				SourceCategoryId = category.Id,
				TargetCategoryId = categoryLink.CategoryId,
				TargetCatalogId =  categoryLink.CatalogId
			};
			return retVal;
		}

		/// <summary>
		/// Patch LinkedCategory type
		/// </summary>
		/// <param name="source"></param>
		/// <param name="target"></param>
		public static void Patch(this dataModel.CategoryRelation source, dataModel.CategoryRelation target)
		{
			//Nothing todo. Because we not support change  link
		}

		/// <summary>
		/// Patch LinkedCategory type
		/// </summary>
		/// <param name="source"></param>
		/// <param name="target"></param>
		public static void Patch(this dataModel.CategoryItemRelation source, dataModel.CategoryItemRelation target)
		{
			//Nothing todo. Because we not support change link
		}

	}

	public class CategoryItemRelationComparer : IEqualityComparer<dataModel.CategoryItemRelation>
	{
		#region IEqualityComparer<CategoryItemRelation> Members

		public bool Equals(dataModel.CategoryItemRelation x, dataModel.CategoryItemRelation y)
		{
            return GetHashCode(x) == GetHashCode(y);
		}

		public int GetHashCode(dataModel.CategoryItemRelation obj)
		{
            int hash = 17;
            hash = hash * 23 + obj.CatalogId.GetHashCode();
            hash = hash * 23 + obj.Priority.GetHashCode();
            if (obj.CategoryId != null)
            {
                hash = hash * 23 + obj.CategoryId.GetHashCode();
            }
            return hash;
		}

		#endregion
	}

	public class LinkedCategoryComparer : IEqualityComparer<dataModel.CategoryRelation>
	{
		#region IEqualityComparer<LinkedCategory> Members

		public bool Equals(dataModel.CategoryRelation x, dataModel.CategoryRelation y)
		{
            return GetHashCode(x) == GetHashCode(y);
        }

		public int GetHashCode(dataModel.CategoryRelation obj)
		{
            var hash = 17;
            hash = hash * 23 + obj.SourceCategoryId.GetHashCode();
            if (obj.TargetCategoryId != null)
            {
                hash = hash * 23 + obj.TargetCategoryId.GetHashCode();
            }
            else if (obj.TargetCatalogId != null)
            {
                hash = hash * 23 + obj.TargetCatalogId.GetHashCode();
            }
            return hash;     
		}

		#endregion
	}

}
