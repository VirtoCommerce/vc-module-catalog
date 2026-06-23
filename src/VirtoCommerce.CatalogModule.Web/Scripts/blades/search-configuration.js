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
            title: 'catalog.blades.sort-ordering-list.title',
            subtitle: 'catalog.blades.sort-ordering-list.subtitle',
            controller: 'virtoCommerce.catalogModule.sortOrderingListController',
            template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/sort-ordering-list.tpl.html'
        }, blade);
    }

    function openFacets() {
        // Reuse the existing filtering-properties (facets) blade unchanged.
        bladeNavigationService.showBlade({
            id: 'storeFilteringProperties',
            storeId: blade.storeId,
            title: 'Filtering properties',
            controller: 'virtoCommerce.catalogModule.aggregationPropertiesController',
            template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/aggregation-properties-list.tpl.html'
        }, blade);
    }

    blade.menuItems = [
        { name: 'catalog.blades.search-configuration.menu.facets', icon: 'fab fa-buffer', action: openFacets },
        { name: 'catalog.blades.search-configuration.menu.sorting', icon: 'fas fa-sort-amount-down', action: openSorting }
    ];
}]);
