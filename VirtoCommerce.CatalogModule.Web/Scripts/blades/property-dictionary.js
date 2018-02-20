angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.propertyDictionaryController',
    ['$scope', '$filter', 'platformWebApp.dialogService', 'platformWebApp.settings', function ($scope, $filter, dialogService, settings) {
        var dictionaryValues;
        var pb = $scope.blade.parentBlade;
        $scope.pb = pb;
        $scope.duplicate = false;
        $scope.newValue = null;

        var newSingleValueAttrs = { dictionary: false, isNew: true, isReadOnly: false, required: true, multivalue: false, duplicate: false, values: [{ value: null }] };
        var newMultilanguageValueAttrs = { dictionary: false, isNew: true, isReadOnly: false, required: false, multivalue: false, duplicate: false };

        var formScope;
        $scope.setForm = function (form) {
            formScope = form;
        }

        $scope.exposeAlias = false;
        var promise = settings.getSettings({
            id: 'VirtoCommerce.Catalog'
        }).$promise;
        promise.then(function (promiseData) {
            var temp = _.findWhere(promiseData, { name: 'Catalog.ExposeAliasInDictionary' }).value;
            $scope.exposeAlias = temp.toLowerCase() === 'true';
        });

        listValueValidator = function (newValues) {
            if (newValues) {
                // Summary form validation to make button 'Save' enabled or disabled
                $scope.pb.currentEntity.$isValid = (!formScope || formScope.$valid) && !_.findWhere($scope.dictionaryForProperty, { duplicate : true });

                if ($scope.pb.currentEntity.$isValid)
                    dictionaryValues[newValues.index] = newValues.values[0];
            }
        }

        valueValidator = function () {
            if (pb.currentEntity.multilanguage) {
                $scope.newValue.duplicate = !_.all($scope.newValue.values, function (value) {
                    return dictValueValidator(value.value, value);
                });
                return !$scope.newValue.duplicate;
            } else {
                var value = $scope.newValue.values[0].value;
                $scope.newValue.duplicate = !_.all(dictionaryValues, function (item) { return item.value !== value; });
                return !$scope.newValue.duplicate;
            }
        }

        selectedValueValidator = function () {
            if ($scope.selectedItem) {
                var editValue = $scope.selectedItem.values[0];
                $scope.selectedItem.duplicate = _.any($scope.dictionaryForProperty, function (item) { return editValue != item.values[0] && item.values[0].value === editValue.value; });
                return !$scope.selectedItem.duplicate;
            }
            else
                return true;
        }

        dictValueValidator = function (value, editEntity) {
            var testEntity = angular.copy(editEntity);
            testEntity.value = value;
            return _.all(dictionaryValues, function (item) {
                return item.value !== value || item.languageCode !== testEntity.languageCode || ($scope.selectedItem && _.some($scope.selectedItem.values, function (x) {
                    return angular.equals(x.id, item.id);
                }));
            });
        }

        $scope.cancel = function () {
            $scope.selectedItem = undefined;
            resetNewValue();
        }

        $scope.add = function (form) {
            if (form.$valid && !$scope.newValue.duplicate) {
                if ($scope.newValue.values) {
                    if ($scope.selectedItem) { // editing existing values
                        _.each($scope.newValue.values, function (value) {
                            var existingValue = _.findWhere(dictionaryValues, { alias: $scope.exposeAlias ? $scope.newValue.alias : value.alias, languageCode: value.languageCode });
                            if (value.value) {
                                if (existingValue) {
                                    existingValue.alias = $scope.exposeAlias ? $scope.newValue.alias : $scope.newValue.values[0].value;
                                    existingValue.value = value.value;
                                } else {
                                    dictionaryValues.push({
                                        alias: $scope.exposeAlias ? $scope.newValue.alias : $scope.newValue.values[0].value,
                                        languageCode: value.languageCode,
                                        propertyId: pb.currentEntity.id,
                                        value: value.value
                                    });
                                }
                            } else if (existingValue) {
                                $scope.delete(dictionaryValues.indexOf(existingValue));
                            }
                        });
                        $scope.selectedItem = undefined;
                    } else { // adding new values
                        _.each($scope.newValue.values, function (value) {
                            if (value.value) {
                                dictionaryValues.push({
                                    alias: $scope.exposeAlias ? $scope.newValue.alias : $scope.newValue.values[0].value,
                                    languageCode: value.languageCode,
                                    propertyId: pb.currentEntity.id,
                                    value: value.value
                                });
                            }
                        });
                    }
                    initializeDictionaryValues();
                } else {
                    $scope.newValue.alias = $scope.newValue.alias ? $scope.newValue.alias : $scope.newValue.value;
                    dictionaryValues.push($scope.newValue);
                }
                resetNewValue($scope.newValue.languageCode);
                form.$setPristine();
            }
        };

        $scope.selectItem = function (listItem) {
            $scope.selectedItem = listItem;

            if (pb.currentEntity.multilanguage) {
                resetNewValue();
            }
        };

        $scope.delete = function (index) {
            dictionaryValues.splice(index, 1);
            $scope.selectedItem = undefined;

            initializeDictionaryValues();
        };

        $scope.deleteMultilanguage = function (key) {
            var selectedValues = _.where(dictionaryValues, { alias: key });
            _.forEach(selectedValues, function (value) {
                dictionaryValues.splice(dictionaryValues.indexOf(value), 1);
            });
            initializeDictionaryValues();
            $scope.selectedItem = undefined;
            resetNewValue(pb.defaultLanguage);
        };

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

        $scope.checkAll = function (selected) {
            angular.forEach(getValuesList(), function (item) {
                item.selected = selected;
            });
        };

        function getValuesList() {
            return pb.currentEntity.multilanguage ? $scope.groupedValues : $scope.dictionaryForProperty;
        }

        function resetNewValue(locale) {
            if (pb.currentEntity.multilanguage) {
                // generate input fields for ALL languages
                var defaultLanguageCode = pb.defaultLanguage;
                var values = [{ languageCode: defaultLanguageCode, value: null }];
                _.each(pb.languages, function (lang) {
                    if (lang !== defaultLanguageCode) {
                        values.push({ languageCode: lang, value: null });
                    }
                });
                var alias = "";

                // add current values
                if ($scope.selectedItem) {
                    _.each($scope.selectedItem.values, function (value) {
                        var foundValue = _.findWhere(values, { languageCode: value.languageCode });
                        if (foundValue) {
                            angular.extend(foundValue, value);
                        }
                    });
                    alias = $scope.exposeAlias ? $scope.selectedItem.alias : "";
                }

                $scope.newValue = Object.assign({ }, pb.currentEntity, newMultilanguageValueAttrs, { values: values, alias: alias });
            } else {
                $scope.newValue = Object.assign({ }, pb.currentEntity, newSingleValueAttrs);
            }
        }

        function isItemsChecked() {
            return _.any(getValuesList(), function (x) { return x.selected; });
        }

        function deleteChecked() {
            var dialog = {
                id: "confirmDeleteItem",
                title: "catalog.dialogs.dictionary-values-delete.title",
                message: "catalog.dialogs.dictionary-values-delete.message",
                callback: function (remove) {
                    if (remove) {
                        var selection = $filter('filter')(getValuesList(), { selected: true }, true);
                        angular.forEach(selection, function (listItem) {
                            if (pb.currentEntity.multilanguage) {
                                $scope.deleteMultilanguage(listItem.alias);
                            } else {
                                var index = $scope.dictionaryForProperty.indexOf(listItem);
                                $scope.delete(index);
                            }
                        });
                    }
                }
            }
            dialogService.showConfirmationDialog(dialog);
        }

        function initializeDictionaryValues() {
            dictionaryValues = pb.currentEntity.dictionaryValues;
            _.each(dictionaryValues, function (x) {
                if (!x.alias) {
                    x.alias = x.value;
                }
            });

            $scope.dictionaryValues = dictionaryValues;
            $scope.groupedValues = _.map(_.groupBy(dictionaryValues, 'alias'), function (values, key) {
                return { alias: key, values: values };
            });

            // the dictionary for a list of items
            $scope.dictionaryForProperty = [];

            _.each(dictionaryValues, function (item, index) {
                var propValue = Object.assign({}, pb.currentEntity, newSingleValueAttrs);
                propValue.values = [item];
                propValue.alias = item.value;
                propValue.index = index;

                $scope.dictionaryForProperty.push(propValue);

                $scope.$watch('dictionaryForProperty[' + index + ']', listValueValidator, true);
            });

            resetNewValue(pb.defaultLanguage);
        }

        $scope.$watch('blade.parentBlade.currentEntity.dictionaryValues', initializeDictionaryValues);
        $scope.$watch('blade.parentBlade.currentEntity.multilanguage', initializeDictionaryValues);
        $scope.$watch('newValue.values', valueValidator, true);
        $scope.$watch('selectedItem.values', selectedValueValidator, true);

        $scope.blade.isLoading = false;
    }]);
