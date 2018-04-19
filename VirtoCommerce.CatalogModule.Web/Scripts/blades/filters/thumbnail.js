angular.module('virtoCommerce.catalogModule')
    .filter('thumbnail', function () {
        return function (imageUrl, prefix) {
            var parts = imageUrl.split(".");
            var thumbnailUrl = parts[0].indexOf('_') + 1 != 0 || parts[0].indexOf('-') + 1 != 0 || parts[0].indexOf('.') + 1 != 0 ? imageUrl : parts[0] + prefix + parts[1];
            return thumbnailUrl;
        };
    });
