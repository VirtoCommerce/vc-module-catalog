angular.module('virtoCommerce.catalogModule')
.factory('virtoCommerce.catalogModule.barcodeSearch', ['$resource', function ($resource) {
    return $resource('', {}, {
        getSettings: { method: 'GET', url: 'api/catalog/barcode-search/store/:storeId' },
        saveSettings: { method: 'PUT', url: 'api/catalog/barcode-search/store/:storeId' },
        getFields: { method: 'GET', url: 'api/catalog/barcode-search/store/:storeId/fields', isArray: true }
    });
}]);
