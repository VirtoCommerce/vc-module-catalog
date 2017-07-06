angular.module('virtoCommerce.catalogModule')
.controller('virtoCommerce.catalogModule.propertyTypeController', ['$scope', function ($scope) {
    console.log($scope.blade.parentBlade, '1');
    $scope.selectOption = function (option) {
        $scope.blade.parentBlade.currentEntity.type = option;   
        $scope.bladeClose();
    };

    $scope.blade.headIcon = 'fa-gear';

    $scope.blade.isLoading = false;
}]);
