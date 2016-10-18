using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dataModel = VirtoCommerce.CatalogModule.Data.Model;
using coreModel = VirtoCommerce.Domain.Catalog.Model;
using Omu.ValueInjecter;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Common;
using VirtoCommerce.Platform.Data.Common.ConventionInjections;

namespace VirtoCommerce.CatalogModule.Data.Converters
{
	public static class ProductAssociationConverter
	{
        /// <summary>
        /// Converting to model type
        /// </summary>
        /// <param name="dbAssociation"></param>
        /// <returns></returns>
        public static coreModel.ProductAssociation ToCoreModel(this dataModel.Association dbAssociation)
		{
			if (dbAssociation == null)
				throw new ArgumentNullException("dbAssociation");

            var retVal = new coreModel.ProductAssociation
            {
                Type = dbAssociation.AssociationType,
                Priority = dbAssociation.Priority,
                AssociatedObjectId = dbAssociation.AssociatedItemId ?? dbAssociation.AssociatedCategoryId,
                Quantity = dbAssociation.Quantity
            };
            if(dbAssociation.AssociatedCategory != null)
            {
                retVal.AssociatedObject = dbAssociation.AssociatedCategory.ToCoreModel();
                retVal.AssociatedObjectType = "category";
            }
            if (dbAssociation.AssociatedItem != null)
            {
                //Need to remove associations in associated product to prevent StackOverflow in converter
                dbAssociation.AssociatedItem.Associations = new NullCollection<dataModel.Association>();
                retVal.AssociatedObject = dbAssociation.AssociatedItem.ToCoreModel();
                retVal.AssociatedObjectType = "product";
            }
            if(!dbAssociation.Tags.IsNullOrEmpty())
            {
                retVal.Tags = dbAssociation.Tags.Split(';');
            }
            return retVal;
		}



        /// <summary>
        /// Converting to foundation type
        /// </summary>
        /// <param name="association"></param>
        /// <returns></returns>
        public static dataModel.Association ToDataModel(this coreModel.ProductAssociation association)
		{
			if (association == null)
				throw new ArgumentNullException("association");

			var retVal = new dataModel.Association
			{   
                Priority = association.Priority,
				AssociationType = association.Type,
                Quantity = association.Quantity
			};

            if(association.AssociatedObjectType.EqualsInvariant("product"))
            {
                retVal.AssociatedItemId = association.AssociatedObjectId;
            }
            else if(association.AssociatedObjectType.EqualsInvariant("category"))
            {
                retVal.AssociatedCategoryId = association.AssociatedObjectId;
            }

            if(!association.Tags.IsNullOrEmpty())
            {
                retVal.Tags = string.Join(";", association.Tags);
            }
            return retVal;
		}

	
		
		/// <summary>
		/// Patch Association type
		/// </summary>
		/// <param name="source"></param>
		/// <param name="target"></param>
		public static void Patch(this dataModel.Association source, dataModel.Association target)
		{
            target.Priority = source.Priority;
            target.Tags = source.Tags;
            target.AssociationType = source.AssociationType;
		    target.Quantity = source.Quantity;
		}
	}

	
}
