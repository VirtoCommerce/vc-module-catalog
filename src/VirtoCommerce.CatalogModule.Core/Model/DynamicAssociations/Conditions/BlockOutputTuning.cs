using VirtoCommerce.CoreModule.Core.Conditions;

namespace VirtoCommerce.CatalogModule.Core.Model.DynamicAssociations.Conditions
{
    public class BlockOutputTuning : ConditionTree
    {
        public string SortInfos { get; set; }
        public int OutputLimit { get; set; } = 10;
    }
}
