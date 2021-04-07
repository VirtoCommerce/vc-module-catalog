angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.billOfMaterialsWidgetController', ['$scope', 'platformWebApp.bladeNavigationService', function ($scope, bladeNavigationService) {
    var blade = $scope.blade;

    $scope.openBlade = function () {
        var newBlade = {
            id: "billOfMaterialsList",
            item: blade.item,              
            controller: 'virtoCommerce.catalogModule.billOfMaterialsListController',
            template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/bill-of-materials-list.tpl.html'
        };
        bladeNavigationService.showBlade(newBlade, blade);
    };
}]);
