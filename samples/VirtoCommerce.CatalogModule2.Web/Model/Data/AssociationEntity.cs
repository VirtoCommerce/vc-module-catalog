using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Data.Model;

namespace VirtoCommerce.CatalogModule2.Data.Model
{
    public class AssociationEntity2 : AssociationEntity
    {
        public override AssociationEntity FromModel(ProductAssociation association)
        {
            return base.FromModel(association);
        }
        public override void Patch(AssociationEntity target)
        {
            base.Patch(target);
        }
        public override ProductAssociation ToModel(ProductAssociation association)
        {
            return base.ToModel(association);
        }
        public override ProductAssociation ToReferencedAssociationModel(ProductAssociation association)
        {
            return base.ToReferencedAssociationModel(association);
        }
    }
}
