angular.module('virtoCommerce.catalogBulkActionsModule')
    .controller('virtoCommerce.catalogBulkActionsModule.helloWorldController', ['$scope', 'virtoCommerce.catalogBulkActionsModule.webApi', function ($scope, api) {
        var blade = $scope.blade;
        blade.title = 'VirtoCommerce.CatalogBulkActionsModule';

        blade.refresh = function () {
            api.get(function (data) {
                blade.title = 'virtoCommerce.catalogBulkActionsModule.blades.hello-world.title';
                blade.data = data.result;
                blade.isLoading = false;
            });
        };

        blade.refresh();
    }]);
