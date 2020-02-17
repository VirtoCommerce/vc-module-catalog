using System;
using VirtoCommerce.SearchModule.Core.Services;

namespace VirtoCommerce.CatalogModule.Core.Services
{
    public interface ISearchRequestBuilderRegistrar
    {
        /// <summary>
        /// Gets registered request builder by document type (e.g. KnownDocumentTypes.Product).
        /// </summary>
        /// <param name="documentType"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Thrown when the requested document type is not registered. </exception>
        ISearchRequestBuilder GetRequestBuilderByDocumentType(string documentType);

        /// <summary>
        /// Register Search Request Builder factory for the given document type. Overrides existing registration for a document type.
        /// </summary>
        /// <typeparam name="TSearchRequestBuilder"></typeparam>
        /// <param name="documentType"></param>
        /// <param name="factory"></param>
        void Register<TSearchRequestBuilder>(string documentType, Func<TSearchRequestBuilder> factory) where TSearchRequestBuilder : class, ISearchRequestBuilder;
    }
}
