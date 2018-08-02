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

                $scope.getLanguageName = function (languageCode) {
                    return languagesList.find(x => x.id.contains(languageCode.slice(0, 2))).name;
                }

                blade.toolbarCommands = [
                    {
                        name: "platform.commands.save",
                        icon: 'fa fa-save',
                        executeMethod: $scope.saveChanges,
                        canExecuteMethod: canSave
                    },
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
                            if (!blade.property.values.find(x => x.languageCode == lang)) {
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
                                if (!blade.property.values.find(x => x.languageCode == lang)) {
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

                function canSave() {
                    $scope.isValid = (blade.isNew || isDirty()) && blade.formScope && blade.formScope.$valid;
                    return $scope.isValid;
                }

                blade.onClose = function (closeCallback) {
                    bladeNavigationService.showConfirmationIfNeeded(isDirty(), canSave(), blade, $scope.saveChanges, closeCallback, "catalog.dialogs.property-save.title", "catalog.dialogs.property-save.message");
                };

                function deleteProperty() {
                    var dialog = {
                        id: "confirmDeletePropertyValue",
                        title: "catalog.dialogs.dictionary-values-delete.title",
                        message: "catalog.dialogs.dictionary-values-delete.message",
                        callback: function (remove) {
                            if (remove) {
                                var item = existedDictionary.find(x => x.alias == blade.currentEntity.alias);
                                var index = existedDictionary.indexOf(item);
                                existedDictionary.splice(index, 1);

                                pb.refresh();
                                $scope.bladeClose();
                            }
                        }
                    }
                    dialogService.showConfirmationDialog(dialog);
                }

                $scope.saveChanges = function() {
                    blade.isLoading = true;
                    if (blade.isNew) {
                        _.each(blade.currentEntity.values, function (prop) {
                            prop.alias = blade.currentEntity.alias;
                            prop.languageCode = !prop.languageCode ? $scope.defaultLanguage : prop.languageCode;
                            existedDictionary.push(prop);
                        });
                    } else {
                        blade.currentEntity.values.map(function (prop) {
                            var existedProp = existedDictionary.find(x => x.alias == blade.origEntity.alias && x.languageCode == prop.languageCode);
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
                    blade.isLoading = false;
                };

                $scope.dictValueValidator = function (value) {
                    if (blade.isNew) {
                        var item = existedDictionary.find(x => x.alias == value);
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
