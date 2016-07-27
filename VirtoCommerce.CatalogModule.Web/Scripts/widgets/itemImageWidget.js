angular.module('virtoCommerce.catalogModule')
.controller('virtoCommerce.catalogModule.itemImageWidgetController', ['$scope', 'virtoCommerce.catalogModule.items', 'virtoCommerce.catalogModule.categories', 'platformWebApp.bladeNavigationService', function ($scope, items, categories, bladeNavigationService) {

    $scope.openBlade = function () {
        var blade = {
        	id: "itemImage",
        	item: $scope.blade.currentEntity,
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
    $scope.$watch('blade.item.images', setCurrentEntities);
    $scope.$watch('blade.currentEntity.images', setCurrentEntities);
}]);
