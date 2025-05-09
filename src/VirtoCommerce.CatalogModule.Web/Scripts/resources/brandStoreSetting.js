angular.module('virtoCommerce.catalogModule')
    .factory('virtoCommerce.catalogModule.brandStoreSetting', ['$resource', function ($resource) {
        return $resource('api/brand-setting', {}, {
            getSetting: { method: 'GET', url: 'api/brand-setting/:id' },
            getByStore: { method: 'GET', url: 'api/brand-setting/store/:storeId' },
            createSetting: { method: 'POST' },
            updateSetting: { method: 'PUT' }
        });
    }]);
