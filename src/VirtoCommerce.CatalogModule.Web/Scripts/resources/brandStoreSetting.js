angular.module('virtoCommerce.catalogModule')
    .factory('virtoCommerce.catalogModule.brandStoreSetting', ['$resource', function ($resource) {
        return $resource('api/brand-setting', {}, {
            getByStore: { method: 'GET', url: 'api/brand-setting/store/:storeId' },
            updateSetting: { method: 'PUT' }
        });
    }]);
