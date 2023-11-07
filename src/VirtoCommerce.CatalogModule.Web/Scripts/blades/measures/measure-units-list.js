angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.measureUnitsListController',
        ['$scope', 'platformWebApp.bladeNavigationService', 'platformWebApp.uiGridHelper', 'platformWebApp.bladeUtils', 'platformWebApp.dialogService',
            function ($scope, bladeNavigationService, uiGridHelper, bladeUtils, dialogService) {
                $scope.uiGridConstants = uiGridHelper.uiGridConstants;
                var blade = $scope.blade;
                $scope.selectedItem = null;
                $scope.items = blade.currentEntity.units;

                $scope.selectItem = function (listItem) {
                    $scope.selectedItem = listItem;
                };

                // ui-grid
                $scope.setGridOptions = function (gridOptions) {

                    //disable watched
                    bladeUtils.initializePagination($scope, true);
                    //choose the optimal amount that ensures the appearance of the scroll
                    $scope.pageSettings.itemsPerPageCount = 50;

                    uiGridHelper.initialize($scope, gridOptions, function (gridApi) {
                        //update gridApi for current grid
                        $scope.gridApi = gridApi;

                        uiGridHelper.bindRefreshOnSortChanged($scope);
                    });
                };

                function isItemsChecked() {
                    return $scope.gridApi && _.any($scope.gridApi.selection.getSelectedRows());
                }
    
                function deleteList(selection) {
                    var itemIds = [];
                    angular.forEach(selection, function (item) {
                        itemIds.push(item.id);
                    });

                    var dialog = {
                        id: "confirmDelete",
                        title: "catalog.dialogs.measure-units-delete.title",
                        message: "catalog.dialogs.measure-units-delete.message",
                        callback: function (remove) {
                            if (remove) {
                                bladeNavigationService.closeChildrenBlades(blade);
                                blade.isLoading = true;

                                angular.forEach(itemIds, function (id) {
                                    var toRemove = _.find(blade.currentEntity.units, function (x) { return angular.equals(x.id, id) });
                                    if (toRemove) {
                                        var idx = blade.currentEntity.units.indexOf(toRemove);
                                        blade.currentEntity.units.splice(idx, 1);
                                    }
                                });

                                blade.isLoading = false;
                            }
                        }
                    }
                    dialogService.showConfirmationDialog(dialog);
                }

                $scope.openDetailBlade = function (unit) {
                    if (!unit) {
                        unit = { isNew: true };
                    }
                    $scope.selectedItem = unit;

                    var newBlade = {
                        id: 'unitDetail',
                        currentEntity: unit,
                        controller: 'virtoCommerce.catalogModule.measureUnitDetailsController',
                        confirmChangesFn: function (unit) {
                            if (unit.isNew) {
                                unit.isNew = undefined;
                                blade.currentEntity.units.push(unit);
                                if (blade.confirmChangesFn) {
                                    blade.confirmChangesFn(unit);
                                }
                            }
                        },
                        deleteFn: function (unit) {
                            var toRemove = _.find(blade.currentEntity.units, function (x) { return angular.equals(x, unit) });
                            if (toRemove) {
                                var idx = blade.currentEntity.units.indexOf(toRemove);
                                blade.currentEntity.units.splice(idx, 1);
                                if (blade.deleteFn) {
                                    blade.deleteFn(unit);
                                }
                            }
                        },

                        template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/measures/measure-unit-details.tpl.html'
                    };
                    bladeNavigationService.showBlade(newBlade, $scope.blade);
                }

                blade.headIcon = blade.parentBlade.headIcon;

                blade.toolbarCommands = [
                    {
                        name: "platform.commands.add", icon: 'fas fa-plus',
                        executeMethod: function () {
                            $scope.openDetailBlade();
                        },
                        canExecuteMethod: function () {
                            return true;
                        }
                    },
                    {
                        name: "platform.commands.delete", icon: 'fas fa-trash-alt',
                        executeMethod: function () { deleteList($scope.gridApi.selection.getSelectedRows()); },
                        canExecuteMethod: isItemsChecked,
                        permission: blade.updatePermission
                    }
                ];

                blade.isLoading = false;
            }
        ]
    );
