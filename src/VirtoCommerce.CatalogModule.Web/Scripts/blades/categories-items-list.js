angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.categoriesItemsListController', [
        '$sessionStorage', '$location', '$timeout', '$scope', 'virtoCommerce.catalogModule.listEntries', 'platformWebApp.bladeUtils', 'platformWebApp.dialogService', 'platformWebApp.authService', 'platformWebApp.uiGridHelper', 'virtoCommerce.catalogModule.catalogs',
        function ($sessionStorage, $location, $timeout, $scope, listEntries, bladeUtils, dialogService, authService, uiGridHelper, catalogs) {
            $scope.uiGridConstants = uiGridHelper.uiGridConstants;
            $scope.hasMore = true;
            $scope.items = [];

            var blade = $scope.blade;
            var bladeNavigationService = bladeUtils.bladeNavigationService;
            if (blade.catalogId)
                blade.catalog = catalogs.get({ id: blade.catalogId });

            blade.refresh = function () {

                blade.isLoading = true;

                if ($scope.pageSettings.currentPage !== 1)
                    $scope.pageSettings.currentPage = 1;

                var searchCriteria = getSearchCriteria();

                listEntries.listitemssearch(
                    searchCriteria,
                    function (data) {
                        transformByFilters(data.listEntries);
                        blade.isLoading = false;
                        $scope.pageSettings.totalItems = data.totalCount;
                        $scope.items = data.listEntries;
                        $scope.hasMore = data.listEntries.length === $scope.pageSettings.itemsPerPageCount;

                        //Set navigation breadcrumbs
                        setBreadcrumbs();
                    });

                //reset state grid
                resetStateGrid();
            }


            function showMore() {
                if ($scope.hasMore) {

                    ++$scope.pageSettings.currentPage;
                    $scope.gridApi.infiniteScroll.saveScrollPercentage();
                    blade.isLoading = true;

                    var searchCriteria = getSearchCriteria();

                    listEntries.listitemssearch(
                        searchCriteria,
                        function (data) {
                            transformByFilters(data.listEntries);
                            blade.isLoading = false;
                            $scope.pageSettings.totalItems = data.totalCount;
                            $scope.items = $scope.items.concat(data.listEntries);
                            $scope.hasMore = data.listEntries.length === $scope.pageSettings.itemsPerPageCount;
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

            // Search Criteria
            function getSearchCriteria() {
                var sortCriteria = uiGridHelper.getSortExpression($scope);

                var searchCriteria = {
                    catalogId: blade.catalogId,
                    categoryId: blade.categoryId,
                    objectTypes: blade.objectTypes,
                    keyword: filter.keyword ? filter.keyword : undefined,
                    searchInVariations: filter.searchInVariations ? filter.searchInVariations : false,
                    sort: filter.keyword && filter.ignoreSortingForRelevance == sortCriteria ? '' : sortCriteria,
                    skip: ($scope.pageSettings.currentPage - 1) * $scope.pageSettings.itemsPerPageCount,
                    take: $scope.pageSettings.itemsPerPageCount
                };
                if (searchCriteria.keyword && !searchCriteria.sort) {
                    searchCriteria.sort = "relevance";
                }
                return searchCriteria;
            }

            //Breadcrumbs
            function setBreadcrumbs() {
                //Clone array (angular.copy leave a same reference)
                blade.breadcrumbs = blade.breadcrumbs.slice(0);

                if (!blade.catalogId) return;

                //catalog breadcrumb by default
                var breadCrumb = {
                    blade: blade
                };

                //if category need change to category breadcrumb
                if (angular.isDefined(blade.category)) {

                    breadCrumb.id = blade.categoryId;
                    breadCrumb.name = blade.category.name;
                } else {
                    breadCrumb.id = blade.catalogId;
                    blade.catalog.$promise.then((data) => {
                        breadCrumb.name = data.name;
                    });
                }

                //prevent duplicate items
                if (!_.some(blade.breadcrumbs, function (x) { return x.id === breadCrumb.id })) {
                    blade.breadcrumbs.push(breadCrumb);
                }

                breadCrumb.navigate = function () {
                    if (blade.categoryId) {
                        $location.search({ categoryId: blade.categoryId, catalogId: blade.catalogId });
                    }
                    else {
                        $location.search({});
                    }

                    bladeNavigationService.closeBlade(blade,
                        function () {
                            blade.disableOpenAnimation = true;
                            bladeNavigationService.showBlade(blade, blade.parentBlade);
                        });
                };
            }

            //reset state grid (header checkbox, scroll)
            function resetStateGrid() {
                if ($scope.gridApi) {
                    $scope.items = [];
                    $scope.gridApi.selection.clearSelectedRows();
                    $scope.gridApi.infiniteScroll.resetScroll(true, true);
                    $scope.gridApi.infiniteScroll.dataLoaded();
                }
            }

            $scope.copyItemID = function (data) {
                navigator.clipboard.writeText(data.id).then().catch(e => console.error(e));
            };

            $scope.edit = function (listItem) {
                if (listItem.type === 'category') {
                    blade.setSelectedItem(listItem, true);
                    blade.showCategoryBlade(listItem);
                } else
                    $scope.selectItem(null, listItem);
            };

            blade.showCategoryBlade = function (listItem) {
                var newBlade = {
                    id: "listCategoryDetail",
                    currentEntityId: listItem.id,
                    catalog: blade.catalog,
                    title: listItem.name,
                    subtitle: 'catalog.blades.category-detail.subtitle',
                    controller: 'virtoCommerce.catalogModule.categoryDetailController',
                    template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/category-detail.tpl.html',
                };
                bladeNavigationService.showBlade(newBlade, blade);
            };

            $scope.cut = function (data) {
                cutList([data]);
            }

            function cutList(selection) {
                $sessionStorage.catalogClipboardContent = selection;
            }

            $scope.delete = function (data) {
                deleteList([data]);
            };

            function isItemsChecked() {
                return $scope.gridApi && _.any($scope.gridApi.selection.getSelectedRows());
            }

            function deleteList(selection) {
                var listEntryLinks = [];
                var categoryIds = [];
                var itemIds = [];
                angular.forEach(selection, function (listItem) {
                    var deletingLink = false;

                    if (listItem.type === 'category') {
                        if (blade.catalog && blade.catalog.isVirtual
                            && _.some(listItem.links, function (x) {
                                return x.categoryId === blade.categoryId
                                    || !x.categoryId && !blade.categoryId;
                            })) {
                            deletingLink = true;
                        } else {
                            categoryIds.push(listItem.id);
                        }
                    } else {
                        if (blade.catalog && blade.catalog.isVirtual) {
                            deletingLink = true;
                        } else {
                            itemIds.push(listItem.id);
                        }
                    }

                    if (deletingLink)
                        listEntryLinks.push({
                            listEntryId: listItem.id,
                            listEntryType: listItem.type,
                            catalogId: blade.catalogId,
                            categoryId: blade.categoryId,
                        });
                });

                var listCategoryLinkCount = _.where(listEntryLinks, { listEntryType: 'category' }).length;
                let idsToDelete = categoryIds.concat(itemIds);
                var dialog = {
                    id: "confirmDeleteItem",
                    categoryCount: categoryIds.length,
                    itemCount: itemIds.length,
                    listCategoryLinkCount: listCategoryLinkCount,
                    listItemLinkCount: listEntryLinks.length - listCategoryLinkCount,
                    callback: function (remove) {
                        if (remove) {
                            bladeNavigationService.closeChildrenBlades(blade);
                            blade.isLoading = true;

                            if (listEntryLinks.length > 0) {
                                listEntries.deletelinks(listEntryLinks, () => {
                                    blade.refresh();

                                    if (blade.mode === 'mappingSource') {
                                        blade.parentBlade.refresh();
                                    }
                                }
                                );
                            }

                            if (idsToDelete.length > 0) {
                                listEntries.delete({ objectIds: idsToDelete }, () => blade.refresh());
                            }
                        }
                    }
                }
                dialogService.showDialog(dialog, 'Modules/$(VirtoCommerce.Catalog)/Scripts/dialogs/deleteCategoryItem-dialog.tpl.html', 'platformWebApp.confirmDialogController');
            }

            function mapChecked() {
                bladeNavigationService.closeChildrenBlades(blade);

                blade.isLoading = true;

                if ($scope.gridApi && $scope.gridApi.selection.getSelectAllState()) {
                    listEntries.bulkcreatelinks(
                        {
                            SearchCriteria: getSearchCriteria(),
                            CatalogId: blade.parentBlade.catalogId,
                            CategoryId: blade.parentBlade.categoryId
                        },
                        function () {
                            blade.refresh();
                            blade.parentBlade.refresh();
                        },
                        function (error) {
                            bladeNavigationService.setError('Error ' + error.status, blade);
                        }
                    );
                } else {
                    var selection = $scope.gridApi.selection.getSelectedRows();
                    var listEntryLinks = [];
                    angular.forEach(selection, function (listItem) {
                        listEntryLinks.push({
                            listEntryId: listItem.id,
                            listEntryType: listItem.type,
                            catalogId: blade.parentBlade.catalogId,
                            categoryId: blade.parentBlade.categoryId,
                        });
                    });

                    listEntries.createlinks(listEntryLinks, function () {
                        blade.refresh();
                        blade.parentBlade.refresh();
                    },
                        function (error) { bladeNavigationService.setError('Error ' + error.status, blade); });
                }
            }

            blade.setSelectedItem = function (listItem, edit) {
                $scope.selectedNodeId = listItem.id;
                if (listItem.type === 'category') {
                    $location.search({ categoryId: listItem.id, catalogId: listItem.catalogId, edit: edit });
                }
                else {
                    $location.search({ productId: listItem.id });
                }
            };

            $scope.selectItem = function (e, listItem) {
                blade.setSelectedItem(listItem);
                var newBlade;
                if (listItem.type === 'category') {
                    var openNewBlade = e.ctrlKey || filter.keyword;
                    newBlade = {
                        id: 'itemsList' + (blade.level + (openNewBlade ? 1 : 0)),
                        level: blade.level + (openNewBlade ? 1 : 0),
                        mode: blade.mode,
                        isBrowsingLinkedCategory: blade.isBrowsingLinkedCategory || $scope.hasLinks(listItem),
                        breadcrumbs: blade.breadcrumbs,
                        title: 'catalog.blades.categories-items-list.title',
                        subtitle: 'catalog.blades.categories-items-list.subtitle',
                        subtitleValues: listItem.name !== null ? { name: listItem.name } : '',
                        catalogId: blade.catalogId || listItem.catalogId,
                        categoryId: listItem.id,
                        category: listItem,
                        catalog: blade.catalog,
                        disableOpenAnimation: true,
                        controller: 'virtoCommerce.catalogModule.categoriesItemsListController',
                        template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/categories-items-list.tpl.html'
                    };

                    if (openNewBlade) {
                        bladeNavigationService.showBlade(newBlade, blade);
                    } else {
                        bladeNavigationService.closeBlade(blade, function () {
                            bladeNavigationService.showBlade(newBlade, blade.parentBlade);
                        });
                    }
                } else {
                    newBlade = {
                        id: "listItemDetail" + blade.mode,
                        itemId: listItem.id,
                        productType: listItem.productType,
                        title: listItem.name,
                        catalog: blade.catalog,
                        controller: 'virtoCommerce.catalogModule.itemDetailController',
                        template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/item-detail.tpl.html'
                    };
                    bladeNavigationService.showBlade(newBlade, blade);

                    // setting current categoryId to be globally available
                    bladeNavigationService.catalogsSelectedCategoryId = blade.categoryId;
                }
            };

            $scope.hasLinks = function (listEntry) {
                return blade.catalog && blade.catalog.isVirtual &&
                    _.any(listEntry.links, function (l) {
                        return l.catalogId === blade.catalogId && (!blade.categoryId || l.categoryId === blade.categoryId);
                    });
            }

            $scope.getLabels = function (listEntry) {
                if (listEntry.$labels) {
                    return listEntry.$labels;
                }

                listEntry.$labels = [];

                if (listEntry.type === 'category' && !listEntry.isActive) {
                    listEntry.$labels.push({ text: 'catalog.blades.categories-items-list.labels.hidden', important: true });
                }

                if (listEntry.productType) {
                    listEntry.$labels.push({ text: 'catalog.product-types.' + listEntry.productType });
                }

                if (blade.catalog && blade.catalog.isVirtual) {
                    const link = _.find(listEntry.links, function (x) {
                        return x.catalogId === blade.catalogId && (!blade.categoryId || x.categoryId === blade.categoryId);
                    });

                    if (link) {
                        listEntry.$labels.push({ text: link.isAutomatic ? 'catalog.link-types.automatic' : 'catalog.link-types.manual' });
                    }
                }

                return listEntry.$labels;
            };

            blade.toolbarCommands = [
                {
                    name: "platform.commands.refresh",
                    icon: 'fa fa-refresh',
                    executeMethod: blade.refresh,
                    canExecuteMethod: function () {
                        return true;
                    }
                },
                {
                    name: "platform.commands.delete",
                    icon: 'fas fa-trash-alt',
                    executeMethod: function () { deleteList($scope.gridApi.selection.getSelectedRows()); },
                    canExecuteMethod: isItemsChecked,
                    permission: 'catalog:delete'
                },
                {
                    name: "platform.commands.import",
                    icon: 'fa fa-download',
                    executeMethod: function () {
                        var newBlade = {
                            id: 'catalogImport',
                            title: 'catalog.blades.importers-list.title',
                            subtitle: 'catalog.blades.importers-list.subtitle',
                            catalog: blade.catalog,
                            controller: 'virtoCommerce.catalogModule.importerListController',
                            template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/import/importers-list.tpl.html'
                        };
                        bladeNavigationService.showBlade(newBlade, blade);
                    },
                    canExecuteMethod: function () { return blade.catalogId; },
                    permission: 'catalog:import'
                },
                {
                    name: "platform.commands.export",
                    icon: 'fa fa-upload',
                    executeMethod: function () {
                        var newBlade = {
                            id: 'catalogExport',
                            title: 'catalog.blades.exporter-list.title',
                            subtitle: 'catalog.blades.exporter-list.subtitle',
                            catalog: blade.catalog,
                            controller: 'virtoCommerce.catalogModule.exporterListController',
                            template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/export/exporter-list.tpl.html',
                            selectedProducts: _.filter($scope.gridApi.selection.getSelectedRows(), function (x) { return x.type === 'product' }),
                            selectedCategories: _.filter($scope.gridApi.selection.getSelectedRows(), function (x) { return x.type === 'category' }),
                            isAllSelected: $scope.gridApi.selection.getSelectAllState()
                        };
                        bladeNavigationService.showBlade(newBlade, blade);
                    },
                    canExecuteMethod: function () { return $scope.items.length && blade.catalogId; },
                    permission: 'catalog:export'
                },
                {
                    name: "platform.commands.cut",
                    icon: 'fas fa-cut',
                    executeMethod: function () {
                        cutList($scope.gridApi.selection.getSelectedRows());
                    },
                    canExecuteMethod: isItemsChecked,
                    permission: 'catalog:create'
                },
                {
                    name: "platform.commands.paste",
                    icon: 'fas fa-paste',
                    executeMethod: function () {
                        blade.isLoading = true;
                        listEntries.move({
                            catalog: blade.catalogId,
                            category: blade.categoryId,
                            listEntries: $sessionStorage.catalogClipboardContent
                        }, function () {
                            delete $sessionStorage.catalogClipboardContent;
                            blade.refresh();
                        }, function (error) {
                            bladeNavigationService.setError('Error ' + error.status, blade);
                        });
                    },
                    canExecuteMethod: function () {
                        return $sessionStorage.catalogClipboardContent && blade.catalog && !blade.catalog.isVirtual;
                    },
                    permission: 'catalog:create'
                }
            ];

            if (blade.isBrowsingLinkedCategory) {
                blade.toolbarCommands.splice(1, 5);
            }

            if (angular.isDefined(blade.mode)) {
                // mappingSource
                if (blade.mode === 'mappingSource') {
                    var mapCommand = {
                        name: "catalog.commands.map",
                        icon: 'fas fa-link',
                        executeMethod: function () {
                            mapChecked();
                        },
                        canExecuteMethod: isItemsChecked
                    }
                    blade.toolbarCommands.splice(1, 5, mapCommand);
                }
            } else if (!blade.isBrowsingLinkedCategory && authService.checkPermission('catalog:create')) {
                blade.toolbarCommands.splice(1, 0, {
                    name: "platform.commands.add",
                    icon: 'fas fa-plus',
                    executeMethod: function () {
                        var newBlade = {
                            id: 'listItemChild',
                            catalog: blade.catalog,
                            title: 'catalog.blades.categories-items-add.title',
                            subtitle: 'catalog.blades.categories-items-add.subtitle',
                            controller: 'virtoCommerce.catalogModule.categoriesItemsAddController',
                            template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/categories-items-add.tpl.html'
                        };
                        bladeNavigationService.showBlade(newBlade, blade);
                    },
                    canExecuteMethod: function () { return blade.catalogId; }
                });
            }

            blade.onAfterCatalogSelected = function (selectedNode) {
                var newBlade = {
                    id: 'itemsList' + (blade.level + 1),
                    level: blade.level + 1,
                    mode: 'mappingSource',
                    breadcrumbs: [],
                    title: 'catalog.blades.categories-items-list.title-mapping',
                    subtitle: 'catalog.blades.categories-items-list.subtitle-mapping',
                    catalogId: selectedNode.id,
                    catalog: selectedNode,
                    disableOpenAnimation: true,
                    controller: 'virtoCommerce.catalogModule.categoriesItemsListController',
                    template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/categories-items-list.tpl.html'
                };
                bladeNavigationService.showBlade(newBlade, blade);
            };


            // simple and advanced filtering
            var filter = blade.filter = { keyword: blade.filterKeyword };

            filter.criteriaChanged = function () {
                $scope.appliedKeyword = filter.keyword;
                filter.appliedKeyword = filter.keyword;
                if (!blade.catalogId && !filter.keyword) {
                    $scope.bladeClose();
                } else if ($scope.pageSettings.currentPage > 1) {
                    $scope.pageSettings.currentPage = 1;
                } else {
                    blade.refresh();
                }
            };

            function transformByFilters(data) {
                _.each(data, function (x) {
                    x.$path = _.any(x.path) ? x.path.join(" \\ ") : '\\';
                });
            }

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
                    $scope.gridApi.infiniteScroll.on.needLoadMoreData($scope, showMore);
                });
            };

            //No need to call this because page 'pageSettings.currentPage' is watched!!! It would trigger subsequent duplicated req...
            //blade.refresh();
        }]);
