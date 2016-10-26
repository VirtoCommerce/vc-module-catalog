angular.module('virtoCommerce.catalogModule')
.directive('vcItemSearch', ['$localStorage', 'platformWebApp.bladeNavigationService', function ($localStorage, bladeNavigationService) {
    return {
        restrict: 'E',
        templateUrl: 'Modules/$(VirtoCommerce.Catalog)/Scripts/directives/itemSearch.tpl.html',
        scope: {
            blade: '='
        },
        link: function ($scope) {
            var blade = $scope.blade;
            $scope.$localStorage = $localStorage;
            var filter = $scope.filter = blade.filter;
            if (!$localStorage.catalogSearchFilters2) {
                // todo: add predefined filters
                $localStorage.catalogSearchFilters2 = [
                    { keyword: '_missing_:price\*', id: '1', name: 'catalog.blades.categories-items-list.labels.filter-priceless' },
                    { keyword: '_exists_:price\*', id: '2', name: 'catalog.blades.categories-items-list.labels.filter-withPrice' },
                    { keyword: 'price_usd\*:[100 TO 200]', id: '3', name: 'catalog.blades.categories-items-list.labels.filter-priceRange' },
                    { keyword: '__hidden:true', id: '4', name: 'catalog.blades.categories-items-list.labels.filter-notActive' },
                    { name: 'catalog.blades.categories-items-list.labels.filter-new' }
                ];
            }
            if ($localStorage.catalogSearchFilterId && !filter.keyword) {
                filter.current = _.findWhere($localStorage.catalogSearchFilters2, { id: $localStorage.catalogSearchFilterId });
                filter.keyword = filter.current ? filter.current.keyword : '';
            }

            filter.change = function (isDetailBladeOpen) {
                $localStorage.catalogSearchFilterId = filter.current ? filter.current.id : null;
                if (filter.current && !filter.current.id) {
                    filter.current = null;
                    showFilterDetailBlade({ isNew: true });
                } else {
                    if (!isDetailBladeOpen)
                        bladeNavigationService.closeBlade({ id: 'filterDetail' });
                    filter.keyword = filter.current ? filter.current.keyword : '';
                    filter.criteriaChanged();
                }
            };

            filter.edit = function ($event, entry) {
                $event.preventDefault();
                $event.stopPropagation();
                filter.current = entry;
                showFilterDetailBlade({ data: entry });
            };

            function showFilterDetailBlade(bladeData) {
                var newBlade = {
                    id: 'filterDetail',
                    controller: 'virtoCommerce.catalogModule.filterDetailController',
                    template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/filter-detail.tpl.html',
                };
                angular.extend(newBlade, bladeData);
                bladeNavigationService.showBlade(newBlade, blade);
            };
        }
    }
}]);