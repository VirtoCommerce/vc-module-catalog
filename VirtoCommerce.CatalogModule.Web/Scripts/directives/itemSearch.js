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
                    { keyword: 'price*', id: '1', name: 'catalog.blades.categories-items-list.labels.filter-priceless' },
                    { keyword: 'range*', id: '2', name: 'catalog.blades.categories-items-list.labels.filter-priceRange' },
                    { keyword: 'stock*', id: '3', name: 'catalog.blades.categories-items-list.labels.filter-lowStock' },
                    { keyword: '+isActive', id: '4', name: 'catalog.blades.categories-items-list.labels.filter-inActive' },
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

            filter.edit = function () {
                if (filter.current) {
                    showFilterDetailBlade({ data: filter.current });
                }
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