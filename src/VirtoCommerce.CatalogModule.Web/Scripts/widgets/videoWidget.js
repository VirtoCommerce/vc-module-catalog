angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.videoWidgetController', ['$scope', 'platformWebApp.bladeNavigationService',
        function ($scope, bladeNavigationService) {
            var blade = $scope.blade;

            $scope.openBlade = function () {
                var newBlade = {
                    id: "videosList",
                    owner: blade.currentEntity,
                    ownerType: 'Product',
                    catalog: blade.catalog,
                    controller: 'virtoCommerce.catalogModule.videoListController',
                    template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/video-list.tpl.html'
                };
                bladeNavigationService.showBlade(newBlade, blade);
            };
        }]);
