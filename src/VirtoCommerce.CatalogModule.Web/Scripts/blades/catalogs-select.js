angular.module('virtoCommerce.catalogModule')
.controller('virtoCommerce.catalogModule.catalogsSelectController', ['$scope', 'virtoCommerce.catalogModule.catalogs', 'platformWebApp.bladeNavigationService', function ($scope, catalogs, bladeNavigationService) {
    var blade = $scope.blade;

    blade.refresh = function () {
        blade.isLoading = true;
        //ToDo: Apply Infinite scrolling
        catalogs.search({take: 1000}, function (data) {
            if (blade.doShowAllCatalogs) {
                $scope.objects = data.results;
            } else {
                $scope.objects = _.where(data.results, { isVirtual: false });
            }

            blade.isLoading = false;
        },
        function (error) { bladeNavigationService.setError('Error ' + error.status, blade); });
    };

    $scope.selectNode = function (selectedNode) {
        $scope.bladeClose(function () {
            blade.parentBlade.onAfterCatalogSelected(selectedNode);
        });
    };

    // actions on load
    blade.refresh();
}]);
