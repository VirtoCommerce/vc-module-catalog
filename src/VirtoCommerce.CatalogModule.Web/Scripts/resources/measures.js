angular.module('virtoCommerce.catalogModule')
    .factory('virtoCommerce.catalogModule.measures', ['$resource', function ($resource) {
        return $resource('api/catalog/measures/', {}, {
            getMeasure: { method: 'GET', url: 'api/catalog/measures/:id' },
            searchMeasures: { method: 'POST', url: 'api/catalog/measures/search' },
            createMeasures: { method: 'POST', isArray: true },
            updateMeasure: { method: 'PUT' },
            deleteMeasure: { method: 'DELETE' },
            getDefaultMeasures: { method: 'GET', url: 'api/catalog/measures/default', isArray: true },
        });
    }]);
