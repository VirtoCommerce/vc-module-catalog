// Call this to register your module to main application
var moduleName = "virtoCommerce.catalogBulkActionsModule";

if (AppDependencies !== undefined) {
    AppDependencies.push(moduleName);
}

angular.module(moduleName, [])
    .config(['$stateProvider', '$urlRouterProvider',
        function ($stateProvider, $urlRouterProvider) {
            $stateProvider
                .state('workspace.virtoCommerceCatalogBulkActionsModuleState', {
                    url: '/virtoCommerce.catalogBulkActionsModule',
                    templateUrl: '$(Platform)/Scripts/common/templates/home.tpl.html',
                    controller: [
                        '$scope', 'platformWebApp.bladeNavigationService', function ($scope, bladeNavigationService) {
                            var newBlade = {
                                id: 'blade1',
                                controller: 'virtoCommerce.catalogBulkActionsModule.helloWorldController',
                                template: 'Modules/$(virtoCommerce.catalog)/Scripts/blades/hello-world.html',
                                isClosingDisabled: true
                            };
                            bladeNavigationService.showBlade(newBlade);
                        }
                    ]
                });
        }
    ])
    .run([
        '$rootScope',
        'platformWebApp.mainMenuService',
        'platformWebApp.widgetService',
        '$state', 'platformWebApp.toolbarService',
        'platformWebApp.bladeUtils',
        'virtoCommerce.catalogModule.catalogBulkActionService',
        'platformWebApp.bladeNavigationService',
        function ($rootScope,
            mainMenuService,
            widgetService,
            $state,
            toolbarService,
            bladeUtils,
            catalogBulkActionService,
            bladeNavigationService) {

            function isItemsChecked(blade) {
                var gridApi = blade.$scope.gridApi;
                return gridApi && _.any(gridApi.selection.getSelectedRows());
            }

            //Register module in main menu
            //var menuItem = {
            //    path: 'browse/virtoCommerce.catalogBulkActionsModule',
            //    icon: 'fa fa-cube',
            //    title: 'VirtoCommerce.CatalogBulkActionsModule',
            //    priority: 100,
            //    action: function () { $state.go('workspace.virtoCommerceCatalogBulkActionsModuleState'); },
            //    permission: 'virtoCommerce.catalogBulkActionsModule.WebPermission'
            //};
            //mainMenuService.addMenuItem(menuItem);

            catalogBulkActionService.register({
                name: 'CategoryChangeBulkAction',
                controller: 'virtoCommerce.catalogModule.changeCategoryActionStepsController',
                template: 'Modules/$(virtoCommerce.catalog)/Scripts/blades/action-change-category.tpl.html'
            });

            catalogBulkActionService.register({
                name: 'PropertiesUpdateBulkAction',
                controller: 'virtoCommerce.catalogModule.editPropertiesActionController',
                template: 'Modules/$(virtoCommerce.catalog)/Scripts/blades/action-edit-properties.tpl.html'
            });


            toolbarService.register({
                name: "Bulk Actions", icon: 'fa fa-cubes',
                executeMethod: function (blade) {
                    var gridApi = blade.$scope.gridApi;
                    var filter = blade.filter;

                    var newBlade = {
                        id: 'catalogBulkActions',
                        title: 'catalogBulkActions.blades.exporter-list.title',
                        subtitle: 'catalogBulkActions.blades.exporter-list.subtitle',
                        catalog: blade.catalog,
                        controller: 'virtoCommerce.catalogModule.actionListController',
                        template: 'Modules/$(virtoCommerce.catalog)/Scripts/blades/action-list.tpl.html',
                        selectedProducts: _.filter(gridApi.selection.getSelectedRows(), function (x) { return x.type === 'product' }),
                        selectedCategories: _.filter(gridApi.selection.getSelectedRows(), function (x) { return x.type === 'category' }),
                        dataQuery: {}
                    };

                    if (gridApi.selection.getSelectAllState()) {
                        var searchData = {
                            searchCriteria: {
                                catalogId: blade.catalogId,
                                categoryId: blade.categoryId,
                                keyword: filter.keyword ? filter.keyword : undefined,
                                searchInVariations: filter.searchInVariations ? filter.searchInVariations : false,
                                responseGroup: 'withCategories, withProducts',
                                //sort: uiGridHelper.getSortExpression($scope),
                                //skip: ($scope.pageSettings.currentPage - 1) * $scope.pageSettings.itemsPerPageCount,
                                //take: $scope.pageSettings.itemsPerPageCount,
                                skip: undefined,
                                take: undefined
                            }
                        };
                        angular.extend(newBlade.dataQuery, searchData);
                    }
                    else {
                        var data = {
                            listEntries: gridApi.selection.getSelectedRows()
                        };

                        angular.extend(newBlade.dataQuery, data);
                    }
                    
                    bladeNavigationService.showBlade(newBlade);

                },
                canExecuteMethod: isItemsChecked,
                index: 20
            }, 'virtoCommerce.catalogModule.categoriesItemsListController');
        }
]);
