angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.catalogsSelectController', ['$scope', 'virtoCommerce.catalogModule.catalogs', 'platformWebApp.bladeNavigationService', function ($scope, catalogs, bladeNavigationService) {
        var blade = $scope.blade;

        blade.refresh = function () {
            blade.isLoading = true;

            const criteria = {
                sort: 'name',
                take: 1000,
            };

            if (!blade.doShowAllCatalogs) {
                criteria.isVirtual = false;
            }

            //ToDo: Apply Infinite scrolling
            catalogs.search(criteria, function (data) {
                $scope.objects = data.results;
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
