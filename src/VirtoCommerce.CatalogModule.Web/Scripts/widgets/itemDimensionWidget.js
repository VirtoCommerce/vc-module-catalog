angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.itemDimensionWidgetController', ['$scope', 'virtoCommerce.catalogModule.items', 'platformWebApp.bladeNavigationService', 'virtoCommerce.coreModule.packageType.packageTypeApi', function ($scope, items, bladeNavigationService, packageTypeApi) {

    $scope.openBlade = function () {
        var blade = {
            id: "itemDimension",
            item: $scope.blade.item,         
            controller: 'virtoCommerce.catalogModule.itemDimensionController',
            template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/item-dimensions.tpl.html'
        };
        bladeNavigationService.showBlade(blade, $scope.blade);
    };

    $scope.$watch('blade.item.packageType', function (packageTypeId) {
        if (packageTypeId) {
            packageTypeApi.query({}, function (results) {
                $scope.blade.packageType = _.find(results, function (x) { return x.id == packageTypeId; });
            });
        }
        else {
            delete $scope.blade.packageType;
        }
    });  

}]);
