angular.module('virtoCommerce.catalogModule')
.factory('virtoCommerce.catalogModule.browsefilters', ['$resource', function ($resource) {
    return $resource('', {}, {
        queryFilterProperties: { url: 'api/catalog/browsefilters/:storeId/properties', isArray: true },
        saveFilterProperties: { url: 'api/catalog/browsefilters/:storeId/properties', method: 'PUT' },
        getPropertyValues: { url: 'api/catalog/browsefilters/:storeId/properties/:propertyName/values', isArray: true }
    });
}]);
