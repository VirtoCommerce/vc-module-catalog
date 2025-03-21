angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.measureDetailsController',
        ['$scope', '$translate', 'virtoCommerce.catalogModule.measures', 'platformWebApp.bladeNavigationService', 'platformWebApp.dialogService', 'platformWebApp.metaFormsService',
            function ($scope, $translate, measures, bladeNavigationService, dialogService, metaFormsService) {
                var blade = $scope.blade;
                blade.headIcon = 'fas fa-ruler-combined';
                blade.updatePermission = 'measures:update';

                blade.metaFields = metaFormsService.getMetaFields("measureDetails");

                blade.toolbarCommands = [
                    {
                        name: "platform.commands.save",
                        icon: 'fas fa-save',
                        executeMethod: function () {
                            $scope.saveChanges();
                        },
                        canExecuteMethod: canSave,
                        permission: blade.updatePermission
                    }
                ];

                if (blade.isNew) {
                    blade.title = 'catalog.blades.measure-details.title-new';
                } else {
                    blade.title = 'catalog.blades.measure-details.title';
                    blade.titleValues = { name: blade.currentEntity.name };
                    blade.subtitle = 'catalog.blades.measure-details.subtitle';

                    blade.toolbarCommands.push(
                        {
                            name: "platform.commands.reset",
                            icon: 'fa fa-undo',
                            executeMethod: function () {
                                angular.copy(blade.originalEntity, blade.currentEntity);
                            },
                            canExecuteMethod: isDirty,
                            permission: blade.updatePermission
                        }
                    );
                }

                $scope.setForm = function (form) {
                    $scope.formScope = blade.formScope = form;
                }

                $scope.saveChanges = function () {
                    blade.isLoading = true;

                    if (blade.isNew) {
                        measures.createMeasures([blade.currentEntity],
                            function () {
                                blade.parentBlade.refresh(true);
                                angular.copy(blade.currentEntity, blade.originalEntity);
                                $scope.bladeClose();
                            },
                            function (error) {
                                bladeNavigationService.setError(`${error.status}: ${error.statusText}`, blade);

                                var errorDialog = {
                                    id: 'errorDetails',
                                    title: 'platform.dialogs.error-details.title',
                                    message: error.data.message
                                }
                                dialogService.showErrorDialog(errorDialog);
                            });
                    } else {
                        measures.updateMeasure(blade.currentEntity,
                            function () {
                                bladeNavigationService.closeChildrenBlades(blade);
                                blade.refresh(true);
                            });
                    }
                };

                blade.onClose = function (closeCallback) {
                    bladeNavigationService.showConfirmationIfNeeded(isDirty(), canSave(), blade, $scope.saveChanges, closeCallback, "catalog.dialogs.measure-save.title", "catalog.dialogs.measure-save.message");
                };

                function canSave() {
                    return isDirty() && $scope.formScope && $scope.formScope.$valid;
                }

                function isDirty() {
                    return !angular.equals(blade.currentEntity, blade.originalEntity) && blade.hasUpdatePermission();
                }

                blade.refresh = function (parentRefresh) {
                    if (blade.isNew) {
                        blade.originalEntity = {};
                        blade.isLoading = false;
                    } else {
                        blade.isLoading = true;

                        measures.getMeasure({ id: blade.currentEntityId }, initializeBlade);

                        if (parentRefresh) {
                            blade.parentBlade.refresh(true);
                        }
                    }
                };

                function initializeBlade(data) {
                    blade.currentEntity = angular.copy(data);
                    blade.originalEntity = data;

                    if (!blade.isNew) {
                        blade.title = blade.currentEntity.name;
                    }

                    blade.isLoading = false;
                }

                blade.refresh(false);
            }
        ]
    );
