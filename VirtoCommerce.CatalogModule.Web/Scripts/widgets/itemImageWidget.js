angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.itemImageWidgetController', ['$scope', 'virtoCommerce.catalogModule.items', 'virtoCommerce.catalogModule.categories', 'platformWebApp.bladeNavigationService', function ($scope, items, categories, bladeNavigationService) {

    var item = $scope.blade.currentEntity;

    $scope.openBlade = function () {
        var blade = {
        	id: "itemImage",
            item: item,
            folderPath: getFolderPath(item.catalogId, item.code),
            controller: 'virtoCommerce.catalogModule.imagesController',
            template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/images.tpl.html'
        };
        bladeNavigationService.showBlade(blade, $scope.blade);
    };

    function setCurrentEntities(images) {
        if (images) {
            $scope.currentEntities = images;
        }
    }

    function getFolderPath(catalogId, code) {
        var catalogShortName = catalogId.length > 5 ? catalogId.substring(0, 5) : catalogId;
        var path = catalogShortName + '/' + code;
        return path;
    }

    $scope.$watch('blade.item.images', setCurrentEntities);
    $scope.$watch('blade.currentEntity.images', setCurrentEntities);
}]);
