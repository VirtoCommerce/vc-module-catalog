angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.catalogSelectorController', ['$scope', 'virtoCommerce.catalogModule.catalogs', function ($scope, catalogs) {
        //ToDo: Apply Infinite scrolling
        catalogs.search({ take: 1000 }, function (data) {
            $scope.catalogs = angular.copy(data.results);
        });
    }]);
