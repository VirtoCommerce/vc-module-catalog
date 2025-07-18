using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.ExportModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Seo.Core.Models;

namespace VirtoCommerce.CatalogModule.Core.Model
{
    public class Catalog : AuditableEntity, IHasProperties, IHasOuterId, IExportable, ISeoSupport
    {
        public string Name { get; set; }
        public bool IsVirtual { get; set; }
        public string OuterId { get; set; }

        public CatalogLanguage DefaultLanguage
        {
            get
            {
                CatalogLanguage retVal = null;
                if (Languages != null)
                {
                    retVal = Languages.FirstOrDefault(x => x.IsDefault);
                }
                return retVal;
            }
        }

        public IList<CatalogLanguage> Languages { get; set; }

        #region IHasProperties members
        public IList<Property> Properties { get; set; }
        #endregion

        #region ICloneable members
        public virtual object Clone()
        {
            var result = (Catalog)MemberwiseClone();

            result.SeoInfos = SeoInfos?.Select(x => x.CloneTyped()).ToList();
            result.Languages = Languages?.Select(x => x.CloneTyped()).ToList();
            result.Properties = Properties?.Select(x => x.CloneTyped()).ToList();

            return result;
        }
        #endregion

        public virtual void ReduceDetails(string responseGroup)
        {
            // Reduce details according to response group
            var catalogResponseGroup = EnumUtility.SafeParseFlags(responseGroup, CatalogResponseGroup.Full);

            if (!catalogResponseGroup.HasFlag(CatalogResponseGroup.WithProperties))
            {
                Properties = null;
            }
            if (!catalogResponseGroup.HasFlag(CatalogResponseGroup.WithSeo))
            {
                SeoInfos = null;
            }
        }

        public string SeoObjectType { get { return GetType().Name; } }
        public IList<SeoInfo> SeoInfos { get; set; }

        public IList<PropertyGroup> PropertyGroups { get; set; }
    }
}
