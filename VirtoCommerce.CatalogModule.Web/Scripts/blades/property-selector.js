angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.propertySelectorController', ['$scope', 'platformWebApp.bladeNavigationService', function ($scope, bladeNavigationService) {
        var blade = $scope.blade;

        blade.isLoading = true;

        function initializeBlade() {
            blade.title = 'Select properties for edit';
            blade.headIcon = 'fa-folder';

            var allProperties = angular.copy(blade.properties);

            allProperties = _.sortBy(allProperties, 'group', 'name');
            var selectedProperties = angular.copy(blade.includedProperties);
            selectedProperties = _.sortBy(selectedProperties, 'name');
            blade.allEntities = _.groupBy(allProperties, 'group');
            blade.selectedEntities = _.groupBy(selectedProperties, 'group');
            blade.isLoading = false;
            $scope.addSelected('All properties');
        }

        $scope.addSelected = function (groupKey) {
            blade.selectedEntities[groupKey] = _.filter(blade.allEntities[groupKey], function (item) { return item.isSelected });
        }

        $scope.selectAllInGroup = function (groupKey) {
            blade.selectedEntities[groupKey] = blade.allEntities[groupKey];
        }

        $scope.clearAllInGroup = function (groupKey) {
            blade.selectedEntities[groupKey] = [];
        }

        $scope.sortSelected = function (groupKey) {
            blade.selectedEntities[groupKey] = _.sortBy(blade.selectedEntities[groupKey], 'name');
        }

        $scope.cancelChanges = function () {
            bladeNavigationService.closeBlade(blade);
        }

        $scope.isValid = function () {
            return _.some(blade.selectedEntities, function (item) { return item.length; })
                || $scope.HasChangedProperties(blade.properties);
        }

        $scope.HasChangedProperties = function (properties) {
            return _.filter(properties,
                function (prop) {
                    return prop.isChanged;
                }).length > 0;
        };

        $scope.saveChanges = function () {
            var includedProperties = _.flatten(_.map(blade.selectedEntities, _.values));
            if (blade.onSelected) {
                blade.onSelected(includedProperties);
                bladeNavigationService.closeBlade(blade);
            }
        };

        initializeBlade();

    }]);
