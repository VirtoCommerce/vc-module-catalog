angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.propertyGroupDetailController', ['$scope', 'platformWebApp.metaFormsService', 'platformWebApp.bladeNavigationService', 'platformWebApp.dialogService', 'virtoCommerce.catalogModule.propertyGroups', function ($scope, metaFormsService, bladeNavigationService, dialogService, propertyGroups) {
        var blade = $scope.blade;
        blade.updatePermission = 'catalog:metadata-property:edit';
        blade.title = "catalog.blades.property-group-details.title";
        blade.headIcon = 'fa fa-gear';

        blade.origEntity = {};

        blade.metaFields = metaFormsService.getMetaFields("propertyGroupDetail");

        blade.refresh = function (parentRefresh) {
            if (blade.isNew) {
                initialize(blade.propertyGroup);
            }
            else {
                propertyGroups.get({ id: blade.currentEntityId }, function (data) {
                    initialize(data);
                    if (parentRefresh) {
                        blade.parentBlade.refresh();
                    }
                }, function (error) { bladeNavigationService.setError('Error ' + error.status, blade); });
            }
        };

        function initialize(propertyGroup) {
            blade.origEntity = propertyGroup;
            blade.currentEntity = angular.copy(blade.origEntity);
            blade.isLoading = false;
        }
    
        function saveChanges() {
            blade.isLoading = true;
            propertyGroups.save({}, blade.currentEntity, function (data) {
                blade.isNew = false;
                blade.currentEntityId = data.id;
                blade.refresh(true);
            }, function (error) { bladeNavigationService.setError('Error ' + error.status, blade); });
        };

        function isDirty() {
            return !angular.equals(blade.currentEntity, blade.origEntity) && blade.hasUpdatePermission();
        }

        function canSave() {
            return (blade.origEntity.isNew || isDirty()) && formScope && formScope.$valid;
        }
      
        $scope.deletePropertyGroups = function (propertyGroup) {
            var dialog = {
                id: "confirmDeletePropertyValue",
                title: "catalog.dialogs.property-group-delete.title",
                message: "catalog.dialogs.property-group-delete.message",
                callback: function (remove) {
                    if (remove) {
                        bladeNavigationService.closeChildrenBlades(blade, function () {
                            propertyGroups.remove({ ids: [propertyGroup.id] }, function (data) {
                                $scope.bladeClose();
                                blade.parentBlade.refresh();
                            });
                        });
                    }
                }
            };
            dialogService.showWarningDialog(dialog);
        }

        blade.onClose = function (closeCallback) {
            bladeNavigationService.showConfirmationIfNeeded(isDirty(), canSave(), blade, saveChanges, closeCallback,
                "catalog.dialogs.property-group-save.title", "catalog.dialogs.property-group-save.message");
        };

        var formScope;
        $scope.setForm = function (form) {
            formScope = form;
        }

        blade.toolbarCommands = [
            {
                name: "platform.commands.save",
                icon: 'fas fa-save',
                executeMethod: saveChanges,
                canExecuteMethod: canSave
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
                icon: 'fas fa-trash-alt',
                executeMethod: function () {
                    $scope.deletePropertyGroups(blade.origEntity);
                },
                canExecuteMethod: function () {
                    return !blade.isNew;
                }
            }
        ];

        blade.refresh();
    }]);
