angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.propertyDictionaryDetailsController',
        ['$scope', '$filter', 'platformWebApp.dialogService', 'platformWebApp.settings','platformWebApp.i18n', 'platformWebApp.common.languages',
            function ($scope, $filter, dialogService, settings, i18n, languages) {
                var blade = $scope.blade;
                var pb = $scope.blade.parentBlade;
                var languagesList = languages.query();
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
                            deleteTask();
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

                function saveChanges() {
                    blade.isLoading = true;
                    if (blade.isNew) {
                        var property  = {
                            value: blade.property.value,
                            alias: blade.property.alias,
                            languageCode: pb.parentBlade.defaultLanguage
                        }
                        pb.parentBlade.currentEntity.dictionaryValues.push(property);
                        pb.refresh();
                    } else {
                        return taskApi.update(blade.currentEntity,
                            function () {
                                blade.refresh(true);
                            }).$promise;
                    }
                    $scope.bladeClose();
                };
            }]);
