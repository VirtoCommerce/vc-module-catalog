angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.propertyValidationRulesController', ['$scope', function ($scope) {
        var blade = $scope.blade;
        var rule = _.first(blade.parentBlade.currentEntity.validationRules) || {};

        blade.propertyValidationRule = {
            id: rule.id,
            required: blade.parentBlade.currentEntity.required,
            isLimited: rule.charCountMin != null || rule.charCountMax != null,
            isSpecificPattern: rule.regExp != null,
            charCountMin: rule.charCountMin,
            charCountMax: rule.charCountMax,
            selectedLimit: 'between',
            characterLimit: ['between', 'at-least', 'not-more-than'],
            validationPatterns: [
                {
                    name: 'custom',
                    pattern: ""
                },
                {
                    name: "email",
                    pattern: "^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$"
                },
                {
                    name: "url",
                    pattern: "https?:\\/\\/(www\\.)?[-a-zA-Z0-9@:%._\\+~#=]{2,256}\\.[a-z]{2,6}\\b([-a-zA-Z0-9@:%_\\+.~#?&//=]*)"
                },
                {
                    name: "date",
                    pattern: "(0[1-9]|[12][0-9]|3[01])[ \.-](0[1-9]|1[012])[ \.-](19|20|)\d\d"
                }
            ],
            selectedPattern: { name: 'custom', pattern: rule.regExp },
            pattern: rule.regExp
        };

        if (rule.charCountMin && !rule.charCountMax)
            blade.propertyValidationRule.selectedLimit = 'at-least';
        else if (!rule.charCountMin && rule.charCountMax)
            blade.propertyValidationRule.selectedLimit = 'not-more-than';
        else if (rule.charCountMin && rule.charCountMax)
            blade.propertyValidationRule.selectedLimit = 'between';

        blade.isLoading = false;

        $scope.saveChanges = function () {
            blade.parentBlade.currentEntity.required = blade.propertyValidationRule.required;
            blade.parentBlade.currentEntity.validationRules = [{
                id: blade.propertyValidationRule.id,
                charCountMin: blade.propertyValidationRule.isLimited && blade.propertyValidationRule.selectedLimit !== 'not-more-than' ? blade.propertyValidationRule.charCountMin : null,
                charCountMax: blade.propertyValidationRule.isLimited && blade.propertyValidationRule.selectedLimit !== 'at-least' ? blade.propertyValidationRule.charCountMax : null,
                regExp: blade.propertyValidationRule.isSpecificPattern ? blade.propertyValidationRule.selectedPattern.pattern : null
            }];
            blade.parentBlade.currentEntity.validationRule = blade.parentBlade.currentEntity.validationRules[0];
            $scope.bladeClose();
        };

        var formScope;
        $scope.setForm = function (form) {
            formScope = form;
        };
        $scope.$watch('blade.propertyValidationRule', function () {
            $scope.isValid = formScope && formScope.$valid;
        }, true);

    }]);
