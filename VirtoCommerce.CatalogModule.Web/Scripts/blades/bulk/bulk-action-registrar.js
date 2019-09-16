angular.module('virtoCommerce.catalogModule')
    .factory('virtoCommerce.catalogModule.catalogBulkActionService', function () {
        var registrationList = [];
        var retVal = {
            getByName: function (name) {
                return _.find(registrationList, function (info) { return info.name === name});
            },
            register: function (registrationInfo) {

                var index = _.findIndex(registrationList,
                    function (info) { return info.name === registrationInfo.name; });
                if (index >= 0) {
                    registrationList[index] = registrationInfo;
                }
                else 
                {
                    registrationList.push(registrationInfo);
                }
            }
        };
        return retVal;
    });
