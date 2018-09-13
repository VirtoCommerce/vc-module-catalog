angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.propertyDictionaryDetailsController',
        ['$scope', 'platformWebApp.dialogService', 'platformWebApp.bladeNavigationService', 'platformWebApp.common.languages',
            function ($scope, dialogService, bladeNavigationService, languages) {
                var blade = $scope.blade;
                var pb = $scope.blade.parentBlade;
                var languagesList = languages.query();
                var existedDictionary = pb.parentBlade.currentEntity.dictionaryValues;

                blade.headIcon = 'fa-book';

                $scope.pb = pb;
                $scope.defaultLanguage = pb.parentBlade.defaultLanguage;
                $scope.isValid = true;
                $scope.blade.isLoading = false;
                $scope.validationRules = pb.parentBlade.currentEntity.validationRule;

                $scope.getLanguageName = function (languageCode) {
                    return _.find(languagesList, function (item) { return item.id.contains(languageCode.slice(0, 2)) }).name;
                }

                blade.toolbarCommands = [
                    {
                        name: "platform.commands.delete",
                        icon: 'fa fa-trash-o',
                        executeMethod: deleteProperty,
                        canExecuteMethod: function () {
                            return !blade.isNew;
                        }
                    }
                ];

                blade.formScope = null;
                $scope.setForm = function (form) { blade.formScope = form; }

                function initializeBlade() {
                    if (blade.isNew) {
                        blade.property = {
                            alias: "",
                            values: []
                        };
                    }

                    if (pb.parentBlade.currentEntity.multilanguage) {
                        _.each(pb.parentBlade.parentBlade.languages, function (lang) {
                            if (!_.find(blade.property.values, function (item) { return item.languageCode == lang })) {
                                blade.property.values.push({
                                    value: "",
                                    languageCode: lang,
                                    propertyId: pb.parentBlade.currentEntityId,
                                    alias: ""
                                })
                            }
                        });
                    }

                    if (!blade.isNew) {
                        if (pb.parentBlade.currentEntity.multilanguage) {
                            _.each(pb.parentBlade.parentBlade.languages, function (lang) {
                                if (!_.find(blade.property.values, function (item) { return item.languageCode == lang })) {
                                    blade.property.values.push({
                                        value: "",
                                        languageCode: lang,
                                        propertyId: pb.parentBlade.currentEntityId,
                                        alias: blade.property.alias
                                    })
                                }
                            });
                        }
                    }

                    blade.currentEntity = angular.copy(blade.property);
                    blade.origEntity = blade.property;
                    blade.isLoading = false;
                };

                function isDirty() {
                    return !angular.equals(blade.currentEntity, blade.origEntity) && blade.hasUpdatePermission();
                }

                $scope.$watch("blade.currentEntity", function () {
                    $scope.isValid = isDirty() && blade.formScope && blade.formScope.$valid;
                }, true);

                blade.onClose = function (closeCallback) {
                    bladeNavigationService.showConfirmationIfNeeded(isDirty(), $scope.isValid, blade, $scope.saveChanges, closeCallback, "catalog.dialogs.property-save.title", "catalog.dialogs.property-save.message");
                };

                function deleteProperty() {
                    var dialog = {
                        id: "confirmDeletePropertyValue",
                        title: "catalog.dialogs.dictionary-values-delete.title",
                        message: "catalog.dialogs.dictionary-values-delete.message",
                        callback: function (remove) {
                            if (remove) {
                                var item = _.find(existedDictionary, function (item) { return item.alias == blade.currentEntity.alias });
                                var index = existedDictionary.indexOf(item);
                                existedDictionary.splice(index, 1);

                                pb.refresh();
                                $scope.bladeClose();
                            }
                        }
                    }
                    dialogService.showConfirmationDialog(dialog);
                }

                $scope.saveChanges = function () {
                    blade.isLoading = true;
                    if (blade.isNew) {
                        _.each(blade.currentEntity.values, function (prop) {
                            prop.alias = blade.currentEntity.alias;
                            prop.languageCode = !prop.languageCode ? $scope.defaultLanguage : prop.languageCode;
                            existedDictionary.push(prop);
                        });
                    } else {
                        blade.currentEntity.values.map(function (prop) {
                            var existedProp = _.find(existedDictionary, function (item) { return item.alias == blade.origEntity.alias && item.languageCode == prop.languageCode });
                            if (existedProp) {
                                existedProp.alias = blade.currentEntity.alias;
                                existedProp.value = prop.value;
                            } else {
                                prop.alias = blade.currentEntity.alias;
                                existedDictionary.push(prop);
                            }
                        })
                    }

                    pb.refresh();
                    if (blade.isNew) {
                        blade.isNew = false;
                    }

                    blade.property = blade.currentEntity;
                    initializeBlade();
                    $scope.isValid = false;
                    blade.isLoading = false;
                    $scope.bladeClose();
                };

                $scope.dictValueValidator = function (value) {
                    if (blade.isNew) {
                        var item = _.find(existedDictionary, function (item) { return item.alias == value });
                        var index = existedDictionary.indexOf(item);
                        if (index != -1) {
                            return false;
                        }
                    }
                    return true;
                }

                $scope.$watch('pb.parentBlade.currentEntity.multilanguage', initializeBlade);
                initializeBlade();
            }]);
