angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.measureUnitDetailsController',
        ['$scope', 'platformWebApp.dialogService', 'platformWebApp.metaFormsService', 'platformWebApp.bladeNavigationService',
            function ($scope, dialogService, metaFormsService, bladeNavigationService) {
                var blade = $scope.blade;

                blade.metaFields = blade.metaFields && blade.metaFields.length ? blade.metaFields : metaFormsService.getMetaFields('measureUnitDetails');

                if (blade.isNew) {
                    blade.title = 'catalog.blades.measure-unit-details.title-new';
                    blade.currentEntity = {};
                    blade.origEntity = {};
                } else {
                    blade.origEntity = blade.currentEntity;
                    blade.currentEntity = angular.copy(blade.origEntity);
                    blade.title ='catalog.blades.measure-unit-details.title';
                    blade.titleValues = { name: blade.currentEntity.name };
                    blade.subtitle = 'catalog.blades.measure-unit-details.subtitle';

                    blade.toolbarCommands = [{
                        name: "platform.commands.reset", icon: 'fa fa-undo',
                        executeMethod: function () {
                            angular.copy(blade.origEntity, blade.currentEntity);
                        },
                        canExecuteMethod: isDirty
                    }, {
                        name: "platform.commands.delete", icon: 'fas fa-trash-alt',
                        executeMethod: deleteEntry,
                        canExecuteMethod: function () {
                            return !blade.isNew;
                        }
                    }, {
                        name: "catalog.commands.default", icon: 'fas fa-flag',
                        executeMethod: function () {
                            blade.currentEntity.isDefault = true;
                            blade.setDefaultMeasureUnitFn(blade.currentEntity);
                        },
                        canExecuteMethod: function () {
                            return !blade.currentEntity.isDefault;
                        }
                    }];
                }

                blade.isLoading = false;

                blade.onClose = function (closeCallback) {
                    bladeNavigationService.showConfirmationIfNeeded(isDirty(), canSave(), blade, $scope.saveChanges, closeCallback, "catalog.dialogs.measure-unit-save.title", "catalog.dialogs.measure-unit-save.message");
                };

                $scope.setForm = function (form) {
                    $scope.formScope = form;
                };

                $scope.isValid = function () {
                    return $scope.formScope && $scope.formScope.$valid;
                };

                $scope.cancelChanges = function () {
                    $scope.bladeClose();
                };

                $scope.saveChanges = function () {
                    if (blade.confirmChangesFn) {
                        blade.confirmChangesFn(blade.currentEntity, blade.isNew);
                    }
                    angular.copy(blade.currentEntity, blade.origEntity);
                    $scope.bladeClose();
                };

                function isDirty() {
                    return !angular.equals(blade.currentEntity, blade.origEntity);
                }

                function canSave() {
                    return isDirty();
                }

                function deleteEntry() {
                    var dialog = {
                        id: "confirmDelete",
                        title: "catalog.dialogs.measure-unit-delete.title",
                        message: "catalog.dialogs.measure-unit-delete.message",
                        callback: function (remove) {
                            if (remove) {
                                if (blade.deleteFn) {
                                    blade.deleteFn(blade.currentEntity);
                                }
                                $scope.bladeClose();
                            }
                        }
                    }
                    dialogService.showConfirmationDialog(dialog);
                }
            }
        ]
    );
