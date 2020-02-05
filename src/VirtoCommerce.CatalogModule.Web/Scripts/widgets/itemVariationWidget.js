angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.itemVariationWidgetController', ['$scope', 'platformWebApp.bladeNavigationService', 'virtoCommerce.catalogModule.search', function ($scope, bladeNavigationService, search) {
        var blade = $scope.blade;

        function refresh() {
            $scope.variartionCount = '...';

            var searchCriteria = {
                mainProductId: blade.currentEntityId,
                objectType: "CatalogProduct",
                responseGroup: "withProducts",
                take: 0,
                withHidden: true
            };
            search.searchProducts(searchCriteria, function (data) {
                $scope.variartionCount = data.totalCount;
            });
        }

        $scope.openVariationListBlade = function () {
            var newBlade = {
                id: "itemVariationList",
                item: blade.item,
                catalog: blade.catalog,
                toolbarCommandsAndEvents: blade.variationsToolbarCommandsAndEvents,
                controller: 'virtoCommerce.catalogModule.itemVariationListController',
                template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/item-variation-list.tpl.html',
            };
            bladeNavigationService.showBlade(newBlade, blade);
        };

        $scope.$watch("widget.blade.currentEntity", function (pricelist) {
            refresh();
        });

        refresh();
    }]);
