angular.module('virtoCommerce.catalogModule')
    .factory('virtoCommerce.catalogModule.brandSettings', ['$resource', function ($resource) {
        return $resource('api/brand-settings', {}, {
            getByStore: { method: 'GET', url: 'api/brand-settings/store/:storeId' },
            updateSetting: { method: 'PUT' }
        });
    }]);
