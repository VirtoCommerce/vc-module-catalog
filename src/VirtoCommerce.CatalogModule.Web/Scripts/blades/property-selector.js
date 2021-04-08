angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.propertySelectorController', ['$scope', 'platformWebApp.bladeNavigationService', function ($scope, bladeNavigationService) {
        var blade = $scope.blade;
        blade.existingFilteredProperties = [];
        blade.isLoading = true;
        const maxPreviewFiltersCount = 10;

        function initializeBlade() {
            blade.title = 'catalog.blades.property-selector.title';
            blade.headIcon = 'fa fa-folder';

            var allProperties = angular.copy(blade.properties);

            allProperties = _.sortBy(allProperties, 'group', 'name');

            blade.existingFilteredProperties = _.first(blade.selectedProperties, maxPreviewFiltersCount);
            if (blade.existingFilteredProperties.length >= maxPreviewFiltersCount) {
                blade.existingFilteredProperties.push('...');
            }

            var selectedProperties = _.sortBy(_.filter(allProperties,
                function (property) { return blade.selectedProperties.includes(property.name.toLowerCase()); }), 'name');


            blade.allEntities = _.groupBy(allProperties, 'group');
            blade.selectedEntities = _.groupBy(selectedProperties, 'group');

            blade.isLoading = false;
            
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
                blade.onSelected(includedProperties);
                bladeNavigationService.closeBlade(blade);
            }
        };

        initializeBlade();

    }]);
