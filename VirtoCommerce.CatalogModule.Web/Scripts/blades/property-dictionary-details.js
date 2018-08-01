angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.propertyDictionaryDetailsController',
        ['$scope', '$filter', 'platformWebApp.dialogService', 'platformWebApp.settings', function ($scope, $filter, dialogService, settings) {
            var blade = $scope.blade;
            var pb = $scope.blade.parentBlade;
            blade.headIcon = 'fa-book';
            
            debugger;
            $scope.blade.isLoading = false;
        }]);
