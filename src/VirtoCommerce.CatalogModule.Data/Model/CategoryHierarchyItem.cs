namespace VirtoCommerce.CatalogModule.Data.Model;

public class CategoryHierarchyItem
{
    public string Id { get; set; }
    public string ParentCategoryId { get; set; }
    public int Depth { get; set; }
}
