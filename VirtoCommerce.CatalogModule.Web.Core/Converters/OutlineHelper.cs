using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Domain.Catalog.Model;

namespace VirtoCommerce.CatalogModule.Web.Converters
{
    public static class OutlineHelper
    {
        public static string[] EnsurePhysicalOutline(ICollection<string> rawOutlines, string catalogId)
        {
            var retVal = new List<string>(rawOutlines.Count);
            foreach (var rawOutline in rawOutlines)
            {
                var outlineParts = rawOutline.Split('/').ToList();
                // Add the catalog to the outline if it is not present.
                if (!outlineParts.First().Equals(catalogId))
                {
                    outlineParts.Insert(0, catalogId);
                }

                var outline = string.Join("/", outlineParts.ToArray()).ToLowerInvariant();
                if (!retVal.Contains(outline))
                {
                    retVal.Add(outline);
                }
            }

            return retVal.ToArray();
        }

        public static IEnumerable<string> EnsurePhysicalOutline(ICollection<Outline> outlines, string catalogId)
        {
            var x = outlines.Select(ol => ol.ToString()).ToArray();
            return EnsurePhysicalOutline(x, catalogId);
        }
    }
}
