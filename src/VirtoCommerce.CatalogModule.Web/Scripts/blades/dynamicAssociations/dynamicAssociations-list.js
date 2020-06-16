angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.dynamicAssociationsListController', ['$scope', 'virtoCommerce.catalogModule.dynamicAssociations', 'platformWebApp.dialogService', 'platformWebApp.bladeUtils', 'platformWebApp.uiGridHelper', '$timeout', 
        function ($scope, associations, dialogService, bladeUtils, uiGridHelper, $timeout) {
            var blade = $scope.blade;
            var bladeNavigationService = bladeUtils.bladeNavigationService;
            var selectedNode = null;

            blade.refresh = function () {
                blade.isLoading = true;

                if ($scope.pageSettings.currentPage !== 1) {
                    $scope.pageSettings.currentPage = 1;
                }

                associations.search(getSearchCriteria(), function (data) {
                    blade.isLoading = false;
                    $scope.pageSettings.totalItems = data.totalCount;
                    blade.currentEntities = data.results;
                    $scope.hasMore = data.results.length === $scope.pageSettings.itemsPerPageCount;

                    if (selectedNode) {
                        //select the node in the new list
                        selectedNode = _.findWhere(data.results, { id: selectedNode.id });
                    }
                    if ($scope.gridApi) {
                        $scope.gridApi.infiniteScroll.resetScroll(true, true);
                        $scope.gridApi.infiniteScroll.dataLoaded();
                    }
                });
            };

            function showMore() {
                if ($scope.hasMore) {
                    ++$scope.pageSettings.currentPage;
                    $scope.gridApi.infiniteScroll.saveScrollPercentage();
                    blade.isLoading = true;

                    associations.search(getSearchCriteria(), function (data) {
                        blade.isLoading = false;
                        $scope.pageSettings.totalItems = data.totalCount;
                        blade.currentEntities = blade.currentEntities.concat(data.results);
                        $scope.hasMore = data.results.length === $scope.pageSettings.itemsPerPageCount;
                        $scope.gridApi.infiniteScroll.dataLoaded();
                    });
                }
            }

            function getSearchCriteria() {
                return {
                    keyword: filter.keyword,
                    sort: uiGridHelper.getSortExpression($scope),
                    skip: ($scope.pageSettings.currentPage - 1) * $scope.pageSettings.itemsPerPageCount,
                    take: $scope.pageSettings.itemsPerPageCount
                };
            }


            $scope.selectNode = function (node) {
                $scope.selectedNodeId = node.id;

                var newBlade = {
                    id: 'listItemChild',
                    currentEntityId: node.id,
                    title: node.name,
                    subtitle: blade.subtitle,
                    controller: 'virtoCommerce.catalogModule.dynamicAssociationDetailController',
                    template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/dynamicAssociations/dynamicAssociations-detail.tpl.html'
                };

                bladeNavigationService.showBlade(newBlade, blade);
            };

            $scope.deleteList = function (list) {
                var dialog = {
                    id: "confirmDeleteItem",
                    title: "catalog.dialogs.dynamic-association-delete.title",
                    message: "catalog.dialogs.dynamic-association-delete.message",
                    callback: function (remove) {
                        if (remove) {
                            bladeNavigationService.closeChildrenBlades(blade, function () {
                                blade.isLoading = true;
                                var itemIds = _.pluck(list, 'id');
                                associations.remove({ ids: itemIds }, function () {
                                    blade.refresh();
                                });
                            });
                        }
                    }
                };
                dialogService.showConfirmationDialog(dialog);
            };

            blade.headIcon = 'fa-area-chart';

            blade.toolbarCommands = [
                {
                    name: "platform.commands.refresh", icon: 'fa fa-refresh',
                    executeMethod: blade.refresh,
                    canExecuteMethod: function () { return true; }
                },
                {
                    name: "platform.commands.add", icon: 'fa fa-plus',
                    executeMethod: function () {
                        bladeNavigationService.closeChildrenBlades(blade, function () {
                            var newBlade = {
                                id: 'listItemChild',
                                title: 'catalog.blades.dynamicAssociation-detail.title-new',
                                subtitle: blade.subtitle,
                                isNew: true,
                                controller: 'virtoCommerce.catalogModule.dynamicAssociationDetailController',
                                template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/dynamicAssociations/dynamicAssociations-detail.tpl.html'
                            };
                            bladeNavigationService.showBlade(newBlade, blade);
                        });
                    },
                    canExecuteMethod: function () { return true; },
                    permission: 'catalog:create'
                },
                {
                    name: "platform.commands.delete", icon: 'fa fa-trash-o',
                    executeMethod: function () {
                        $scope.deleteList($scope.gridApi.selection.getSelectedRows());
                    },
                    canExecuteMethod: function () {
                        return $scope.gridApi && _.any($scope.gridApi.selection.getSelectedRows());
                    },
                    permission: 'marketing:delete'
                }
            ];

            var filter = blade.filter = $scope.filter = {};
            filter.criteriaChanged = function () {
                if ($scope.pageSettings.currentPage > 1) {
                    $scope.pageSettings.currentPage = 1;
                } else {
                    blade.refresh();
                }
            };

            // ui-grid
            $scope.setGridOptions = function (gridOptions) {
                bladeUtils.initializePagination($scope, true);
                $scope.pageSettings.itemsPerPageCount = 20;

                uiGridHelper.initialize($scope, gridOptions, function (gridApi) {
                    //update gridApi for current grid
                    $scope.gridApi = gridApi;
                    uiGridHelper.bindRefreshOnSortChanged($scope);
                    $scope.gridApi.infiniteScroll.on.needLoadMoreData($scope, showMore);

                });

                $timeout(function () {  blade.refresh(); });
            };
        }]);
