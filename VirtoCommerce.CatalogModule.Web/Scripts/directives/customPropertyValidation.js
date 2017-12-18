﻿angular.module('virtoCommerce.catalogModule')
    .directive('vaGeoPointValidation', function () {
        var isValid = function (s) {
        var pattern = /^[-+]?([1-8]?\d(.\d+)?|90(.0+)?),\s*[-+]?(180(.0+)?|((1[0-7]\d)|([1-9]?\d))(.\d+)?)$/;
        return pattern.test(s) || s === null || s === '';
    };

    return {
        require: 'ngModel',
        link: function (scope, elm, attrs, ngModelCtrl) {

            ngModelCtrl.$parsers.unshift(function (viewValue) {
                ngModelCtrl.$setValidity('geoPoint', isValid(viewValue));
                return viewValue;
            });

            ngModelCtrl.$formatters.unshift(function (modelValue) {
                ngModelCtrl.$setValidity('geoPoint', isValid(modelValue));
                return modelValue;
            });
        }
    };
});