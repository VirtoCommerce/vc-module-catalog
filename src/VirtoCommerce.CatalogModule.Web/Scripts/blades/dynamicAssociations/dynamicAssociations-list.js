angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.dynamicAssociationsListController', ['$scope', '$localStorage', 'virtoCommerce.catalogModule.dynamicAssociations', 'platformWebApp.dialogService', 'platformWebApp.bladeUtils', 'platformWebApp.uiGridHelper',
        function ($scope, $localStorage, associations, dialogService, bladeUtils, uiGridHelper) {
            var blade = $scope.blade;
            var bladeNavigationService = bladeUtils.bladeNavigationService;

            blade.refresh = function () {
                blade.isLoading = true;

                var criteria = {
                    keyword: filter.keyword,
                    sort: uiGridHelper.getSortExpression($scope),
                    skip: ($scope.pageSettings.currentPage - 1) * $scope.pageSettings.itemsPerPageCount,
                    take: $scope.pageSettings.itemsPerPageCount
                };
                if (filter.current) {
                    angular.extend(criteria, filter.current);
                }

                associations.search(criteria, function (data) {
                    blade.isLoading = false;

                    $scope.pageSettings.totalItems = data.totalCount;
                    blade.currentEntities = data.results;
                });
            };

            $scope.selectNode = function (node) {
                $scope.selectedNodeId = node.id;

                //var newBlade = {
                //    id: 'listItemChild',
                //    currentEntityId: node.id,
                //    title: node.name,
                //    subtitle: blade.subtitle,
                //    controller: 'virtoCommerce.marketingModule.promotionDetailController',
                //    template: 'Modules/$(VirtoCommerce.Marketing)/Scripts/promotion/blades/promotion-detail.tpl.html'
                //};

                bladeNavigationService.showBlade(newBlade, blade);
            };

            $scope.deleteList = function (list) {
                var dialog = {
                    id: "confirmDeleteItem",
                    title: "marketing.dialogs.associations-delete.title",
                    message: "marketing.dialogs.associations-delete.message",
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
                                title: 'marketing.blades.associations-detail.title-new',
                                subtitle: blade.subtitle,
                                isNew: true,
                                controller: 'virtoCommerce.marketingModule.associationDetailController',
                                template: 'Modules/$(VirtoCommerce.Marketing)/Scripts/association/blades/association-detail.tpl.html'
                            };
                            bladeNavigationService.showBlade(newBlade, blade);
                        });
                    },
                    canExecuteMethod: function () { return true; },
                    permission: 'marketing:create'
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

            // simple and advanced filtering
            var filter = blade.filter = $scope.filter = {};
            $scope.$localStorage = $localStorage;
            if (!$localStorage.associationSearchFilters) {
                $localStorage.associationSearchFilters = [{ name: 'marketing.blades.association-list.new-filter' }];
            }
            if ($localStorage.associationSearchFilterId) {
                filter.current = _.findWhere($localStorage.associationSearchFilters, { id: $localStorage.associationSearchFilterId });
            }

            filter.change = function () {
                $localStorage.associationSearchFilterId = filter.current ? filter.current.id : null;
                if (filter.current && !filter.current.id) {
                    filter.current = null;
                    showFilterDetailBlade({ isNew: true });
                } else {
                    bladeNavigationService.closeBlade({ id: 'filterDetail' });
                    filter.criteriaChanged();
                }
            };

            filter.edit = function () {
                if (filter.current) {
                    showFilterDetailBlade({ data: filter.current });
                }
            };

            function showFilterDetailBlade(bladeData) {
                var newBlade = {
                    id: 'filterDetail',
                    controller: 'virtoCommerce.marketingModule.filterDetailController',
                    template: 'Modules/$(VirtoCommerce.Marketing)/Scripts/association/blades/filter-detail.tpl.html'
                };
                angular.extend(newBlade, bladeData);
                bladeNavigationService.showBlade(newBlade, blade);
            }

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
                    uiGridHelper.bindRefreshOnSortChanged($scope);
                });

                bladeUtils.initializePagination($scope);
            };

            // actions on load
            //No need to call this because page 'pageSettings.currentPage' is watched!!! It would trigger subsequent duplicated req...
            //blade.refresh();
        }]);
