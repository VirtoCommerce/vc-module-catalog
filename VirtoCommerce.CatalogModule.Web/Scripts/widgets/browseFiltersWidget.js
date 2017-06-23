angular.module('virtoCommerce.catalogModule')
.controller('virtoCommerce.catalogModule.browseFiltersWidgetController', ['$scope', 'platformWebApp.bladeNavigationService', function ($scope, bladeNavigationService) {
    var blade = $scope.blade;

    $scope.openBlade = function () {
        var newBlade = {
            id: "storeFilteringProperties",
            storeId: blade.currentEntity.id,
            title: 'Filtering properties',
            controller: 'virtoCommerce.catalogModule.browseFiltersController',
            template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/browse-filters.tpl.html'
        };
        bladeNavigationService.showBlade(newBlade, blade);
    };
}]);
