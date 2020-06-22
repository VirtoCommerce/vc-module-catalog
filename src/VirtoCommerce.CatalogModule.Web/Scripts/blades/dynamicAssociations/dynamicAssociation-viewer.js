angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.dynamicAssociationViewerController',
        ['$localStorage', '$timeout', '$scope', 'platformWebApp.bladeUtils', 'platformWebApp.uiGridHelper', 'virtoCommerce.catalogModule.dynamicAssociations', 'virtoCommerce.catalogModule.items',
            function ($localStorage, $timeout, $scope, bladeUtils, uiGridHelper, associations, items) {
                $scope.uiGridConstants = uiGridHelper.uiGridConstants;
                $scope.hasMore = true;
                $scope.items = [];
                $scope.blade.headIcon = 'fa-upload';

                var blade = $scope.blade;
                var bladeNavigationService = bladeUtils.bladeNavigationService;
                blade.isLoading = true;
                blade.isExpanded = true;

                var filter = blade.filter = $scope.filter = {};
                blade.exportDataRequest = blade.exportDataRequest ? angular.copy(blade.exportDataRequest) : { exportTypeName: "NotSpecified" };

                if (blade.exportDataRequest.dataQuery && blade.exportDataRequest.dataQuery.keyword) {
                    filter.keyword = blade.exportDataRequest.dataQuery.keyword;
                }

                $scope.$localStorage = $localStorage;

                blade.refresh = function () {
                    $scope.items = [];

                    if ($scope.pageSettings.currentPage !== 1) {
                        $scope.pageSettings.currentPage = 1;
                    }

                    loadData();
                    resetStateGrid();
                };

                function loadData(callback) {
                    blade.isLoading = true;

                    var dataRequest = buildDataQuery();

                    associations.preview(dataRequest, (data) => {
                        let productIds = data;
                        blade.isLoading = false;
                        $scope.pageSettings.totalItems = data.length;
                            //$scope.items = $scope.items.concat(data);
                        $scope.hasMore = data.length === $scope.pageSettings.itemsPerPageCount;


                        items.getByIds({ ids: productIds},
                            response => {
                                $scope.items = $scope.items.concat(response);
                            });
                            if (callback) {
                                callback();
                            }
                        });
                }

                function showMore() {
                    if ($scope.hasMore) {
                        ++$scope.pageSettings.currentPage;
                        $scope.gridApi.infiniteScroll.saveScrollPercentage();
                        loadData(function () {
                            $scope.gridApi.infiniteScroll.dataLoaded();

                            $timeout(function () {
                                // wait for grid to ingest data changes
                                if ($scope.gridApi.selection.getSelectAllState()) {
                                    $scope.gridApi.selection.selectAllRows();
                                }
                            });
                        });
                    }
                }

                function buildDataQuery() {
                    let propertyValues = {};
                    _.each(blade.properties,
                        property => {
                            propertyValues[property.name] = property.values.map(x => x.value);
                        });

                    var dataQuery = {
                        categoryIds: blade.categoryIds,
                        propertyValues: propertyValues,
                        keyword: filter.keyword,
                        skip: ($scope.pageSettings.currentPage - 1) * $scope.pageSettings.itemsPerPageCount,
                        take: $scope.pageSettings.itemsPerPageCount

                    };
                    return dataQuery;
                }

                function resetStateGrid() {
                    if ($scope.gridApi) {
                        $scope.items = [];
                        $scope.gridApi.selection.clearSelectedRows();
                        $scope.gridApi.infiniteScroll.resetScroll(true, true);
                        $scope.gridApi.infiniteScroll.dataLoaded();
                    }
                }

                blade.setSelectedItem = function (listItem) {
                    $scope.selectedNodeId = listItem.id;
                };

                $scope.selectItem = function (e, listItem) {
                    blade.setSelectedItem(listItem);

                    var viewerFunc = genericViewerItemService.getViewer(listItem.type);

                    if (viewerFunc) {
                        bladeNavigationService.showBlade(viewerFunc(listItem), blade);
                    }
                };

                filter.criteriaChanged = function () {
                    blade.refresh();
                };

                filter.resetKeyword = function() {
                    filter.keyword = undefined;
                };

                blade.toolbarCommands = [{
                    name: "platform.commands.refresh",
                    icon: 'fa fa-refresh',
                    executeMethod: function () {
                        //blade.resetFiltering();
                        blade.refresh();
                    },
                    canExecuteMethod: function () {
                        return true;
                    }
                }];

                $scope.setGridOptions = function (gridOptions) {
                    bladeUtils.initializePagination($scope, true);
                    $scope.pageSettings.itemsPerPageCount = 20;

                    uiGridHelper.initialize($scope, gridOptions, function (gridApi) {
                        //update gridApi for current grid
                        $scope.gridApi = gridApi;

                        uiGridHelper.bindRefreshOnSortChanged($scope);
                        $scope.gridApi.infiniteScroll.on.needLoadMoreData($scope, showMore);

                    });

                    // need to call refresh after digest cycle as we do not "$watch" for $scope.pageSettings.currentPage
                    $timeout(function () {
                        blade.refresh();
                    });
                };

                $scope.cancelChanges = function () {
                    bladeNavigationService.closeBlade(blade);
                };

                $scope.isValid = function () {
                    return ($scope.items && $scope.items.length);
                };

                $scope.saveChanges = function () {
                    bladeNavigationService.closeBlade(blade);
                };
            }]);
