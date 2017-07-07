angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.propertyValidationRulesController', ['$scope', function ($scope) {
        $scope.blade.propertyValidationRule = {
            isUniquie: false,
            isLimited: false,
            isSpecificPattern:false,
            charCountMin: undefined,
            charCountMax: undefined,
            selectedLimit: 'beetween',
            selectedPattern:'custom',
            validationPattern: ['custom', 'email', 'url', 'date'],
            characterLimit: ['beetween', 'at least', 'not more than'],
            seatPattern:'',
            pattern: ''
        };
        $scope.checkSelectedPattern = function (selectedPattern) {
            if (selectedPattern == 'email')
                $scope.blade.propertyValidationRule.seatPattern = '/^([\w\._]+)@\1\.([a-z]{2,6}\.?)$/';
            else if (selectedPattern == 'url')
                $scope.blade.propertyValidationRule.seatPattern = '/^(https?:\/\/)?([\w\.]+)\.([a-z]{2,6}\.?)(\/[\w\.]*)*\/?$/';
            else if (selectedPattern == 'date')
                $scope.blade.propertyValidationRule.seatPattern = '(0[1-9]|[12][0-9]|3[01])[ \.-](0[1-9]|1[012])[ \.-](19|20|)\d\d';
            else if 
                return;
        }
    
    $scope.selectOption = function (option) {
    	$scope.blade.property.valueType = option;
        $scope.bladeClose();
        };
    $scope.blade.headIcon = 'fa-gear';
    $scope.blade.isLoading = false;
}]);
