angular.module('virtoCommerce.catalogModule')
    .factory('virtoCommerce.catalogModule.bulkActions', ['$resource', function ($resource) {
        return $resource('', null, {
            getActions: { method: 'GET', isArray: true, url: 'api/catalog/bulkActions/actions' },
            getActionData: { method: 'POST', url: 'api/catalog/bulkActions/actionData' },
            runBulkAction: { method: 'POST', url: 'api/catalog/bulkActions/run' },
            cancel: { method: 'POST', url: 'api/catalog/bulkActions/task/cancel' }
        });
    }]);
