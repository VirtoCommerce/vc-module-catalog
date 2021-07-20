angular.module('virtoCommerce.catalogModule')
    .factory('virtoCommerce.catalogModule.propertyValidators', function () {
        var namePattern = /^\b[0-9a-z]+(_[0-9a-z]+)*\b$/i;
        var maxLenght = 128;

        function isNameValid(value) {
            return value.length <= maxLenght && namePattern.test(value);
        }

        return {
            isNameValid: isNameValid
        };
    });
