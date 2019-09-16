angular.module('virtoCommerce.catalogModule')
    .factory('virtoCommerce.catalogModule.catalogBulkActionService', function () {
        var registrationList = [];
        var retVal = {
            getByName: function (name) {
                return _.find(registrationList, function (info) { return info.name === name});
            },
            register: function (registrationInfo) {
                registrationList.push(registrationInfo);
            }
        };
        return retVal;
    });
