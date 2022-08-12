angular.module('virtoCommerce.catalogModule').
    directive('vcItemSearchCriteria', ['$localStorage', 'platformWebApp.bladeNavigationService', 'virtoCommerce.catalogModule.predefinedSearchFilters',
        function ($localStorage, bladeNavigationService, predefinedSearchFilters) {
    return {
        restrict: 'E',
        templateUrl: function (elem, attrs) {
            return attrs.templateUrl || 'Modules/$(VirtoCommerce.Catalog)/Scripts/directives/itemSearchCriteria.tpl.html'
        },
        scope: {
            blade: '='
        },
        link: function ($scope) {
            var blade = $scope.blade;
            var filter = $scope.filter = blade.filter;
        }
    }
}]);
