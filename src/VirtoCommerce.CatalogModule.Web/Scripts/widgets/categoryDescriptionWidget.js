angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.categoryDescriptionWidgetController', ['$scope', 'platformWebApp.bladeNavigationService', function ($scope, bladeNavigationService) {
    $scope.currentBlade = $scope.widget.blade;

    $scope.openBlade = function () {
        var blade = {
            id: "categoryDescriptionsList",
            category: $scope.currentBlade.currentEntity,
            catalog: $scope.currentBlade.catalog,
            controller: 'virtoCommerce.catalogModule.categoryDescriptionsListController',
            template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/categoryDescriptions-list.tpl.html'
        };

        bladeNavigationService.showBlade(blade, $scope.currentBlade);
    };

}]);
