using System.Collections.Generic;
using VirtoCommerce.CoreModule.Core.Conditions;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Core.Model.DynamicAssociations.Conditions
{
    public class BlockOutputTuning : ConditionTree
    {
        public IList<SortInfo> SortInfos { get; set; }
        public int OutputLimit { get; set; } = 10;
    }
}
