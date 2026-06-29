angular.module('virtoCommerce.catalogModule')
.controller('virtoCommerce.catalogModule.searchConfigurationWidgetController', ['$scope', 'platformWebApp.bladeNavigationService', function ($scope, bladeNavigationService) {
    var blade = $scope.blade;

    $scope.openBlade = function () {
        var newBlade = {
            id: 'storeSearchConfiguration',
            storeId: blade.currentEntity.id,
            store: blade.currentEntity,
            title: 'catalog.blades.search-configuration.title',
            subtitle: 'catalog.blades.search-configuration.subtitle',
            controller: 'virtoCommerce.catalogModule.searchConfigurationController',
            template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/search-configuration.tpl.html'
        };
        bladeNavigationService.showBlade(newBlade, blade);
    };
}]);
