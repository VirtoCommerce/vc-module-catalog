angular.module('virtoCommerce.catalogModule')
.controller('virtoCommerce.catalogModule.propertyValidationRulesController', ['$scope', function ($scope) {

	
    $scope.selectOption = function (option) {
    	$scope.blade.property.valueType = option;
        $scope.bladeClose();
    };

    //$scope.blade.headIcon = 'fa-gear';
    //console.log($scope.blade,'valuetp');
    //$scope.blade.isLoading = false;
}]);
