using System.Collections.Generic;

namespace VirtoCommerce.CatalogModule.Core.Outlines;

public interface IHasOutlines
{
    IList<Outline> Outlines { get; set; }
}
