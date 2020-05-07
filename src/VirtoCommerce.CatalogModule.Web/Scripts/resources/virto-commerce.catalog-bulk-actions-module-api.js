angular.module('virtoCommerce.catalogBulkActionsModule')
    .factory('virtoCommerce.catalogBulkActionsModule.webApi', ['$resource', function ($resource) {
        var baseUrl = 'api/bulk/actions';
        return $resource('', null, {
            getActions: { method: 'GET', isArray: true, url: baseUrl },
            getActionData: { method: 'POST', url: baseUrl + '/data' },
            runBulkAction: { method: 'POST', url: baseUrl },
            cancel: { method: 'DELETE', url: baseUrl }
        });
}]);
