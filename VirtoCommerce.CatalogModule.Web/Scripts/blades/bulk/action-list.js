angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.actionListController', ['$scope', 'platformWebApp.bladeNavigationService', 'virtoCommerce.catalogModule.catalogBulkActionService', function ($scope, bladeNavigationService, catalogBulkActionService) {
        var blade = $scope.blade;
        $scope.selectedNodeId = null;

        function initializeBlade() {
            $scope.registrationsList = catalogBulkActionService.registrationsList;
            blade.isLoading = false;
        };

        $scope.openBlade = function(data) {
            var newBlade = {};
            angular.copy(data, newBlade);
            newBlade.selectedCategories = blade.selectedCategories;
            newBlade.selectedProducts = blade.selectedProducts;
            newBlade.catalog = blade.catalog;

            if (angular.isFunction(data.onInitialize)) {
                data.onInitialize(newBlade);
            }

            bladeNavigationService.showBlade(newBlade, blade);
        };

        $scope.blade.headIcon = 'fa-upload';
        $scope.blade.title = "Bulk action list";
        $scope.blade.subtitle = "Select action for bulk operation";

        initializeBlade();
    }]);
