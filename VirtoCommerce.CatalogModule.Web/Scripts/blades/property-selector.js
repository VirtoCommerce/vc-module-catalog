angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.propertySelectorController', ['$scope', 'platformWebApp.bladeNavigationService', '$localStorage', 'platformWebApp.authService', function ($scope, bladeNavigationService, $localStorage, authService) {
        var blade = $scope.blade;
        blade.existingFilteredProperties = [];
        blade.isLoading = true;
        const maxPreviewFiltersCount = 10;

        function initializeBlade() {
            blade.title = 'catalog.blades.property-selector.title';
            blade.headIcon = 'fa-folder';

            var allProperties = angular.copy(blade.properties);

            allProperties = _.sortBy(allProperties, 'group', 'name');
            var selectedProperties = angular.copy(blade.includedProperties);
            selectedProperties = _.sortBy(selectedProperties, 'name');
            blade.allEntities = _.groupBy(allProperties, 'group');
            blade.selectedEntities = _.groupBy(selectedProperties, 'group');

            if ($localStorage.entryPropertyFilters) {
                if ($localStorage.entryPropertyFilters[authService.id].length > maxPreviewFiltersCount) {
                    blade.existingFilteredProperties = $localStorage.entryPropertyFilters[authService.id].slice(0, maxPreviewFiltersCount - 1);
                    blade.existingFilteredProperties.push('...');


                } else {
                    blade.existingFilteredProperties = $localStorage.entryPropertyFilters[authService.id];
                }
            }

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
                || $scope.hasChangedProperties(blade.properties);
        }

        $scope.hasChangedProperties = function (properties) {
            return _.filter(properties,
                function (prop) {
                    return prop.isChanged;
                }).length > 0;
        };

        $scope.saveChanges = function () {
            var includedProperties = _.flatten(_.map(blade.selectedEntities, _.values));

            if (blade.onSelected) {

                var currentFilter = {};
                currentFilter[authService.id] = _.map(includedProperties, function (item) { return item.name; });
                if ($localStorage.entryPropertyFilters) {
                    angular.extend($localStorage.entryPropertyFilters, currentFilter);
                } else {
                    $localStorage.entryPropertyFilters = currentFilter;
                }

                blade.onSelected(includedProperties);
                bladeNavigationService.closeBlade(blade);
            }
        };

        initializeBlade();

    }]);
