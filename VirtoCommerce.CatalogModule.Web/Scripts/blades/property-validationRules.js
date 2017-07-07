angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.propertyValidationRulesController', ['$scope', function ($scope) {
        var parentEntity = $scope.blade.parentBlade.currentEntity;
        var rule = $scope.blade.parentBlade.currentEntity.validationRule;
        if (!rule) rule = {};

        $scope.blade.propertyValidationRule = {
            id: rule.id,
            required: parentEntity.required,
            isLimited: rule.charCountMin != null || rule.charCountMax != null,
            isSpecificPattern: rule.regExp != null,
            charCountMin: rule.charCountMin,
            charCountMax: rule.charCountMax,
            selectedLimit: 'beetween',
            selectedPattern: 'custom',
            validationPattern: ['custom', 'email', 'url', 'date'],
            characterLimit: ['beetween', 'at least', 'not more than'],
            seatPattern: rule.regExp,
            pattern: rule.regExp
        };

        $scope.checkSelectedPattern = function (selectedPattern) {
            if (selectedPattern === 'email')
                $scope.blade.propertyValidationRule.seatPattern = "/\S+@\S+\.\S+/";
            else if (selectedPattern === 'url')
                $scope.blade.propertyValidationRule.seatPattern = '/^(https?:\/\/)?([\w\.]+)\.([a-z]{2,6}\.?)(\/[\w\.]*)*\/?$/';
            else if (selectedPattern === 'date')
                $scope.blade.propertyValidationRule.seatPattern = '(0[1-9]|[12][0-9]|3[01])[ \.-](0[1-9]|1[012])[ \.-](19|20|)\d\d';
        }

        $scope.selectOption = function (option) {
            $scope.blade.property.valueType = option;
            $scope.bladeClose();
        };

        $scope.blade.headIcon = 'fa-gear';
        $scope.blade.isLoading = false;


        $scope.saveChanges = function () {
            $scope.blade.parentBlade.currentEntity.required = $scope.blade.propertyValidationRule.required;
            $scope.blade.parentBlade.currentEntity.validationRule = {
                id: $scope.blade.propertyValidationRule.id,
                charCountMin: $scope.blade.propertyValidationRule.charCountMin,
                charCountMax: $scope.blade.propertyValidationRule.charCountMax,
                regExp: $scope.blade.propertyValidationRule.seatPattern
            };
            $scope.bladeClose();
        };

        var formScope;
        $scope.setForm = function (form) {
            formScope = form;
        }

        $scope.$watch('blade.propertyValidationRule', function () {
            $scope.isValid = formScope && formScope.$valid;
        }, true);

    }]);
