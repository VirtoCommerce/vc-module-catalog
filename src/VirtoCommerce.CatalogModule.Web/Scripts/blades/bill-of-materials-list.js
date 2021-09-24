angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.billOfMaterialsListController',
        ['$scope', '$timeout',
            'platformWebApp.bladeNavigationService', 'platformWebApp.uiGridHelper', 'platformWebApp.bladeUtils', 'platformWebApp.dialogService',
            'virtoCommerce.catalogModule.items', 'virtoCommerce.catalogModule.categories', 'virtoCommerce.catalogModule.associations',
            function ($scope, $timeout, bladeNavigationService, uiGridHelper, bladeUtils, dialogService, items, categories, associations) {
                $scope.uiGridConstants = uiGridHelper.uiGridConstants;
                $scope.hasMore = true;

                var selectedNode = null;
                var blade = $scope.blade;

                blade.headIcon = 'far fa-list-alt';
                blade.title = 'catalog.blades.bill-of-materials-list.title';
                blade.subtitle = 'catalog.blades.bill-of-materials-list.subtitle';
                blade.cupImage = 'Modules/$(VirtoCommerce.Catalog)/Content/images/cup.svg';

                blade.associationType = "BillOfMaterials";
                blade.associatedObjectType = 'product';
                blade.currentEntities = [];
                blade.filter = {};

                blade.filter.criteriaChanged = function () {
                    blade.refresh();
                };

                blade.refresh = function () {
                    if ($scope.pageSettings.currentPage > 1) {
                        $scope.pageSettings.currentPage = 1;
                    }

                    startPaging();
                };

                function startPaging() {
                    blade.isLoading = true;

                    associations.search(getSearchCriteria(), function (data) {
                        blade.isLoading = false;

                        populateAssociationsData(data.results);
                        blade.currentEntities = data.results;

                        $scope.pageSettings.totalItems = data.totalCount;
                        $scope.hasMore = data.results.length === $scope.pageSettings.itemsPerPageCount;

                        if (selectedNode) {
                            //re-select the node in the new list
                            selectedNode = _.findWhere(data.results, { id: selectedNode.id });
                        }
                    });

                    if ($scope.gridApi) {
                        blade.currentEntities = [];
                        $scope.gridApi.infiniteScroll.resetScroll(true, true);
                        $scope.gridApi.infiniteScroll.dataLoaded();
                    }
                }

                function loadMore() {
                    if ($scope.hasMore) {
                        blade.isLoading = true;

                        ++$scope.pageSettings.currentPage;
                        $scope.gridApi.infiniteScroll.saveScrollPercentage();

                        associations.search(getSearchCriteria(), function (data) {
                            blade.isLoading = false;

                            populateAssociationsData(data.results);
                            blade.currentEntities = blade.currentEntities.concat(data.results);

                            $scope.pageSettings.totalItems = data.totalCount;
                            $scope.hasMore = data.results.length === $scope.pageSettings.itemsPerPageCount;
                            $scope.gridApi.infiniteScroll.dataLoaded();
                        });
                    }
                }

                function getSearchCriteria() {
                    return {
                        keyword: blade.filter.keyword,
                        skip: ($scope.pageSettings.currentPage - 1) * $scope.pageSettings.itemsPerPageCount,
                        take: $scope.pageSettings.itemsPerPageCount,
                        objectIds: [blade.item.id],
                        group: blade.associationType
                    };
                }

                function populateAssociationsData(productAssociations) {
                    if (!_.some(productAssociations)) {
                        return;
                    }

                    blade.isLoading = true;

                    var itemIds = _.pluck(productAssociations, 'associatedObjectId');
                    items.plenty({ respGroup: 'WithImages' }, itemIds, function (products) {
                        var uniqueCategoriesIds = _.uniq(_.pluck(products, 'categoryId'));

                        categories.plenty({ respGroup: 'Info' }, uniqueCategoriesIds, function (categoriesResult) {
                            addAssociationProperties(productAssociations, products, categoriesResult);

                            blade.isLoading = false;
                        });
                    });
                }

                function addAssociationProperties(productAssociations, products, productCategories) {
                    _.each(productAssociations, function (x) {
                        x.$$quantity = x.quantity;
                        x.associatedObjectType = blade.associatedObjectType;

                        var item = _.find(products, function (d) { return d.id === x.associatedObjectId; });
                        if (item) {
                            x.$$productName = item.name;

                            x.$$categoryId = item.categoryId;
                            x.$$category = _.find(productCategories, function (c) { return c.id === x.$$categoryId; });

                            if (item.images && item.images.length) {
                                x.$$imageUrl = item.images[0].url;
                            }

                            if (x.associatedObjectType === 'product') {
                                x.$$productType = item.productType;
                            }
                        }
                    });
                }

                $scope.openProduct = function (listItem) {
                    $scope.selectedNodeId = listItem.associatedObjectId;

                    var newBlade = {
                        id: 'billOfMaterialsDetail',
                        itemId: listItem.associatedObjectId,
                        productType: listItem.$$productType,
                        catalog: blade.catalog,
                        controller: 'virtoCommerce.catalogModule.itemDetailController',
                        template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/item-detail.tpl.html'
                    };

                    bladeNavigationService.showBlade(newBlade, blade);
                };

                $scope.isValidQuantity = function (listItem) {
                    return listItem.quantity && listItem.quantity > 0;
                }

                $scope.updateQuantity = function (listItem) {
                    if (listItem.$$quantity !== listItem.quantity && $scope.isValidQuantity(listItem)) {
                        blade.isLoading = true;

                        associations.update([listItem], function () {
                            listItem.$$quantity = listItem.quantity;

                            blade.isLoading = false;
                        });
                    }
                }

                $scope.deleteList = function (list) {
                    bladeNavigationService.closeChildrenBlades(blade,
                        function () {
                            var dialog = {
                                id: "confirmDeleteBomAssociaitonItem",
                                title: "catalog.dialogs.bill-or-materials-associations-delete.title",
                                message: "catalog.dialogs.bill-or-materials-associations-delete.message",
                                callback: function (remove) {
                                    if (remove) {
                                        bladeNavigationService.closeChildrenBlades(blade, function () {
                                            blade.isLoading = true;

                                            var itemIds = _.pluck(list, 'id');
                                            associations.delete({ ids: itemIds }, function () {
                                                blade.refresh();
                                            });
                                        });
                                    }
                                }
                            };
                            dialogService.showConfirmationDialog(dialog);
                        });
                };

                $scope.openAddItemBlade = function () {
                    var selection = [];

                    var options = {
                        allowCheckingCategory: false,
                        selectedItemIds: [],
                        excludeProductType: 'BillOfMaterials',
                        checkItemFn: function (listItem, isSelected) {
                            if (isSelected) {
                                if (!_.find(selection, function (x) { return x.id == listItem.id; })) {
                                    selection.push(listItem);
                                }
                            }
                            else {
                                selection = _.reject(selection, function (x) { return x.id == listItem.id; });
                            }
                        }
                    };

                    var newBlade = {
                        id: "CatalogItemsSelect",
                        controller: 'virtoCommerce.catalogModule.catalogItemSelectController',
                        template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/common/catalog-items-select.tpl.html',
                        options: options,
                        breadcrumbs: [],
                        toolbarCommands: [
                            {
                                name: "platform.commands.confirm", icon: 'fa fa-check',
                                executeMethod: function (selectBlade) {
                                    blade.isLoading = true;
                                    bladeNavigationService.closeBlade(selectBlade);

                                    // search for associated products
                                    var selectedProductIds = _.pluck(selection, 'id');
                                    var searchCriteria = {
                                        take: selectedProductIds.length,
                                        objectIds: [blade.item.id],
                                        group: blade.associationType,
                                        associatedObjectIds: selectedProductIds
                                    };

                                    associations.search(searchCriteria, function (data) {
                                        // filter out already associated products, if any
                                        if (data.totalCount) {
                                            selectedProductIds = _.filter(selectedProductIds, function (productId) {
                                                return !_.some(data.results, function (association) {
                                                    return productId === association.associatedObjectId;
                                                });
                                            });
                                        }

                                        // save only unique product associations
                                        if (selectedProductIds.length) {
                                            var newAssociations = _.map(selectedProductIds, function (id) {
                                                return {
                                                    associatedObjectId: id,
                                                    associatedObjectType: blade.associatedObjectType,
                                                    itemId: blade.item.id,
                                                    quantity: 1,
                                                    type: blade.associationType
                                                }
                                            });

                                            blade.isLoading = true;
                                            associations.update(newAssociations, function () {
                                                blade.refresh();
                                            });
                                        }

                                        blade.isLoading = false;
                                    });
                                },
                                canExecuteMethod: function () {
                                    return _.any(selection);
                                }
                            }]
                    };

                    bladeNavigationService.showBlade(newBlade, blade);
                }

                blade.toolbarCommands = [
                    {
                        name: "platform.commands.add", icon: 'fas fa-plus',
                        executeMethod: function () {
                            $scope.openAddItemBlade();
                        },
                        canExecuteMethod: function () {
                            return true;
                        }
                    },
                    {
                        name: "platform.commands.delete", icon: 'fas fa-trash-alt',
                        executeMethod: function () {
                            $scope.deleteList($scope.gridApi.selection.getSelectedRows());
                        },
                        canExecuteMethod: function () {
                            return $scope.gridApi && _.any($scope.gridApi.selection.getSelectedRows());
                        }
                    }
                ];

                // ui-grid
                $scope.setGridOptions = function (gridOptions) {
                    bladeUtils.initializePagination($scope, true);
                    $scope.pageSettings.itemsPerPageCount = 20;

                    uiGridHelper.initialize($scope, gridOptions, function (gridApi) {
                        $scope.gridApi = gridApi;

                        uiGridHelper.bindRefreshOnSortChanged($scope);
                        $scope.gridApi.infiniteScroll.on.needLoadMoreData($scope, loadMore);
                    });

                    // need to call refresh after digest cycle as we do not "$watch" for $scope.pageSettings.currentPage
                    $timeout(function () {
                        blade.refresh();
                    });
                };
            }]);
