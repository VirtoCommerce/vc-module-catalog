angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.brandSettingWidgetController', ['$scope', 'platformWebApp.bladeNavigationService',
        function ($scope, bladeNavigationService) {
            $scope.openBlade = function () {
                var newBlade = {
                    id: 'brandSettingBlade',
                    store: $scope.blade.currentEntity,
                    controller: 'virtoCommerce.catalogModule.brandSettingDetailsController',
                    template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/brand-setting-details.tpl.html'
                };

                bladeNavigationService.showBlade(newBlade, $scope.blade);
            };
        }]);
