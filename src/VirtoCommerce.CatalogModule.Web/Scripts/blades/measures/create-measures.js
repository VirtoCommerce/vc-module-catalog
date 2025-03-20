angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.createMeasuresController',
        ['$scope', 'virtoCommerce.catalogModule.measures', 'platformWebApp.bladeUtils', 'platformWebApp.ui-grid.extension', 'platformWebApp.uiGridHelper',
            function ($scope, measures, bladeUtils, gridOptionExtension, uiGridHelper) {
                var blade = $scope.blade;
                $scope.uiGridConstants = uiGridHelper.uiGridConstants;
                blade.title = 'catalog.blades.create-measures.title';

                blade.refresh = function () {
                    measures.getDefaultMeasures({}, function (data) {
                        blade.isLoading = false;
                        $scope.listEntries = data ? data : [];
                    });
                };

                blade.toolbarCommands = [
                    {
                        name: "platform.commands.create",
                        icon: 'fas fa-plus',
                        executeMethod: function () {
                            $scope.createMeasures($scope.gridApi.selection.getSelectedRows());
                        },
                        canExecuteMethod: function () {
                            return $scope.gridApi && _.any($scope.gridApi.selection.getSelectedRows());
                        },
                        permission: 'measure:create'
                    }
                ];

                $scope.createMeasures = function (selection) {
                    measures.createMeasures(selection, function () {
                        blade.parentBlade.refresh();
                        $scope.bladeClose();
                    });
                }

                $scope.setGridOptions = function (gridId, gridOptions) {
                    $scope.gridOptions = gridOptions;
                    gridOptionExtension.tryExtendGridOptions(gridId, gridOptions);

                    uiGridHelper.initialize($scope, gridOptions, function (gridApi) {
                        uiGridHelper.bindRefreshOnSortChanged($scope);
                    });

                    gridOptions.onRegisterApi = function (gridApi) {
                        $scope.gridApi = gridApi;
                    };

                    bladeUtils.initializePagination($scope);

                    return gridOptions;
                };
            }
        ]
    );
