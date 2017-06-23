angular.module('virtoCommerce.catalogModule')
.factory('virtoCommerce.catalogModule.browsefilters', ['$resource', function ($resource) {
    return $resource('', {}, {
        queryFilterProperties: { url: 'api/catalog/browsefilters/properties/:id', isArray: true
    },
        saveFilterProperties: { url: 'api/catalog/browsefilters/properties/:id', method: 'PUT' }
    });
}]);
