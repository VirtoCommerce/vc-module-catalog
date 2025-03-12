angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.measuresListController',
        ['$scope', 'virtoCommerce.catalogModule.measures', 'platformWebApp.bladeUtils', 'platformWebApp.dialogService', 'platformWebApp.ui-grid.extension', 'platformWebApp.uiGridHelper',
            function ($scope, measures, bladeUtils, dialogService, gridOptionExtension, uiGridHelper) {
                var blade = $scope.blade;
                blade.headIcon = 'fas fa-ruler-combined';
                blade.title = 'catalog.blades.measures-list.title';
                blade.subtitle = 'catalog.blades.measures-list.subtitle';

                $scope.uiGridConstants = uiGridHelper.uiGridConstants;
                var bladeNavigationService = bladeUtils.bladeNavigationService;

                blade.toolbarCommands = [
                    {
                        name: "platform.commands.refresh",
                        icon: 'fa fa-refresh',
                        executeMethod: function () {
                            blade.refresh();
                        },
                        canExecuteMethod: function () {
                            return true;
                        }
                    },
                    {
                        name: "platform.commands.add",
                        icon: 'fas fa-plus',
                        executeMethod: createMeasure,
                        canExecuteMethod: function () {
                            return true;
                        },
                        permission: 'measure:create'
                    },
                    {
                        name: "platform.commands.delete",
                        icon: 'fas fa-trash-alt',
                        executeMethod: function () {
                            $scope.deleteList($scope.gridApi.selection.getSelectedRows());
                        },
                        canExecuteMethod: function () {
                            return $scope.gridApi && _.any($scope.gridApi.selection.getSelectedRows());
                        },
                        permission: 'measure:delete'
                    }
                ];

                function getSearchCriteria() {
                    return {
                        sort: "name:desc",
                        skip: ($scope.pageSettings.currentPage - 1) * $scope.pageSettings.itemsPerPageCount,
                        take: $scope.pageSettings.itemsPerPageCount
                    };
                }

                blade.refresh = function () {
                    var searchCriteria = getSearchCriteria();

                    measures.searchMeasures(searchCriteria, function (data) {
                        blade.isLoading = false;

                        $scope.pageSettings.totalItems = data.totalCount;
                        blade.currentEntities = data.results;
                        if (!blade.currentEntities.length && $scope.gridApi) {
                            $scope.gridApi.selection.clearSelectedRows();
                        }
                    });
                };

                $scope.selectNode = function (measure) {
                    $scope.selectedNodeId = measure.id;
                    blade.selectedConctact = measure;
                    blade.editMeasure(measure);
                };

                function createMeasure() {
                    var newBlade = {
                        id: 'createMeasure',                        
                        controller: 'virtoCommerce.catalogModule.measureDetailsController',
                        template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/measures/measure-details.html',
                        currentEntity: {},
                        isNew: true,
                    };

                    bladeNavigationService.showBlade(newBlade, blade);
                }

                blade.editMeasure = function (measure) {
                    var newBlade = {
                        id: 'editMeasure',
                        controller: 'virtoCommerce.catalogModule.measureDetailsController',
                        template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/measures/measure-details.html',
                        currentEntity: measure,
                        currentEntityId: measure.id,
                        isNew: false
                    };

                    bladeNavigationService.showBlade(newBlade, blade);
                };

                blade.openCreateMeasures = function () {
                    var newBlade = {
                        id: 'createMeasures',
                        controller: 'virtoCommerce.catalogModule.createMeasuresController',
                        template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/measures/create-measures.html'
                    };

                    bladeNavigationService.showBlade(newBlade, blade);
                }

                $scope.deleteList = function (selection) {
                    var dialog = {
                        id: "confirmDeleteMeasures",
                        title: "catalog.dialogs.measures-delete.title",
                        message: "catalog.dialogs.measures-delete.message",
                        callback: function (remove) {
                            if (remove) {
                                bladeNavigationService.closeChildrenBlades(blade, function () {
                                    blade.isLoading = true;
                                    var itemIds = _.pluck(selection, 'id');
                                    measures.deleteMeasure({ ids: itemIds }, function () {
                                        blade.refresh();
                                    },
                                        function (error) {
                                            bladeNavigationService.setError('Error ' + error.status, blade);
                                        });
                                });
                            }
                        }
                    };
                    dialogService.showConfirmationDialog(dialog);
                }

                $scope.setGridOptions = function (gridOptions) {
                    uiGridHelper.initialize($scope, gridOptions, function (gridApi) {
                        $scope.gridApi = gridApi;
                        uiGridHelper.bindRefreshOnSortChanged($scope);
                    });

                    bladeUtils.initializePagination($scope);
                };
            }
        ]
    );
