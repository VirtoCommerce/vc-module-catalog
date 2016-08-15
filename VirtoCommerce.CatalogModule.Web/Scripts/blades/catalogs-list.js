angular.module('virtoCommerce.catalogModule')
.controller('virtoCommerce.catalogModule.catalogsListController', ['$scope', 'virtoCommerce.catalogModule.catalogs', 'platformWebApp.bladeNavigationService', 'platformWebApp.dialogService', 'platformWebApp.authService', 'platformWebApp.uiGridHelper', 'filterFilter', 'platformWebApp.bladeUtils',
function ($scope, catalogs, bladeNavigationService, dialogService, authService, uiGridHelper, filterFilter, bladeUtils) {
    $scope.uiGridConstants = uiGridHelper.uiGridConstants;
    var blade = $scope.blade;
    var selectedNode = null;

    blade.refresh = function () {
        blade.isLoading = true;

        catalogs.getCatalogs({
            sort: uiGridHelper.getSortExpression($scope),
            start: ($scope.pageSettings.currentPage - 1) * $scope.pageSettings.itemsPerPageCount,
            count: $scope.pageSettings.itemsPerPageCount
        }, function (results) {
            blade.isLoading = false;
            //filter the catalogs in which we not have access
            blade.currentEntities = results;

            if (selectedNode) {
                //select the node in the new list
                selectedNode = _.findWhere(results, { id: selectedNode.id });
            }
        },
        function (error) { bladeNavigationService.setError('Error ' + error.status, $scope.blade); });
    };

    $scope.selectNode = function (node) {
        selectedNode = node;
        $scope.selectedNodeId = selectedNode.id;

        var newBlade = {
            id: 'itemsList1',
            level: 1,
            breadcrumbs: blade.breadcrumbs,
            title: 'catalog.blades.categories-items-list.title',
            subtitle: 'catalog.blades.categories-items-list.subtitle',
            subtitleValues: { name: selectedNode.name },
            catalogId: selectedNode.id,
            catalog: selectedNode,
            controller: 'virtoCommerce.catalogModule.categoriesItemsListController',
            template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/categories-items-list.tpl.html',
            securityScopes: selectedNode.securityScopes
        };

        bladeNavigationService.showBlade(newBlade, blade);

        // setting current catalog to be globally available 
        bladeNavigationService.catalogsSelectedCatalog = selectedNode;
        bladeNavigationService.catalogsSelectedCategoryId = undefined;
    };

    $scope.editCatalog = function (catalog) {
        if (catalog.isVirtual) {
            showVirtualCatalogBlade(catalog.id, null, catalog.name);
        }
        else {
            showCatalogBlade(catalog.id, null, catalog.name);
        }
    };

    $scope.deleteCatalog = function (node) {
        var dialog = {
            id: "confirmDelete",
            name: node.name,
            callback: function (remove) {
                if (remove) {
                    bladeNavigationService.closeChildrenBlades(blade, function () {
                        selectedNode = undefined;
                        $scope.selectedNodeId = undefined;
                        blade.isLoading = true;
                        catalogs.delete({ id: node.id },
                            blade.refresh,
                            function (error) { bladeNavigationService.setError('Error ' + error.status, blade); }
                        );
                    });
                }
            }
        };
        dialogService.showDialog(dialog, 'Modules/$(VirtoCommerce.Catalog)/Scripts/dialogs/deleteCatalog-dialog.tpl.html', 'platformWebApp.confirmDialogController');
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
            name: "platform.commands.refresh", icon: 'fa fa-refresh',
            executeMethod: blade.refresh,
            canExecuteMethod: function () {
                return true;
            }
        }
    ];

    if (authService.checkPermission('catalog:create')) {
        blade.toolbarCommands.splice(1, 0, {
            name: "platform.commands.add",
            icon: 'fa fa-plus',
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

    // actions on load
    // blade.refresh();
}]);