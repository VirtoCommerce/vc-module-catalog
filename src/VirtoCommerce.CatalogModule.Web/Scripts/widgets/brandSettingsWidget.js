angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.brandSettingsWidgetController', ['$scope', 'platformWebApp.bladeNavigationService',
        function ($scope, bladeNavigationService) {
            $scope.openBlade = function () {
                var newBlade = {
                    id: 'brandSettingDetails',
                    controller: 'virtoCommerce.catalogModule.brandSettingDetailsController',
                    template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/brand-setting-details.tpl.html',
                    store: $scope.blade.currentEntity,
                };

                bladeNavigationService.showBlade(newBlade, $scope.blade);
            };
        }]);
