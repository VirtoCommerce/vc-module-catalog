angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.propertyDictionaryController',
    ['$scope', 'platformWebApp.bladeNavigationService', '$filter', 'platformWebApp.dialogService', 'platformWebApp.settings', function ($scope, bladeNavigationService, $filter, dialogService, settings) {
        $scope.newValue = null;
        $scope.selectedItem = null;
        $scope.dictionaryPropertyValues = [];
        $scope.pb = $scope.blade.parentBlade;
        $scope.isValid = false;
        $scope.modified = false;

        var propertyValueAttrs = { dictionary: false, isReadOnly: false, required: true, multivalue: false, duplicate: false };

        var formScope;
        $scope.setForm = function (form) {
            formScope = form;

            // catchs an event when a new-panel is resized

            $scope.newElem = property_dictionary_new;
            $scope.window = window;

            $scope.$watch('newElem.offsetHeight', updateListHeight, true);
            $scope.$watch('window.innerHeight', updateListHeight, true);            
        }

        function updateListHeight() {
            if (property_dictionary) {
                var bladeElem = $(property_dictionary).closest('.blade-container')[0],
                    newElem = property_dictionary.children[0],
                    listElem = property_dictionary.children[1],
                    bottomElem = property_dictionary.children[2];

                var height = bladeElem.offsetHeight - newElem.offsetHeight - bottomElem.offsetHeight;

                $(listElem).css('height', height + 'px');
            }
        }

        $scope.selectItem = function (item) {
            $scope.selectedItem = item;
        }

        var dictionaryValues = angular.copy($scope.pb.currentEntity.dictionaryValues);

        // INITIALIZATION

        function initialize() {
            $scope.dictionaryPropertyValues = [];

            if ($scope.pb.currentEntity.multilanguage) {
                initializeDictionaryValuesForMultilanguage();
            }
            else {
                initializeDictionaryValuesForSingle();
            }

            resetNewValue();
        }

        function initializeDictionaryValuesForSingle() {
            _.each(dictionaryValues, function (item, index) {
                insertValueToDictionary({ values: [item], alias: item.value });
            });
        }

        function initializeDictionaryValuesForMultilanguage() {
            var groupedValues = _.map(_.groupBy(dictionaryValues, 'alias'), function (values, key) {
                return { alias: key, values: values };
            });

            _.each(groupedValues, function (item, index) {
                // Appending values with all languages
                _.each($scope.pb.languages, function (lang) {
                    if (!_.findWhere(item.values, { languageCode: lang })) {
                        item.values.push({ alias: item.alias, languageCode: lang, value: null });
                    }
                });

                insertValueToDictionary(item);
            });
        }

        function insertValueToDictionary(item) {
            var propValue = Object.assign(
                {},
                $scope.pb.currentEntity,
                propertyValueAttrs,
                item
            );

            _.each(propValue.values, function (v, i) {
                // Storing an original value to catch a change
                v.originalValue = v.value;

                // Catching changes
                $scope.$watch('dictionaryPropertyValues[' + $scope.dictionaryPropertyValues.length + '].values[' + i + ']', listValueValidator, true);
            });

            $scope.dictionaryPropertyValues.push(propValue);
        }

        // RESET A NEW VALUE

        function resetNewValue() {
            var values;

            if ($scope.pb.currentEntity.multilanguage) {
                values = resetNewValueForMultilanguage();
            }
            else {
                values = [{ value: null }];
            }

            $scope.newValue = Object.assign(
                { isNew: true },
                $scope.pb.currentEntity,
                propertyValueAttrs,
                { values: values }
            );
        }

        function resetNewValueForMultilanguage() {
            var defaultLanguageCode = $scope.pb.defaultLanguage;
            var values = [{ languageCode: defaultLanguageCode, value: null }];

            _.each($scope.pb.languages, function (lang) {
                if (lang !== defaultLanguageCode) {
                    values.push({ languageCode: lang, value: null });
                }
            });

            return values;
        }

        // ADDING

        $scope.add = function () {
            $scope.newValue.alias = $scope.newValue.values[0].value;
            _.forEach($scope.newValue.values, function (value) {
                value.alias = $scope.newValue.alias;
            });

            $scope.dictionaryPropertyValues.push($scope.newValue);

            $scope.modify(true);

            resetNewValue();
        }

        // VALIDATIONS

        $scope.modify = function (modified) {
            $scope.modified = $scope.modified || modified;
            $scope.isValid = $scope.modified && !$scope.duplicate && !(formScope && formScope.$invalid);

            return $scope.isValid;
        }

        newValueValidator = function () {
            $scope.newValue.duplicate = duplicateValidator($scope.newValue);
        }

        listValueValidator = function (newValues) {
            $scope.duplicate = false;
            _.forEach($scope.dictionaryPropertyValues, function (item) {
                item.duplicate = duplicateValidator(item);
                $scope.duplicate = $scope.duplicate || item.duplicate;
            });

            $scope.modify(newValues && ((!newValues.originalValue && newValues.value) || (newValues.originalValue && newValues.value !== newValues.originalValue)));
        }

        function duplicateValidator(propertyValue) {
            return _.any($scope.dictionaryPropertyValues, function (itemValue) {
                return itemValue != propertyValue && propertyValueIsDuplicated(propertyValue, itemValue);
            });
        }

        function propertyValueIsDuplicated(propertyValue, itemValue) {
            return _.any(propertyValue.values, function (targetValue) {
                return _.any(itemValue.values, function (dictionaryValue) {
                    return targetValue.value === dictionaryValue.value && (!$scope.pb.currentEntity.multilanguage || targetValue.languageCode === dictionaryValue.languageCode);
                });
            });
        }

        // DELETION

        $scope.delete = function (value) {
            $scope.dictionaryPropertyValues.splice($scope.dictionaryPropertyValues.indexOf(value), 1);
            $scope.modify(true);
        };

        function deleteChecked() {
            var dialog = {
                id: "confirmDeleteItem",
                title: "catalog.dialogs.dictionary-values-delete.title",
                message: "catalog.dialogs.dictionary-values-delete.message",
                callback: function (remove) {
                    if (remove) {
                        var selection = $filter('filter')($scope.dictionaryPropertyValues, { selected: true }, true);

                        angular.forEach(selection, function (item) {
                            $scope.delete(item);
                        });
                    }
                }
            }

            dialogService.showConfirmationDialog(dialog);
        }

        // SETTINGS

        $scope.blade.headIcon = 'fa-book';

        $scope.blade.toolbarCommands = [
            {
                name: "platform.commands.delete", icon: 'fa fa-trash-o',
                executeMethod: function () {
                    deleteChecked();
                },
                canExecuteMethod: function () {
                    return isItemsChecked();
                }
            }
        ];

        function isItemsChecked() {
            return _.any($scope.dictionaryPropertyValues, function (x) { return x.selected; });
        }

        $scope.checkAll = function (selected) {
            angular.forEach($scope.dictionaryPropertyValues, function (item) {
                item.selected = selected;
            });
        };

        // SAVING CHANGES

        function save() {
            var dictionary = [];

            _.forEach($scope.dictionaryPropertyValues, function (item) {
                _.forEach(item.values, function (value) {
                    dictionary.push(value);
                });
            });

            $scope.pb.currentEntity.dictionaryValues = dictionary;
            $scope.pb.currentEntity.$modified = true;

            $scope.modified = false;
        }

        // FORM ACTIONS

        $scope.blade.onClose = function (closeCallback) {
            bladeNavigationService.showConfirmationIfNeeded(
                $scope.modified,
                true,
                $scope.blade,
                $scope.saveChanges,
                closeCallback,
                "Save changes", "The property dictionary have been modified. Do you want to save changes?"
            );
        };

        $scope.saveChanges = function () {
            //angular.copy(blade.property, blade.originalProperty);
            save();
            $scope.bladeClose();
        };

        // WATCHERS

        $scope.$watch('blade.parentBlade.currentEntity.multilanguage', initialize);
        //$scope.$watch('dictionaryPropertyValues', listValueValidator);
        $scope.$watch('newValue.values', newValueValidator, true);

        $scope.blade.isLoading = false;

    }]);
