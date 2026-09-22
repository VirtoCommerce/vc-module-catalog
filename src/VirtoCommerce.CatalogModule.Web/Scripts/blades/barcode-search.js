angular.module('virtoCommerce.catalogModule')
.controller('virtoCommerce.catalogModule.barcodeSearchController',
    ['$scope', '$translate', 'platformWebApp.bladeNavigationService', 'virtoCommerce.catalogModule.barcodeSearch',
    function ($scope, $translate, bladeNavigationService, barcodeSearch) {
        var blade = $scope.blade;
        blade.updatePermission = 'catalog:BrowseFilters:Update';
        blade.headIcon = 'fas fa-barcode';

        var MATCH_FULL_TEXT = 'fullText';
        var MATCH_EXACT = 'exact';
        var FIELD_LABEL_PREFIX = 'catalog.blades.barcode-search.fields.';

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
                buildFieldList();
                blade.isLoading = false;
            }, function (error) {
                blade.isLoading = false;
                bladeNavigationService.setError(getErrorMessage(error), blade);
            });
        }

        function getErrorMessage(error) {
            return (error && error.data && error.data.message) || ('Error ' + (error ? error.status : ''));
        }

        // Builds the single flat row list and freezes its order: the fields of the saved selection first,
        // then the rest, alphabetically by the text each row shows. Only a data load rebuilds it, so
        // checking or unchecking a row never makes it jump.
        function buildFieldList() {
            var available = blade.availableFields || [];
            var savedNames = blade.currentEntity.fields || [];

            blade.selection = {};
            _.each(available, function (field) {
                blade.selection[field.name] = containsName(savedNames, field.name);
            });

            refreshMissingFields();

            var rows = _.map(available, function (field) {
                return {
                    name: field.name,
                    isProductField: field.isProductField,
                    isCollection: field.isCollection,
                    isMissing: false,
                    isVisible: true,
                    text: getRowText(field)
                };
            });

            _.each(blade.missingFields, function (name) {
                blade.selection[name] = true;
                rows.push({
                    name: name,
                    isProductField: false,
                    isCollection: false,
                    isMissing: true,
                    isVisible: true,
                    text: name
                });
            });

            blade.fields = sortRows(rows);

            blade.matchMode = savedNames.length ? MATCH_EXACT : MATCH_FULL_TEXT;
        }

        // Built-in fields are ordered by the label the row shows (SKU / GTIN / MPN), properties by their index name.
        function getRowText(field) {
            if (!field.isProductField) {
                return field.name;
            }

            var key = FIELD_LABEL_PREFIX + field.name;
            var label = $translate.instant(key);
            return label && label !== key ? label : field.name;
        }

        function sortRows(rows) {
            // _.sortBy is stable, so the checked/unchecked split keeps the alphabetical order inside each part.
            var sorted = _.sortBy(rows, function (row) { return (row.text || '').toLowerCase(); });
            var isChecked = function (row) { return !!blade.selection[row.name]; };
            return _.filter(sorted, isChecked).concat(_.reject(sorted, isChecked));
        }

        // Selected fields the product index no longer exposes (property renamed/removed, or not re-indexed yet).
        // They are shown as checked, disabled rows and stop being rendered as soon as they leave the selection
        // (any save drops them); hiding a row never reorders the others.
        function refreshMissingFields() {
            var availableNames = _.pluck(blade.availableFields || [], 'name');
            blade.missingFields = _.filter(blade.currentEntity.fields || [], function (name) {
                return !containsName(availableNames, name);
            });

            _.each(blade.fields || [], function (row) {
                if (row.isMissing) {
                    row.isVisible = containsName(blade.missingFields, row.name);
                }
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
                name: 'platform.commands.save', icon: 'fas fa-save',
                executeMethod: $scope.saveChanges,
                canExecuteMethod: function () { return isDirty() && isValid(); },
                permission: blade.updatePermission
            },
            {
                name: 'platform.commands.reset', icon: 'fa fa-undo',
                executeMethod: function () {
                    blade.currentEntity = angular.copy(blade.origEntity);
                    buildFieldList();
                },
                canExecuteMethod: isDirty
            }
        ];

        initializeBlade();
    }]);
