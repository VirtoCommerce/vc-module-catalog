angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.propertyDictionaryDetailsController',
        ['$scope', '$filter', 'platformWebApp.dialogService', 'platformWebApp.settings', 'platformWebApp.i18n', 'platformWebApp.common.languages',
            function ($scope, $filter, dialogService, settings, i18n, languages) {
                var blade = $scope.blade;
                var pb = $scope.blade.parentBlade;
                var languagesList = languages.query();

                $scope.pb = pb;
                $scope.defaultLanguage = pb.parentBlade.defaultLanguage;

                blade.headIcon = 'fa-book';

                $scope.blade.isLoading = false;

                $scope.getLanguageName = function (languageCode) {
                    return languagesList.find(x => x.id.contains(languageCode.slice(0, 2))).name;
                }

                blade.toolbarCommands = [
                    {
                        name: "platform.commands.save",
                        icon: 'fa fa-save',
                        executeMethod: function () {
                            saveChanges();
                        },
                        canExecuteMethod: function () {
                            return canSave();
                        }
                    },
                    {
                        name: "platform.commands.delete",
                        icon: 'fa fa-trash-o',
                        executeMethod: function () {
                            deleteProperty();
                        },
                        canExecuteMethod: function () {
                            return !blade.isNew;
                        }
                    }
                ];

                blade.formScope = null;
                $scope.setForm = function (form) { blade.formScope = form; }

                function canSave() {
                    return blade.formScope && blade.formScope.$valid;
                }

                function deleteProperty() {
                    var dictionary = pb.parentBlade.currentEntity.dictionaryValues;
                    var item = dictionary.find(x => x.alias == blade.property.alias);
                    var index = dictionary.indexOf(item);
                    dictionary.splice(index, 1);

                    pb.refresh();
                    $scope.bladeClose();
                }

                function saveChanges() {
                    blade.isLoading = true;
                    if (blade.isNew) {
                        var dictionary = pb.parentBlade.currentEntity.dictionaryValues;
                        var item = dictionary.find(x => x.alias == blade.property.alias);
                        var index = dictionary.indexOf(item);
                        if (index != -1) {
                            alert('already exist');
                            blade.isLoading = false;
                            return false;
                        }

                        _.each(blade.property.values, function (prop) {
                            var property = {
                                value: prop.value,
                                alias: blade.property.alias,
                                languageCode: prop.languageCode
                            }
                            pb.parentBlade.currentEntity.dictionaryValues.push(property);
                        });

                        pb.refresh();
                    } else {
                        blade.property.values.map(function (prop) {
                            if (prop.id) {   // existed property
                                var existedProp = pb.parentBlade.currentEntity.dictionaryValues.find(x=>x.id == prop.id);
                                existedProp.alias = prop.alias;
                                existedProp.value = prop.value;
                            } else {    // new property
                                var property = {
                                    value: prop.value,
                                    alias: blade.property.alias,
                                    languageCode: prop.languageCode
                                }
                                pb.parentBlade.currentEntity.dictionaryValues.push(property);
                            }
                        })
                    }
                    $scope.bladeClose();
                };
            }]);
