angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.propertyGroupListController', ['$scope', 'platformWebApp.uiGridHelper', 'platformWebApp.bladeNavigationService', 'virtoCommerce.catalogModule.propertyGroups', 'platformWebApp.dialogService', function ($scope, uiGridHelper, bladeNavigationService, propertyGroups, dialogService) {
        var blade = $scope.blade;
        blade.updatePermission = 'catalog:metadata-property:edit';
        blade.title = "catalog.blades.property-group-list.title";
        blade.isLoading = true;

        blade.refresh = function (catalog) {
            if (catalog) {
                initialize(catalog);
            }
            else {
                blade.parentBlade.refresh();
            }
        };

        function initialize(catalog) {
            blade.currentEntities = catalog.propertyGroups;
            blade.isLoading = false;
        }

        blade.toolbarCommands = [
            {
                name: "platform.commands.add",
                icon: 'fas fa-plus',
                executeMethod: function () {
                    openDetailsBlade({
                        catalogId: blade.catalog.id,
                        priority: 1,
                    }, true);
                },
                canExecuteMethod: function () {
                    return true;
                },
                permission: blade.updatePermission
            },
            {
                name: "platform.commands.delete",
                icon: 'fas fa-trash-alt',
                executeMethod: function () {
                    $scope.deletePropertyGroups($scope.gridApi.selection.getSelectedRows());
                },
                canExecuteMethod: function () {
                    return $scope.gridApi && _.any($scope.gridApi.selection.getSelectedRows()) && _.any(blade.catalog.propertyGroups);
                },
                permission: blade.updatePermission
            }
        ];

        blade.selectNode = openDetailsBlade;

        function openDetailsBlade(propertyGroup, isNew) {
            $scope.selectedNodeId = propertyGroup.id;

            var newBlade = {
                id: 'propertyGroupDetails',
                isNew: isNew,
                propertyGroup: propertyGroup,
                currentEntityId: propertyGroup.id,
                catalog: blade.catalog,
                languages: blade.languages,
                controller: 'virtoCommerce.catalogModule.propertyGroupDetailController',
                template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/property-group-detail.tpl.html'
            };

            bladeNavigationService.showBlade(newBlade, blade);
        }

        $scope.deletePropertyGroups = function (selection) {
            var dialog = {
                id: "confirmDeletePropertyValue",
                title: "catalog.dialogs.property-group-delete.title",
                message: "catalog.dialogs.property-group-delete.message",
                callback: function (remove) {
                    if (remove) {
                        bladeNavigationService.closeChildrenBlades(blade, function () {
                            var propertyGroupIds = _.pluck(selection, 'id');
                            propertyGroups.remove({ ids: propertyGroupIds }, function (data) {
                                blade.refresh();
                            });
                        });
                    }
                }
            };
            dialogService.showWarningDialog(dialog);
        }

        // ui-grid
        $scope.setGridOptions = function (gridOptions) {
            uiGridHelper.initialize($scope, gridOptions);
        };

        initialize(blade.catalog);
    }]);
