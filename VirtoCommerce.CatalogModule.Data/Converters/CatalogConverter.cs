using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dataModel = VirtoCommerce.CatalogModule.Data.Model;
using coreModel = VirtoCommerce.Domain.Catalog.Model;
using System.Collections.ObjectModel;
using VirtoCommerce.Platform.Core.Common;
using Omu.ValueInjecter;
using VirtoCommerce.Platform.Data.Common;
using VirtoCommerce.Platform.Data.Common.ConventionInjections;
using VirtoCommerce.Domain.Commerce.Model;

namespace VirtoCommerce.CatalogModule.Data.Converters
{
    public static class CatalogConverter
    {
        /// <summary>
        /// Converting to model type
        /// </summary>
        /// <param name="catalogBase"></param>
        /// <returns></returns>
        public static coreModel.Catalog ToCoreModel(this dataModel.Catalog dbCatalog, bool convertProps = true)
        {
            if (dbCatalog == null)
                throw new ArgumentNullException("catalog");

            var retVal = new coreModel.Catalog();
            retVal.Id = dbCatalog.Id;
            retVal.Name = dbCatalog.Name;           
            retVal.IsVirtual = dbCatalog.Virtual;
            retVal.Languages = new List<coreModel.CatalogLanguage>();

            var defaultLanguage = (new dataModel.CatalogLanguage { Language = string.IsNullOrEmpty(dbCatalog.DefaultLanguage) ? "en-us" : dbCatalog.DefaultLanguage }).ToCoreModel(retVal);
            defaultLanguage.IsDefault = true;
            retVal.Languages = new List<coreModel.CatalogLanguage>();
            retVal.Languages.Add(defaultLanguage);

            //populate additional languages
            foreach (var catalogLanguage in dbCatalog.CatalogLanguages.Where(x => x.Language != defaultLanguage.LanguageCode).Select(x => x.ToCoreModel(retVal)))
            {
                catalogLanguage.Catalog = retVal;
                retVal.Languages.Add(catalogLanguage);
            }


            if (convertProps)
            {
                retVal.PropertyValues = dbCatalog.CatalogPropertyValues.Select(x => x.ToCoreModel()).ToList();
                //Self properties
                retVal.Properties = dbCatalog.Properties.Where(x => x.CategoryId == null).OrderBy(x => x.Name).Select(x => x.ToCoreModel(new dataModel.Catalog[] { dbCatalog }, new dataModel.Category[] { })).ToList();

                //Next need set Property in PropertyValues objects
                foreach (var propValue in retVal.PropertyValues.ToArray())
                {
                    propValue.Property = retVal.Properties.FirstOrDefault(x => x.IsSuitableForValue(propValue));
                    //Return each localized value for selecte dictionary value
                    //Because multilingual dictionary values for all languages may not stored in db need add it in result manually from property dictionary values
                    var localizedDictValues = propValue.TryGetAllLocalizedDictValues();
                    foreach (var localizedDictValue in localizedDictValues)
                    {
                        if (!retVal.PropertyValues.Any(x => x.ValueId == localizedDictValue.ValueId && x.LanguageCode == localizedDictValue.LanguageCode))
                        {
                            retVal.PropertyValues.Add(localizedDictValue);
                        }
                    }
                }
            }

            return retVal;
        }

        /// <summary>
        /// Converting to foundation type
        /// </summary>
        /// <param name="catalog"></param>
        /// <returns></returns>
        public static dataModel.Catalog ToDataModel(this coreModel.Catalog catalog, PrimaryKeyResolvingMap pkMap)
        {
            if (catalog == null)
                throw new ArgumentNullException("catalog");

            if (catalog.DefaultLanguage == null)
                throw new NullReferenceException("DefaultLanguage");

            var retVal = new dataModel.Catalog();
            pkMap.AddPair(catalog, retVal);

            if (catalog.PropertyValues != null)
            {
                retVal.CatalogPropertyValues = new ObservableCollection<dataModel.PropertyValue>();
                retVal.CatalogPropertyValues.AddRange(catalog.PropertyValues.Select(x => x.ToDataModel(pkMap)));
            }

            retVal.InjectFrom(catalog);
            retVal.Virtual = catalog.IsVirtual;

            retVal.DefaultLanguage = catalog.DefaultLanguage.LanguageCode;

            if (catalog.Languages != null)
            {
                retVal.CatalogLanguages = new ObservableCollection<dataModel.CatalogLanguage>();
                retVal.CatalogLanguages.AddRange(catalog.Languages.Select(x => x.ToDataModel()));
            }

            return retVal;
        }


        /// <summary>
        /// Patch CatalogBase type
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public static void Patch(this dataModel.Catalog source, dataModel.Catalog target)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            target.Name = source.Name;
            target.DefaultLanguage = source.DefaultLanguage;

            //Languages patch
            if (!source.CatalogLanguages.IsNullCollection())
            {
                var languageComparer = AnonymousComparer.Create((dataModel.CatalogLanguage x) => x.Language);
                source.CatalogLanguages.Patch(target.CatalogLanguages, languageComparer,
                                                     (sourceLang, targetlang) => sourceLang.Patch(targetlang));
            }

            //Property values
            if (!source.CatalogPropertyValues.IsNullCollection())
            {
                source.CatalogPropertyValues.Patch(target.CatalogPropertyValues, (sourcePropValue, targetPropValue) => sourcePropValue.Patch(targetPropValue));
            }


        }

    }


}
