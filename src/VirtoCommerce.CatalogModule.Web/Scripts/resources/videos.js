angular.module('virtoCommerce.catalogModule')
    .factory('virtoCommerce.catalogModule.videos', ['$resource', function ($resource) {
        return $resource('api/catalog/videos', {}, {
            search: { method: 'POST', url: 'api/catalog/videos/search' },
            save: { method: 'POST', isArray: true },
            remove: { method: 'DELETE', isArray: true },
            options: { method: 'GET', url: 'api/catalog/videos/options' },
            youtube_embed: { method: 'GET', url: 'https://www.youtube.com/oembed', responseType: 'json' },
            youtube_api: { method: 'GET', url: 'https://www.googleapis.com/youtube/v3/videos' }
        });
    }]);
