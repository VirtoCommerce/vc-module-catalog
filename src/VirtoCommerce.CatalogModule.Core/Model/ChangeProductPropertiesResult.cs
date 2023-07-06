using System.Collections.Generic;
using System.Linq;

namespace VirtoCommerce.CatalogModule.Core.Model
{
    public class ChangeProductPropertiesResult
    {
        public bool Succeeded => Errors == null || !Errors.Any();
        public List<string> Errors { get; set; } = new();
    }
}
