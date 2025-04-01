angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.propertyGroupListController', ['$scope', 'platformWebApp.uiGridHelper', 'platformWebApp.bladeNavigationService', 'platformWebApp.authService', 'platformWebApp.dialogService', function ($scope, uiGridHelper, bladeNavigationService, authService, dialogService) {
        var blade = $scope.blade;
        blade.updatePermission = 'catalog:metadata-property:edit';
        blade.title = "catalog.blades.property-group-list.title";

        blade.refresh = function (catalog) {
            if (catalog) {
                blade.currentEntities = catalog.propertyGroups;
            }
            else {
                blade.parentBlade.refresh();
            }
        };

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
                    deleteList($scope.gridApi.selection.getSelectedRows());
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
                catalog: blade.catalog,
                languages: blade.languages,
                controller: 'virtoCommerce.catalogModule.propertyGroupDetailController',
                template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/property-group-detail.tpl.html'
            };

            bladeNavigationService.showBlade(newBlade, blade);
        }

        // ui-grid
        $scope.setGridOptions = function (gridOptions) {
            uiGridHelper.initialize($scope, gridOptions);
        };

        blade.refresh(blade.catalog);
        blade.isLoading = false;
    }]);
