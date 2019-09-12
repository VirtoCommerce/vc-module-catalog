angular.module('virtoCommerce.catalogModule')
    .factory('virtoCommerce.catalogModule.bulkActions', ['$resource', function ($resource) {
        return $resource('', null, {
            getActions: { method: 'GET', isArray: true, url: 'api/bulkUpdate/actions' },
            getActionData: { method: 'POST', url: 'api/bulkUpdate/action/data' },
            runBulkAction: { method: 'POST', url: 'api/bulkUpdate/run' },
            cancel: { method: 'POST', url: 'api/bulkUpdate/task/cancel' }
        });
    }]);
