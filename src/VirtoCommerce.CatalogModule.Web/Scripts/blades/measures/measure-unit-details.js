angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.measureUnitDetailsController',
        ['$scope', '$translate', 'platformWebApp.dialogService', 'platformWebApp.metaFormsService', 'platformWebApp.bladeNavigationService',
            function ($scope, $translate, dialogService, metaFormsService, bladeNavigationService) {
                var blade = $scope.blade;

                blade.metaFields = blade.metaFields && blade.metaFields.length ? blade.metaFields : metaFormsService.getMetaFields('measureUnitDetails');

                blade.origEntity = blade.currentEntity;
                blade.currentEntity = angular.copy(blade.origEntity);       

                if (blade.isNew) {
                    blade.title = 'catalog.blades.measure-unit-details.title-new';
                    blade.currentEntity = {};
                } else {
                    blade.title = blade.currentEntity.name + $translate.instant('catalog.blades.measure-unit-details.title');
                    blade.subtitle = 'catalog.blades.measure-unit-details.subtitle';
                }

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
                        return !blade.currentEntity.isNew;
                    }
                }];

                blade.isLoading = false;

                blade.onClose = function (closeCallback) {
                    bladeNavigationService.showConfirmationIfNeeded(isDirty(), canSave(), blade, $scope.saveChanges, closeCallback, "core.dialogs.address-save.title", "core.dialogs.address-save.message");
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
                        blade.confirmChangesFn(blade.currentEntity);
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
                        title: "catalog.dialogs.phone-delete.title",
                        message: "catalog.dialogs.phone-delete.message",
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
