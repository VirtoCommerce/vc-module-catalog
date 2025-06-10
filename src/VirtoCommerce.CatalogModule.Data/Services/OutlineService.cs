using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogModule.Core.Extensions;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.OutlinePart;
using VirtoCommerce.CatalogModule.Core.Outlines;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Seo.Core.Models;
using static VirtoCommerce.CatalogModule.Core.Extensions.SeoExtensions;

namespace VirtoCommerce.CatalogModule.Data.Services
{
    public sealed class OutlineService : IOutlineService
    {
        private readonly IOutlinePartResolver _outlinePartResolver;
        private readonly IOutlinePartNameResolver _outlinePartNameResolver;

        public OutlineService(IOutlinePartNameResolver outlinePartNameResolver, IOutlinePartResolver outlinePartResolver = null)
        {
            _outlinePartResolver = outlinePartResolver ?? new IdOutlinePartResolver();
            _outlinePartNameResolver = outlinePartNameResolver;
        }

        /// <summary>
        /// Constructs single physical and/or multiple virtual outlines for given objects.
        /// Outline is the path from the catalog to one of the child objects (product or category):
        /// catalog/parent-category1/.../parent-categoryN/object
        /// </summary>
        /// <param name="objects">Objects for which outlines should be constructed.</param>
        /// <param name="catalogId">If catalogId is not null then only outlines starting with this catalog will be constructed. If catalogId is null then all possible outlines will be constructed.</param>
        public void FillOutlinesForObjects(IEnumerable<IHasOutlines> objects, string catalogId)
        {
            foreach (var obj in objects)
            {
                var relationshipsTree = GetRelationshipsTree(obj);
                if (relationshipsTree != null)
                {
                    obj.Outlines = new List<Outline>();
                    foreach (var flatBranch in relationshipsTree.GetFlatBranches())
                    {
                        var outlineItems = new List<OutlineItem>();
                        Entity parentEntity = null;
                        foreach (var entity in flatBranch)
                        {
                            var outlineItem = ConvertToOutlineItem(entity);
                            outlineItem.HasVirtualParent = !IsVirtualEntity(entity) && IsVirtualEntity(parentEntity);
                            outlineItems.Add(outlineItem);
                            parentEntity = entity;
                        }
                        //filter branches with passed catalog
                        if (string.IsNullOrEmpty(catalogId) || outlineItems.ContainsCatalog(catalogId))
                        {
                            var outline = new Outline
                            {
                                Items = outlineItems,
                            };
                            obj.Outlines.Add(outline);
                        }
                    }
                }
            }
        }

        private GenericTreeNode<Entity> GetRelationshipsTree(IHasOutlines obj)
        {
            var product = obj as CatalogProduct;
            var category = obj as Category;
            GenericTreeNode<Entity> retVal = null;
            if (product != null)
            {
                retVal = GetRelationshipsTree(product);
            }
            if (category != null)
            {
                retVal = GetRelationshipsTree(category);
            }
            return retVal;
        }

        private GenericTreeNode<Entity> GetRelationshipsTree(CatalogProduct product)
        {
            var retVal = new GenericTreeNode<Entity>(product);

            if (product.Category != null)
            {
                retVal.AddChild(GetRelationshipsTree(product.Category));
            }
            else
            {
                retVal.AddChild(new GenericTreeNode<Entity>(product.Catalog));
            }

            var productLinks = new List<CategoryLink>(product.Links ?? Array.Empty<CategoryLink>());

            // VP-5628: Need to use main product links for the variations
            if (product.MainProduct != null)
            {
                productLinks.AddRange(product.MainProduct.Links ?? Array.Empty<CategoryLink>());
            }

            if (!productLinks.IsNullOrEmpty())
            {
                foreach (var link in productLinks)
                {
                    if (link.Category != null)
                    {
                        retVal.AddChild(GetRelationshipsTree(link.Category));
                    }
                    else
                    {
                        retVal.AddChild(new GenericTreeNode<Entity>(link.Catalog));
                    }
                }
            }
            return retVal;
        }

        private GenericTreeNode<Entity> GetRelationshipsTree(Category category)
        {
            var retVal = new GenericTreeNode<Entity>(category);
            if (!category.Parents.IsNullOrEmpty())
            {
                var parentNode = GetRelationshipsTree(category.Parents.Reverse().First());
                retVal.AddChild(parentNode);
            }
            else
            {
                retVal.AddChild(new GenericTreeNode<Entity>(category.Catalog));
            }
            if (!category.Links.IsNullOrEmpty())
            {
                foreach (var link in category.Links)
                {
                    var node = new GenericTreeNode<Entity>(link.Catalog);
                    if (link.Category != null)
                    {
                        node = GetRelationshipsTree(link.Category);
                    }
                    retVal.AddChild(node);
                }
            }
            return retVal;
        }

        private static bool IsVirtualEntity(Entity obj)
        {
            return obj switch
            {
                Category category => category.IsVirtual,
                Catalog catalog => catalog.IsVirtual,
                _ => false,
            };
        }

        private OutlineItem ConvertToOutlineItem(Entity entity)
        {
            var seoSupport = entity as ISeoSupport;

            var retVal = new OutlineItem
            {
                Id = _outlinePartResolver.ResolveOutlinePart(entity),
                SeoObjectType = seoSupport != null ? seoSupport.SeoObjectType : SeoCatalog,
                SeoInfos = seoSupport?.SeoInfos,
                Name = _outlinePartNameResolver.ResolveOutlineName(entity),
                LocalizedName = _outlinePartNameResolver.ResolveLocalizedOutlineName(entity)
            };

            return retVal;
        }
    }


    internal class GenericTreeNode<T>
    {
        private ICollection<GenericTreeNode<T>> _children;

        public GenericTreeNode(T value)
        {
            Value = value;
            _children = new List<GenericTreeNode<T>>();
        }

        public T Value { get; private set; }

        public GenericTreeNode<T> Parent { get; set; }

        public IEnumerable<GenericTreeNode<T>> Children
        {
            get
            {
                return _children;
            }
        }

        public GenericTreeNode<T> AddChild(GenericTreeNode<T> node)
        {
            _children.Add(node);
            node.Parent = this;

            return node;
        }

        public override string ToString()
        {
            var retVal = new List<string>();
            foreach (var branch in GetFlatBranches())
            {
                retVal.Add(string.Join("/", branch));
            }
            return string.Join(" | ", retVal);
        }

        public IEnumerable<IEnumerable<T>> GetFlatBranches()
        {
            var retVal = new List<List<T>>();
            if (!_children.IsNullOrEmpty())
            {
                foreach (var children in _children)
                {
                    var childrenFlatBranches = children.GetFlatBranches();
                    foreach (var childrenFlatBranch in childrenFlatBranches)
                    {
                        var flatBranch = new List<T>();
                        flatBranch.AddRange(childrenFlatBranch.Concat(new[] { Value }));
                        retVal.Add(flatBranch);
                    }
                }
            }
            else
            {
                retVal.Add(new List<T>(new[] { Value }));
            }
            return retVal;
        }
    }

}
