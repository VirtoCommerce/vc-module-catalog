angular.module('virtoCommerce.catalogModule')
    .factory('virtoCommerce.catalogModule.catalogBulkActionService', function () {
        var registrationList = [];
        var retVal = {
            getAll: function () {
                return angular.copy(registrationList);
            },
            register: function (bulkAction) {
                registrationList.push(bulkAction);
            }
        };
        return retVal;
    });
