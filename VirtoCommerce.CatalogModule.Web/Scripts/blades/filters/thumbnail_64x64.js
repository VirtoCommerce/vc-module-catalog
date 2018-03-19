angular.module('virtoCommerce.catalogModule')
    .filter('thumbnail_64x64', function () {
        return function (imageUrl) {
            var parts = imageUrl.split(".");
            var thumbnailUrl = parts[0].indexOf('_') + 1 != 0 ? imageUrl : parts[0] + "_64x64." + parts[1];
            return thumbnailUrl;
        };
    });
