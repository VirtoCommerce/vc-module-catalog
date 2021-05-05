angular.module('virtoCommerce.catalogModule').controller('virtoCommerce.catalogModule.itemVariationListController', ['$scope', 'platformWebApp.bladeNavigationService', 'platformWebApp.dialogService', 'virtoCommerce.catalogModule.items', 'filterFilter', 'uiGridConstants', 'platformWebApp.uiGridHelper', 'virtoCommerce.catalogModule.search', 'platformWebApp.bladeUtils', function ($scope, bladeNavigationService, dialogService, items, filterFilter, uiGridConstants, uiGridHelper, search, bladeUtils) {
    $scope.uiGridConstants = uiGridConstants;
    var blade = $scope.blade;
    blade.title = blade.item.name;
    blade.subtitle = 'catalog.widgets.itemVariation.blade-subtitle';


    blade.refresh = function (pageNumber) {

        if (!pageNumber) {
            blade.parentBlade.refresh();
            return;
        }

        blade.isLoading = true;
        var searchCriteria = {
            mainProductId: blade.item.id,
            responseGroup: 'withProducts',
            objectType: 'CatalogProduct',
            sort: uiGridHelper.getSortExpression($scope),
            skip: ($scope.pageSettings.currentPage - 1) * $scope.pageSettings.itemsPerPageCount,
            take: $scope.pageSettings.itemsPerPageCount,
            withHidden: true
        };

        search.searchProducts(
            searchCriteria,
            function (data) {
                blade.isLoading = false;
                $scope.pageSettings.totalItems = data.totalCount;
                blade.variations = data.results;
                $scope.hasMore = data.results.length === $scope.pageSettings.itemsPerPageCount;
            });
    };


    blade.setSelectedItem = function (listItem) {
        $scope.selectedNodeId = listItem.id;
    };

    $scope.selectVariation = function (listItem) {
        blade.setSelectedItem(listItem);
        var newBlade = {
            id: 'variationDetail',
            itemId: listItem.id,
            productType: listItem.productType,
            title: listItem.code,
            catalog: blade.catalog,
            subtitle: 'catalog.blades.item-detail.subtitle-variation',
            controller: 'virtoCommerce.catalogModule.itemDetailController',
            template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/item-detail.tpl.html'
        };
        bladeNavigationService.showBlade(newBlade, blade);
    };

    $scope.deleteList = function (list) {
        bladeNavigationService.closeChildrenBlades(blade, function () {
            var dialog = {
                id: "confirmDeleteItem",
                title: "catalog.dialogs.variation-delete.title",
                message: "catalog.dialogs.variation-delete.message",
                callback: function (remove) {
                    if (remove) {
                        var ids = _.pluck(list, 'id');
                        items.remove({ ids: ids }, blade.parentBlade.refresh, function (error) { bladeNavigationService.setError('Error ' + error.status, blade); });
                    }
                }
            }
            dialogService.showConfirmationDialog(dialog);
        });
    };


    blade.headIcon = 'fa fa-dropbox';

    if (blade.toolbarCommandsAndEvents && blade.toolbarCommandsAndEvents.toolbarCommands) {
        blade.toolbarCommands = blade.toolbarCommandsAndEvents.toolbarCommands;
    } else
        blade.toolbarCommands = [
            {
                name: "platform.commands.refresh", icon: 'fa fa-refresh',
                executeMethod: function () {
                    blade.parentBlade.refresh()
                },
                canExecuteMethod: function () { return true; }
            },
            {
                name: "platform.commands.add", icon: 'fas fa-plus',
                executeMethod: function () {
                    items.newVariation({ itemId: blade.item.id }, function (data) {
                        // take variation properties only
                        data.properties = _.where(data.properties, { type: 'Variation' });
                        data.productType = blade.item.productType;

                        if (data.productType === 'Digital'
                            || data.productType === 'BillOfMaterials') {
                            data.trackInventory = false;
                        }

                        var newBlade = {
                            id: 'variationDetail',
                            item: data,
                            catalog: blade.catalog,
                            title: "catalog.wizards.new-variation.title",
                            subtitle: 'catalog.wizards.new-variation.subtitle',
                            controller: 'virtoCommerce.catalogModule.newProductWizardController',
                            template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/wizards/newProduct/new-variation-wizard.tpl.html'
                        };
                        bladeNavigationService.showBlade(newBlade, blade);
                    },
                        function (error) { bladeNavigationService.setError('Error ' + error.status, blade); });
                },
                canExecuteMethod: function () { return true; },
                permission: 'catalog:create'
            },
            {
                name: "platform.commands.delete", icon: 'fas fa-trash-alt',
                executeMethod: function () {
                    $scope.deleteList($scope.gridApi.selection.getSelectedRows());
                },
                canExecuteMethod: function () {
                    return $scope.gridApi && _.any($scope.gridApi.selection.getSelectedRows());
                },
                permission: 'catalog:delete'
            }
        ];

    // simple and advanced filtering
    var filter = $scope.filter = {};

    filter.criteriaChanged = function () {
        if ($scope.pageSettings.currentPage > 1) {
            $scope.pageSettings.currentPage = 1;
        } else {
            blade.refresh();
        }
    };

    // ui-grid
    $scope.setGridOptions = function (gridOptions) {
        uiGridHelper.initialize($scope, gridOptions, function (gridApi) {
            gridApi.grid.registerRowsProcessor($scope.singleFilter, 90);
            uiGridHelper.bindRefreshOnSortChanged($scope);
        });
        bladeUtils.initializePagination($scope);
    };

    $scope.singleFilter = function (renderableRows) {
        var visibleCount = 0;
        renderableRows.forEach(function (row) {
            row.visible = _.any(filterFilter([row.entity], blade.searchText));
            if (row.visible) visibleCount++;
        });

        $scope.filteredEntitiesCount = visibleCount;
        return renderableRows;
    };

}]);
