angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.catalogsListController', ['$scope', 'virtoCommerce.catalogModule.catalogs', 'platformWebApp.bladeNavigationService', 'platformWebApp.dialogService', 'platformWebApp.authService', 'platformWebApp.uiGridHelper', 'platformWebApp.bladeUtils', '$timeout',
        function ($scope, catalogs, bladeNavigationService, dialogService, authService, uiGridHelper, bladeUtils, $timeout) {
            $scope.uiGridConstants = uiGridHelper.uiGridConstants;
            var blade = $scope.blade;
            $scope.hasMore = true;
            blade.currentEntities = [];
            var selectedNode = null;

            blade.refresh = function () {
                blade.isLoading = true;

                if ($scope.pageSettings.currentPage !== 1)
                    $scope.pageSettings.currentPage = 1;

                catalogs.search(getSearchCriteria(), function (data) {
                    blade.isLoading = false;
                    $scope.pageSettings.totalItems = data.totalCount;
                    blade.currentEntities = data.results;
                    $scope.hasMore = data.results.length === $scope.pageSettings.itemsPerPageCount;

                    if (selectedNode) {
                        //select the node in the new list
                        selectedNode = _.findWhere(data.results, { id: selectedNode.id });
                    }
                });
                if ($scope.gridApi) {
                    blade.currentEntities = [];
                    $scope.gridApi.infiniteScroll.resetScroll(true, true);
                    $scope.gridApi.infiniteScroll.dataLoaded();
                }
            };

            function showMore() {
                if ($scope.hasMore) {
                    ++$scope.pageSettings.currentPage;
                    $scope.gridApi.infiniteScroll.saveScrollPercentage();
                    blade.isLoading = true;

                    catalogs.search(getSearchCriteria(), function (data) {
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
                    sort: uiGridHelper.getSortExpression($scope),
                    skip: ($scope.pageSettings.currentPage - 1) * $scope.pageSettings.itemsPerPageCount,
                    take: $scope.pageSettings.itemsPerPageCount
                };
            }

            $scope.selectNode = function (node) {
                selectedNode = node;
                $scope.selectedNodeId = selectedNode.id;

                openItemsBlade(node);

                // setting current catalog to be globally available 
                bladeNavigationService.catalogsSelectedCatalog = selectedNode;
                bladeNavigationService.catalogsSelectedCategoryId = undefined;
            };

            function openItemsBlade(node) {
                var newBlade = {
                    id: 'itemsList1',
                    level: 1,
                    breadcrumbs: blade.breadcrumbs,
                    title: 'catalog.blades.categories-items-list.title',
                    controller: 'virtoCommerce.catalogModule.categoriesItemsListController',
                    template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/categories-items-list.tpl.html'
                };

                if (node.id)
                    angular.extend(newBlade,
                        {
                            subtitle: 'catalog.blades.categories-items-list.subtitle',
                            subtitleValues: { name: node.name },
                            catalogId: node.id,
                            catalog: node,
                            securityScopes: node.securityScopes
                        });
                else
                    angular.extend(newBlade,
                        {
                            filterKeyword: filter.keyword,
                            subtitle: 'catalog.blades.categories-items-list.subtitle-search',
                            subtitleValues: { keyword: filter.keyword }
                        });

                bladeNavigationService.showBlade(newBlade, blade);
            }

            $scope.editCatalog = function (catalog) {
                if (catalog.isVirtual) {
                    showVirtualCatalogBlade(catalog.id, null, catalog.name);
                } else {
                    showCatalogBlade(catalog.id, null, catalog.name);
                }
            };

            $scope.deleteCatalog = function (node) {
                var dialog = {
                    id: "confirmDelete",
                    name: node.name,
                    callback: function (remove) {
                        if (remove) {
                            bladeNavigationService.closeChildrenBlades(blade,
                                function () {
                                    selectedNode = undefined;
                                    $scope.selectedNodeId = undefined;
                                    blade.isLoading = true;
                                    catalogs.delete({ id: node.id },
                                        blade.refresh,
                                        function (error) {
                                            bladeNavigationService.setError('Error ' + error.status, blade);
                                        }
                                    );
                                });
                        }
                    }
                };
                dialogService.showDialog(dialog,
                    'Modules/$(VirtoCommerce.Catalog)/Scripts/dialogs/deleteCatalog-dialog.tpl.html',
                    'platformWebApp.confirmDialogController');
            };

            function showCatalogBlade(id, data, title) {
                var newBlade = {
                    currentEntityId: id,
                    currentEntity: data,
                    title: title,
                    id: 'catalogEdit',
                    subtitle: 'catalog.blades.catalog-detail.subtitle',
                    controller: 'virtoCommerce.catalogModule.catalogDetailController',
                    template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/catalog-detail.tpl.html',
                };

                bladeNavigationService.showBlade(newBlade, blade);
            }

            function showVirtualCatalogBlade(id, data, title) {
                var newBlade = {
                    currentEntityId: id,
                    currentEntity: data,
                    title: title,
                    subtitle: 'catalog.blades.catalog-detail.subtitle-virtual',
                    id: 'catalogEdit',
                    controller: 'virtoCommerce.catalogModule.virtualCatalogDetailController',
                    template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/catalog-detail.tpl.html',
                };

                bladeNavigationService.showBlade(newBlade, blade);
            }

            blade.toolbarCommands = [
                {
                    name: "platform.commands.refresh",
                    icon: 'fa fa-refresh',
                    executeMethod: blade.refresh,
                    canExecuteMethod: function () {
                        return true;
                    }
                }
            ];

            if (authService.checkPermission('catalog:create')) {
                blade.toolbarCommands.splice(1,
                    0,
                    {
                        name: "platform.commands.add",
                        icon: 'fas fa-plus',
                        executeMethod: function () {
                            selectedNode = undefined;
                            $scope.selectedNodeId = undefined;

                            var newBlade = {
                                id: 'listItemChild',
                                title: 'catalog.blades.catalog-add.title',
                                subtitle: 'catalog.blades.catalog-add.subtitle',
                                controller: 'virtoCommerce.catalogModule.catalogAddController',
                                template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/catalog-add.tpl.html'
                            };

                            bladeNavigationService.showBlade(newBlade, blade);
                        },
                        canExecuteMethod: function () {
                            return true;
                        }
                    });
            }

            // simple and advanced filtering
            var filter = blade.filter = { keyword: null };

            filter.criteriaChanged = function () {
                if (filter.keyword) {
                    openItemsBlade({});
                } else
                    bladeNavigationService.closeChildrenBlades(blade);

                selectedNode = null;
                $scope.selectedNodeId = null;
                bladeNavigationService.catalogsSelectedCatalog = undefined;
                bladeNavigationService.catalogsSelectedCategoryId = undefined;
            };

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

            // actions on load
            // blade.refresh();
        }
    ]);
