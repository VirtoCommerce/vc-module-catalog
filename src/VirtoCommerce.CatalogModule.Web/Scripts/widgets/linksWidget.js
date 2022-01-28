angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.linksWidgetController', ['$scope', 'platformWebApp.bladeNavigationService', 'virtoCommerce.catalogModule.listEntries',
        function ($scope, bladeNavigationService, listEntries) {
            var blade = $scope.blade;
            var type = blade.productType ? 'CatalogProduct' : 'Category';

            function refresh() {
                $scope.linksCount = '...';

                var searchCriteria = {
                    objectIds: [blade.currentEntityId],
                    objectType: type,
                    take: 0
                };

                listEntries.searchlinks(searchCriteria, function (data) {
                    $scope.linksCount = data.totalCount;
                });
            }

            $scope.openLinksBlade = function () {
                var newBlade = {
                    id: 'linksListBlade',
                    currentEntity: blade.currentEntity,
                    type: type,
                    controller: 'virtoCommerce.catalogModule.linksListController',
                    template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/links-list/links-list.tpl.html'
                };
                bladeNavigationService.showBlade(newBlade, blade);
            };

            refresh();
        }]);
