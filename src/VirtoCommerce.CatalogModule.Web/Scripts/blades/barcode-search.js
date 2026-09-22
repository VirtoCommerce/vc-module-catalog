angular.module('virtoCommerce.catalogModule')
.controller('virtoCommerce.catalogModule.barcodeSearchController',
    ['$scope', 'platformWebApp.bladeNavigationService', 'virtoCommerce.catalogModule.barcodeSearch',
    function ($scope, bladeNavigationService, barcodeSearch) {
        var blade = $scope.blade;
        blade.updatePermission = 'catalog:BrowseFilters:Update';
        blade.headIcon = 'fas fa-barcode';

        var MATCH_FULL_TEXT = 'fullText';
        var MATCH_EXACT = 'exact';

        blade.matchFullText = MATCH_FULL_TEXT;
        blade.matchExact = MATCH_EXACT;

        function initializeBlade() {
            blade.isLoading = true;

            // The picker only offers fields that exist in the product index; free text is never accepted.
            barcodeSearch.getFields({ storeId: blade.storeId }, function (fields) {
                blade.availableFields = fields;
                loadSettings();
            }, function (error) {
                blade.isLoading = false;
                bladeNavigationService.setError(getErrorMessage(error), blade);
            });
        }

        function loadSettings() {
            barcodeSearch.getSettings({ storeId: blade.storeId }, function (data) {
                blade.origEntity = {
                    scannerEnabled: data.scannerEnabled !== false,
                    fields: angular.copy(data.fields || [])
                };
                blade.currentEntity = angular.copy(blade.origEntity);
                buildFieldGroups();
                blade.isLoading = false;
            }, function (error) {
                blade.isLoading = false;
                bladeNavigationService.setError(getErrorMessage(error), blade);
            });
        }

        function getErrorMessage(error) {
            return (error && error.data && error.data.message) || ('Error ' + (error ? error.status : ''));
        }

        function buildFieldGroups() {
            var available = blade.availableFields || [];
            var savedNames = blade.currentEntity.fields || [];

            blade.productFields = _.filter(available, function (x) { return x.isProductField; });
            blade.propertyFields = _.filter(available, function (x) { return !x.isProductField; });

            blade.selection = {};
            _.each(available, function (field) {
                blade.selection[field.name] = containsName(savedNames, field.name);
            });

            refreshMissingFields();

            blade.matchMode = savedNames.length ? MATCH_EXACT : MATCH_FULL_TEXT;
        }

        // Selected fields the product index no longer exposes (property renamed/removed, or not re-indexed yet).
        // They are shown as checked rows and disappear as soon as they leave the selection (any save drops them).
        function refreshMissingFields() {
            var availableNames = _.pluck(blade.availableFields || [], 'name');
            blade.missingFields = _.filter(blade.currentEntity.fields || [], function (name) {
                return !containsName(availableNames, name);
            });
        }

        function containsName(names, name) {
            var lowerName = (name || '').toLowerCase();
            return _.some(names || [], function (x) {
                return (x || '').toLowerCase() === lowerName;
            });
        }

        function collectSelectedFields() {
            return _.chain(blade.availableFields || [])
                .filter(function (field) { return blade.selection[field.name]; })
                .map(function (field) { return field.name; })
                .value();
        }

        blade.onMatchModeChanged = function () {
            blade.currentEntity.fields = blade.matchMode === MATCH_EXACT ? collectSelectedFields() : [];
            refreshMissingFields();
        };

        blade.onFieldSelectionChanged = function () {
            blade.currentEntity.fields = collectSelectedFields();
            refreshMissingFields();
        };

        function isDirty() {
            return !angular.equals(blade.currentEntity, blade.origEntity) && blade.hasUpdatePermission();
        }

        function isValid() {
            return blade.matchMode !== MATCH_EXACT || (blade.currentEntity.fields || []).length > 0;
        }

        $scope.saveChanges = function () {
            blade.isLoading = true;
            barcodeSearch.saveSettings({ storeId: blade.storeId }, blade.currentEntity, function () {
                initializeBlade();
            }, function (error) {
                blade.isLoading = false;
                bladeNavigationService.setError(getErrorMessage(error), blade);
            });
        };

        blade.onClose = function (closeCallback) {
            bladeNavigationService.showConfirmationIfNeeded(isDirty() && isValid(), true, blade, $scope.saveChanges, closeCallback,
                "catalog.dialogs.barcode-search-save.title", "catalog.dialogs.barcode-search-save.message");
        };

        blade.toolbarCommands = [
            {
                name: 'platform.commands.ok', icon: 'fas fa-check',
                executeMethod: $scope.saveChanges,
                canExecuteMethod: function () { return isDirty() && isValid(); },
                permission: blade.updatePermission
            },
            {
                name: 'platform.commands.reset', icon: 'fa fa-undo',
                executeMethod: function () {
                    blade.currentEntity = angular.copy(blade.origEntity);
                    buildFieldGroups();
                },
                canExecuteMethod: isDirty
            }
        ];

        initializeBlade();
    }]);
