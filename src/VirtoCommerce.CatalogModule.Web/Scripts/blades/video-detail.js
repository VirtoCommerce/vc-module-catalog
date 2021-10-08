angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.videoDetailController',
        ['$scope', 'platformWebApp.bladeNavigationService', 'platformWebApp.dialogService', 'platformWebApp.settings', 'virtoCommerce.catalogModule.videos',
            function ($scope, bladeNavigationService, dialogService, settings, videos) {
                var blade = $scope.blade;
                var parentBlade = blade.parentBlade;
                blade.updatePermission = 'catalog:update';
                blade.headIcon = 'fab fa-youtube';
                blade.title = 'catalog.blades.video-detail.title';
                blade.subtitle = 'catalog.blades.video-detail.subtitle';

                $scope.languages = [];
                var languagesPromise = settings.getValues({ id: 'VirtoCommerce.Core.General.Languages' }).$promise;

                blade.refresh = function (parentRefresh) {
                    blade.isLoading = true;

                    languagesPromise.then(function (data) {
                        $scope.languages = data;
                    });

                    initialize(blade.currentEntity);
                    if (parentRefresh && blade.parentBlade) {
                        blade.parentBlade.refresh(parentRefresh);
                    }
                };

                function initialize(item) {
                    blade.currentEntity = angular.copy(item);
                    blade.origEntity = item;
                    blade.isLoading = false;
                }

                // datepicker
                blade.datepickers = {};
                blade.open = function ($event, which) {
                    $event.preventDefault();
                    $event.stopPropagation();
                    blade.datepickers[which] = true;
                };

                function saveChanges() {
                    blade.isLoading = true;
                    var promise = saveOrUpdate();
                    promise.catch(function (error) {
                        bladeNavigationService.setError(error, blade);
                    }).finally(function () {
                        blade.isLoading = false;
                    });
                }

                function saveOrUpdate() {
                    return videos.save([blade.currentEntity], function (data) {
                        blade.isNew = false;
                        blade.currentEntityId = data[0].id;
                        blade.currentEntity.Id = blade.currentEntityId;
                        blade.refresh(true);
                    }).$promise;
                }

                function deleteEntry() {
                    var dialog = {
                        id: "confirmDelete",
                        title: "catalog.dialogs.video-delete.title",
                        message: "catalog.dialogs.video-delete.message",
                        callback: function (remove) {
                            if (remove) {
                                blade.isLoading = true;
                                videos.remove({ ids: [blade.currentEntityId] }, function () {
                                    bladeNavigationService.closeBlade(blade, function () {
                                        if (parentBlade)
                                            parentBlade.refresh(true);
                                    });
                                });
                            }
                        }
                    };
                    dialogService.showConfirmationDialog(dialog);
                }

                var currentForm;
                $scope.setForm = function (form) {
                    currentForm = form;
                };

                $scope.saveChanges = function () {
                    bladeNavigationService.setError(null, blade);
                    saveChanges();
                };

                function isDirty() {
                    return (blade.isNew || !angular.equals(blade.currentEntity, blade.origEntity)) && blade.hasUpdatePermission();
                }

                function canSave() {
                    return isDirty() && currentForm && currentForm.$valid;
                }

                blade.onClose = function (closeCallback) {
                    bladeNavigationService.showConfirmationIfNeeded(isDirty(), canSave(), blade, $scope.saveChanges, closeCallback, "catalog.dialogs.video-save.title", "catalog.dialogs.video-save.message");
                };

                blade.toolbarCommands = [
                    {
                        name: "platform.commands.save",
                        icon: 'fa fa-save',
                        permission: blade.updatePermission,
                        executeMethod: function () {
                            saveChanges();
                        },
                        canExecuteMethod: function () {
                            return canSave();
                        }
                    },
                    {
                        name: "platform.commands.reset",
                        icon: 'fa fa-undo',
                        executeMethod: function () {
                            angular.copy(blade.origEntity, blade.currentEntity);
                        },
                        canExecuteMethod: isDirty
                    },
                    {
                        name: "platform.commands.delete",
                        icon: 'fa fa-trash-o',
                        permission: blade.updatePermission,
                        executeMethod: deleteEntry,
                        canExecuteMethod: function () {
                            return !blade.isNew;
                        }
                    }
                ];

                blade.refresh();
        }]);
