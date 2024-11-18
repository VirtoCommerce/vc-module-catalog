angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.productConfigurationWidgetController', ['$scope', 'platformWebApp.bladeNavigationService', function ($scope, bladeNavigationService) {
        var blade = $scope.blade;

        $scope.openBlade = function () {
            var newBlade = {
                id: "productConfiguration",
                productId: blade.currentEntity.id,
                controller: 'virtoCommerce.catalogModule.productConfigurationDetailController',
                template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/configurations/product-configuration-detail.tpl.html'
            };
            bladeNavigationService.showBlade(newBlade, blade);
        };
    }]);
