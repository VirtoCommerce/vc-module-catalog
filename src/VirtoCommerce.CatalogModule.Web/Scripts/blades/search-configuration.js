angular.module('virtoCommerce.catalogModule')
.controller('virtoCommerce.catalogModule.searchConfigurationController', ['$scope', 'platformWebApp.bladeNavigationService', function ($scope, bladeNavigationService) {
    var blade = $scope.blade;
    blade.isLoading = false;
    blade.headIcon = 'fas fa-sliders-h';

    function openSorting() {
        bladeNavigationService.showBlade({
            id: 'storeSortOrderings',
            storeId: blade.storeId,
            store: blade.store,
            title: 'catalog.blades.product-sorting-list.title',
            subtitle: 'catalog.blades.product-sorting-list.subtitle',
            controller: 'virtoCommerce.catalogModule.productSortingListController',
            template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/product-sorting-list.tpl.html'
        }, blade);
    }

    function openFacets() {
        // Reuse the existing filtering-properties (facets) blade unchanged.
        bladeNavigationService.showBlade({
            id: 'storeFilteringProperties',
            storeId: blade.storeId,
            title: 'catalog.blades.search-configuration.menu.facets',
            controller: 'virtoCommerce.catalogModule.aggregationPropertiesController',
            template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/aggregation-properties-list.tpl.html'
        }, blade);
    }

    blade.menuItems = [
        { name: 'catalog.blades.search-configuration.menu.facets', icon: 'fab fa-buffer', action: openFacets },
        { name: 'catalog.blades.search-configuration.menu.sorting', icon: 'fas fa-sort-amount-down', action: openSorting }
    ];
}]);
