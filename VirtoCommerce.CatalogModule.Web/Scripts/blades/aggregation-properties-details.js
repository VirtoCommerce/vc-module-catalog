angular.module('virtoCommerce.catalogModule')
.controller('virtoCommerce.catalogModule.aggregationPropertyDetailsController', ['$scope', 'platformWebApp.dialogService', 'platformWebApp.bladeNavigationService', 'virtoCommerce.catalogModule.browsefilters', function ($scope, dialogService, bladeNavigationService, browseFilters) {
    var blade = $scope.blade;
    blade.updatePermission = 'store:update';
    blade.headIcon = 'fa-gear';

    var attributeType = "Attribute";
    var rangeType = "Range";
    var priceRangeType = "PriceRange";
    blade.propertyTypes = [attributeType, rangeType];

    function initializeBlade() {
        $scope.isValid = true;
        blade.originalProperty = blade.property;
        blade.property = angular.copy(blade.property);
        blade.isLoading = false;
    }

    blade.canChangeType = function () {
        return blade.property.type !== priceRangeType;
    };

    blade.canChangeSize = function () {
        return blade.property.type === attributeType;
    };

    blade.isRange = function () {
        return blade.property.type !== attributeType;
    };

    function isDirty() {
        return !angular.equals(blade.property, blade.originalProperty) && blade.hasUpdatePermission();
    }
    
    blade.onClose = function (closeCallback) {
        bladeNavigationService.showConfirmationIfNeeded(isDirty(), true, blade, $scope.saveChanges, closeCallback, "Save changes", "The property settings have been modified. Do you want to save changes?");
    };

    $scope.saveChanges = function () {
        // TODO: Visual editor should not change string array to a string
        if (blade.property.values && blade.property.values.split) {
            blade.property.values = blade.property.values.split(',');
        }

        angular.copy(blade.property, blade.originalProperty);
        $scope.bladeClose();
    };

    initializeBlade();
}]);
