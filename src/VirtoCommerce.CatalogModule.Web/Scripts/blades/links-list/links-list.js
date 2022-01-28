angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.linksListController',
        ['$scope', '$timeout', 'platformWebApp.bladeNavigationService', 'platformWebApp.dialogService', 'platformWebApp.bladeUtils', 'platformWebApp.uiGridHelper', 'virtoCommerce.catalogModule.listEntries',
            function ($scope, $timeout, bladeNavigationService, dialogService, bladeUtils, uiGridHelper, listEntries) {
                var blade = $scope.blade;

                blade.updatePermission = 'catalog:update';
                blade.headIcon = 'fas fa-link';
                blade.title = 'catalog.blades.links-list.title';
                blade.linkImage = 'Modules/$(VirtoCommerce.Catalog)/Content/images/link.svg';

                $scope.links = [];
                $scope.hasMore = true;

                blade.refresh = function () {
                    blade.isLoading = true;

                    if ($scope.pageSettings.currentPage !== 1)
                        $scope.pageSettings.currentPage = 1;

                    var searchCriteria = blade.getSearchCriteria();

                    listEntries.searchlinks(searchCriteria, function (data) {
                        setEntryPath(data.results);

                        blade.isLoading = false;

                        $scope.pageSettings.totalItems = data.totalCount;
                        $scope.hasMore = data.results.length === $scope.pageSettings.itemsPerPageCount;
                        $scope.links = data.results;
                    });

                    resetStateGrid();
                };

                function resetStateGrid() {
                    if ($scope.gridApi) {
                        $scope.links = [];
                        $scope.gridApi.selection.clearSelectedRows();
                        $scope.gridApi.infiniteScroll.resetScroll(true, true);
                        $scope.gridApi.infiniteScroll.dataLoaded();
                    }
                }

                function showMore() {
                    if ($scope.hasMore) {
                        ++$scope.pageSettings.currentPage;
                        $scope.gridApi.infiniteScroll.saveScrollPercentage();
                        blade.isLoading = true;

                        var searchCriteria = blade.getSearchCriteria();

                        listEntries.searchlinks(
                            searchCriteria,
                            function (data) {
                                setEntryPath(data.results);

                                blade.isLoading = false;

                                $scope.pageSettings.totalItems = data.totalCount;
                                $scope.hasMore = data.results.length === $scope.pageSettings.itemsPerPageCount;
                                $scope.links = $scope.links.concat(data.results);

                                $scope.gridApi.infiniteScroll.dataLoaded();

                                $timeout(function () {
                                    if ($scope.gridApi.selection.getSelectAllState()) {
                                        $scope.gridApi.selection.selectAllRows();
                                    }
                                });
                            });
                    }
                }

                var filter = $scope.filter = blade.filter || {
                    objectIds: [blade.currentEntity.id],
                    objectType: blade.type
                };

                filter.criteriaChanged = function () {
                    if ($scope.pageSettings.currentPage > 1) {
                        $scope.pageSettings.currentPage = 1;
                    } else {
                        blade.refresh();
                    }
                };

                blade.getSearchCriteria = function () {
                    return angular.extend(filter, {
                        skip: ($scope.pageSettings.currentPage - 1) * $scope.pageSettings.itemsPerPageCount,
                        take: $scope.pageSettings.itemsPerPageCount
                    });
                };

                blade.toolbarCommands = [
                    {
                        name: "platform.commands.add",
                        icon: 'fa fa-plus',
                        permission: blade.updatePermission,
                        executeMethod: function () {
                            $scope.openAddLinksBlade();
                        },
                        canExecuteMethod: function () {
                            return true;
                        }
                    },
                    {
                        name: "platform.commands.delete",
                        icon: 'fa fa-trash-o',
                        permission: blade.updatePermission,
                        executeMethod: function () {
                            deleteList($scope.gridApi.selection.getSelectedRows());
                        },
                        canExecuteMethod: function () {
                            return $scope.gridApi && _.any($scope.gridApi.selection.getSelectedRows());
                        }
                    },
                ];

                function deleteList(selection) {
                    var dialog = {
                        id: "confirmDelete",
                        title: "catalog.dialogs.links-delete.title",
                        message: "catalog.dialogs.links-delete.message",
                        callback: function (remove) {
                            if (remove) {
                                blade.isLoading = true;

                                var links = _.map(selection, function (x) {
                                    return {
                                        listEntryId: blade.currentEntity.id,
                                        catalogId: x.catalogId,
                                        categoryId: x.categoryId
                                    };
                                });

                                listEntries.deletelinks(links,
                                    function () {
                                        blade.refresh();
                                    },
                                    function (error) {
                                        bladeNavigationService.setError(error, blade);
                                    });
                            }
                        }
                    };

                    dialogService.showConfirmationDialog(dialog);
                }

                function setEntryPath(data) {
                    _.each(data, function (x) {
                        var names = [];

                        if (x.category) {
                            var outline = _.first(x.category.outlines);
                            if (outline) {
                                names = _.pluck(outline.items, 'name');

                            }
                        }

                        x.$$path = _.any(names) ? '\/' + names.join("\/") : '\/';
                    });
                }

                $scope.openAddLinksBlade = function () {
                    var selection = [];

                    var options = {
                        selectedItemIds: [],
                        allowCheckingCategory: true,
                        // do not display products
                        withProducts: false,
                        // display only virtual catalogs (only works for catalogs)
                        isVirtual: true,
                        // switch to select 'categories' template
                        getBladeForCategories: function (listItem, categoriesBlade) {
                            categoriesBlade.template = 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/links-list/categories-items-select-links.tpl.html';
                            return categoriesBlade;
                        },
                        checkItemFn: function (listItem, isSelected) {
                            if (isSelected) {
                                if (!_.find(selection, function (x) { return x.id == listItem.id; })) {
                                    selection.push(listItem);
                                }
                            }
                            else {
                                selection = _.reject(selection, function (x) { return x.id == listItem.id; });
                            }
                        },
                        // don't select items with 'marked' true or item with links (most likely virtual category link)
                        isItemSelectable: function (item) {
                            var isLinkToSameCategory = blade.type === "Category" ? item.id === blade.currentEntity.id : false; //prevent self-linking for categories
                            return (!item.links || !item.links.length) && !item.marked && !isLinkToSameCategory;
                        },
                        // find linked categories to place 'Marked' mark
                        onItemsLoaded: function (loadedCategories) {
                            if (!loadedCategories.length)
                                return;

                            var loadedCategoriesIds = _.pluck(loadedCategories, 'id');

                            var searchCriteria = {
                                objectIds: [blade.currentEntity.id],
                                objectType: blade.type,
                                categoryIds: loadedCategoriesIds,
                                take: loadedCategories.length
                            };

                            listEntries.searchlinks(searchCriteria, function (categoryLinks) {
                                _.each(loadedCategories, function (loadedCategory) {
                                    loadedCategory.marked = !!_.find(categoryLinks.results, function (categoryLinkResult) {
                                        return categoryLinkResult.categoryId === loadedCategory.id;
                                    });
                                });
                            });
                        },
                        // find linked categories to place 'Marked' mark
                        onCatalogsLoaded: function (loadedCatalogs) {
                            if (!loadedCatalogs.length)
                                return;

                            var loadedCatalogIds = _.pluck(loadedCatalogs, 'id');

                            var searchCriteria = {
                                objectIds: [blade.currentEntity.id],
                                objectType: blade.type,
                                catalogIds: loadedCatalogIds,
                                take: loadedCatalogs.length
                            };

                            listEntries.searchlinks(searchCriteria, function (catalogLinkData) {
                                _.each(loadedCatalogs, function (loadedCatalog) {
                                    loadedCatalog.marked = !!_.find(catalogLinkData.results, function (catalogLinkResult) {
                                        return catalogLinkResult.catalogId === loadedCatalog.id;
                                    });
                                });
                            });
                        }
                    };

                    var newBlade = {
                        id: "CatalogItemsSelect",
                        controller: 'virtoCommerce.catalogModule.catalogItemSelectController',
                        template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/links-list/catalog-items-select-links.tpl.html',
                        options: options,
                        breadcrumbs: [],
                        toolbarCommands: [
                            {
                                name: "platform.commands.confirm", icon: 'fa fa-check',
                                executeMethod: function (selectBlade) {
                                    blade.isLoading = true;
                                    bladeNavigationService.closeBlade(selectBlade);

                                    var listEntryLinks = [];
                                    angular.forEach(selection, function (listItem) {
                                        var link = {
                                            listEntryId: blade.currentEntity.id,
                                            listEntryType: blade.type
                                        };

                                        if (listItem.type === 'category') {
                                            link.categoryId = listItem.id;
                                            link.catalogId = listItem.catalogId;
                                        }
                                        else {
                                            link.catalogId = listItem.id;
                                        }

                                        listEntryLinks.push(link);
                                    });

                                    listEntries.createlinks(listEntryLinks, function () {
                                        blade.refresh();
                                    },
                                    function (error) { bladeNavigationService.setError('Error ' + error.status, blade); });
                                },
                                canExecuteMethod: function () {
                                    return _.any(selection);
                                }
                            },
                            {
                                name: "platform.commands.refresh", icon: 'fa fa-refresh',
                                executeMethod: function (selectBlade) {
                                    if (selectBlade.refresh) {
                                        selectBlade.refresh();
                                    }
                                },
                                canExecuteMethod: function () {
                                    return true;
                                }
                            }
                        ]
                    };

                    bladeNavigationService.showBlade(newBlade, blade);
                }

                // ui-grid
                $scope.setGridOptions = function (gridOptions) {
                    bladeUtils.initializePagination($scope, true);
                    $scope.pageSettings.itemsPerPageCount = 50;

                    uiGridHelper.initialize($scope, gridOptions, function (gridApi) {
                        $scope.gridApi = gridApi;

                        uiGridHelper.bindRefreshOnSortChanged($scope);
                        $scope.gridApi.infiniteScroll.on.needLoadMoreData($scope, showMore);
                        gridApi.grid.options.enableGridMenu = false;
                    });

                    $timeout(function () {
                        blade.refresh();
                    });
                };
            }]);
