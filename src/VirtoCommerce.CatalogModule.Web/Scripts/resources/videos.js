angular.module('virtoCommerce.catalogModule')
    .factory('virtoCommerce.catalogModule.videos', ['$resource', function ($resource) {
        return $resource('api/catalog/videos', {}, {
            options: { method: 'GET', url: 'api/catalog/videos/options' },
            create: { method: 'POST', url: 'api/catalog/videos/create'  },
            search: { method: 'POST', url: 'api/catalog/videos/search' },
            save: { method: 'POST', isArray: true },
            remove: { method: 'DELETE', isArray: true }
        });
    }]);
