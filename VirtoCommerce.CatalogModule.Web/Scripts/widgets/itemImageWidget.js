angular.module('virtoCommerce.catalogModule')
.controller('virtoCommerce.catalogModule.itemImageWidgetController', ['$scope', 'virtoCommerce.catalogModule.items', 'virtoCommerce.catalogModule.categories', 'platformWebApp.bladeNavigationService', function ($scope, items, categories, bladeNavigationService) {

    $scope.openBlade = function () {
        var blade = {
        	id: "itemImage",
            item: $scope.blade.currentEntity,
            folderPath: getFolderPath(),
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
    function getFolderPath() {
        var path = ($scope.blade.currentEntity.catalogId ? (getShortCatalogId($scope.blade.currentEntity.catalogId) + '/') : '') + $scope.blade.currentEntity.code;
        return path;
    }

    function getShortCatalogId(catalogId) {
        debugger;
            return catalogId.length > 5 ? catalogId.substring(0, 5) : catalogId;
        }

        $scope.$watch('blade.item.images', setCurrentEntities);
    $scope.$watch('blade.currentEntity.images', setCurrentEntities);
}]);
