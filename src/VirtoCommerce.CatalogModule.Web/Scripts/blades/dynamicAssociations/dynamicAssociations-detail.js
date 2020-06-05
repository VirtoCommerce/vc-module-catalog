angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.dynamicAssociationDetailController', ['$scope', 'platformWebApp.bladeNavigationService', 'virtoCommerce.catalogModule.dynamicAssociations', function ($scope, bladeNavigationService, associations) {
        var blade = $scope.blade;
        blade.updatePermission = 'catalog:update';

        blade.refresh = function(parentRefresh) {
            if (blade.isNew) {
                initializeBlade(blade.currentEntity);
            } else {
                associations.get({ id: blade.currentEntityId },
                    function(data) {
                        initializeBlade(data);

                        if (blade.childrenBlades) {
                            _.each(blade.childrenBlades, x => { x.refresh && x.refresh(blade.currentEntity);});
                        }

                        if (parentRefresh) {
                            blade.parentBlade.refresh();
                        }
                    });
            }
        };

        function initializeBlade(data) {
            if (!blade.isNew) {
                blade.title = data.name;
            }

            blade.currentEntity = angular.copy(data);
            blade.origEntity = data;
            blade.isLoading = false;
            blade.securityScopes = data.securityScopes;
        }

        function isDirty() {
            return !angular.equals(blade.currentEntity, blade.origEntity) && blade.hasUpdatePermission();
        }

        function canSave() {
            return isDirty() && formScope && formScope.$valid;
        }

        var formScope;
        $scope.setForm = function (form) { formScope = form; };

        $scope.mainParameters = function() {
            var newBlade = {
                id: "mainParameters",
                title: "catalog.blades.dynamicAssociation-parameters.title",
                subtitle: 'catalog.blades.dynamicAssociation-parameters.subtitle',
                controller: 'virtoCommerce.catalogModule.dynamicAssociationParametersController',
                template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/dynamicAssociations/mainParameters.tpl.html',
                currentEntity: blade.currentEntity
            };
            bladeNavigationService.showBlade(newBlade, blade);
        };

        $scope.cancelChanges = function () {
            angular.copy(blade.origEntity, blade.currentEntity);
            $scope.bladeClose();
        };
        $scope.saveChanges = function () {
            blade.isLoading = true;

            if (blade.isNew) {
                catalogs.save({}, blade.currentEntity, function (data) {
                    blade.isNew = undefined;
                    blade.currentEntityId = data.id;
                    initializeBlade(data);
                    initializeToolbar();
                    $scope.gridsterOpts.maxRows = 3; // force re-initializing the widgets
                    blade.refresh(true);
                }, function (error) {
                    bladeNavigationService.setError('Error ' + error.status, blade);
                });
            }
            else {
                catalogs.update({}, blade.currentEntity, function () {
                    blade.refresh(true);
                }, function (error) {
                    bladeNavigationService.setError('Error ' + error.status, blade);
                });
            }
        };

        blade.onClose = function (closeCallback) {
            bladeNavigationService.showConfirmationIfNeeded(isDirty(), canSave(), blade, $scope.saveChanges, closeCallback, "catalog.dialogs.catalog-save.title", "catalog.dialogs.catalog-save.message");
        };

        function initializeToolbar() {
            if (!blade.isNew) {
                blade.toolbarCommands = [
                    {
                        name: "platform.commands.save", icon: 'fa fa-save',
                        executeMethod: function () {
                            $scope.saveChanges();
                        },
                        canExecuteMethod: canSave,
                        permission: blade.updatePermission
                    },
                    {
                        name: "platform.commands.reset", icon: 'fa fa-undo',
                        executeMethod: function () {
                            angular.copy(blade.origEntity, blade.currentEntity);
                        },
                        canExecuteMethod: isDirty,
                        permission: blade.updatePermission
                    }
                ];
            }
        }

        $scope.gridsterOpts = { columns: 3 };

        $scope.$watch('blade.currentEntity', (data) => {
            if (data) {
                console.log(data);
                $scope.totalPropertiesCount = 5;
                $scope.filledPropertiesCount = 0;

                $scope.filledPropertiesCount += blade.currentEntity.startDate ? 1 : 0;
                $scope.filledPropertiesCount += blade.currentEntity.endDate ? 1 : 0;
                $scope.filledPropertiesCount += blade.currentEntity.storeId ? 1 : 0;
                $scope.filledPropertiesCount += blade.currentEntity.groupName ? 1 : 0;
                $scope.filledPropertiesCount += blade.currentEntity.priority ? 1 : 0;

            }
        }, true);


        initializeToolbar();
        blade.refresh(false);
    }]);
