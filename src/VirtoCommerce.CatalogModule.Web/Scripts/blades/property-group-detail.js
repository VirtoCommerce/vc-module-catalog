angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.propertyGroupDetailController', ['$scope', 'platformWebApp.metaFormsService', 'platformWebApp.bladeNavigationService', 'platformWebApp.dialogService', function ($scope, metaFormsService, bladeNavigationService, dialogService) {
        var blade = $scope.blade;
        blade.updatePermission = 'catalog:metadata-property:edit';
        blade.title = "catalog.blades.property-group-details.title";
        blade.headIcon = 'fa fa-gear';

        blade.origEntity = {};

        blade.metaFields = metaFormsService.getMetaFields("propertyGroupDetail");

        blade.refresh = function () {
            blade.origEntity = blade.propertyGroup;
            blade.currentEntity = angular.copy(blade.origEntity);
            blade.isLoading = false;
        };

        function saveChanges() {
            if (blade.isNew) {
                if (!blade.catalog.propertyGroups) {
                    blade.catalog.propertyGroups = [];
                }
                blade.catalog.propertyGroups.push(blade.currentEntity);
            }

            angular.copy(blade.currentEntity, blade.origEntity);
        }

        function isDirty() {
            return !angular.equals(blade.currentEntity, blade.origEntity) && blade.hasUpdatePermission();
        }

        function canSave() {
            return (blade.origEntity.isNew || isDirty()) && formScope && formScope.$valid;
        }
      
        function remove(propertyGroup) {
            var dialog = {
                id: "confirmDelete",
                messageValues: { name: prop.name },
                callback: function (doDeleteValues) {
                    blade.isLoading = true;

                    properties.remove({ id: prop.id, doDeleteValues: doDeleteValues }, function () {
                        $scope.bladeClose();
                        blade.parentBlade.refresh();
                    });
                }
            };
            dialogService.showDialog(dialog, 'Modules/$(VirtoCommerce.Catalog)/Scripts/dialogs/deleteProperty-dialog.tpl.html', 'platformWebApp.confirmDialogController');
        }

        //blade.onClose = function (closeCallback) {
        //    bladeNavigationService.showConfirmationIfNeeded(isDirty(), canSave(), blade, saveChanges, closeCallback,
        //        "catalog.dialogs.property-save.title", "catalog.dialogs.property-save.message");
        //};

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
                    remove(blade.origEntity);
                },
                canExecuteMethod: function () {
                    return !blade.isNew;
                }
            }
        ];

        blade.refresh();
    }]);
