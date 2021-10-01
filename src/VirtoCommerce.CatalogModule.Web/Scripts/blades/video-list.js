angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.videoListController',
        ['$scope', '$translate', '$timeout', 'platformWebApp.bladeNavigationService', 'platformWebApp.bladeUtils', 'platformWebApp.uiGridHelper', 'virtoCommerce.catalogModule.videos',
            function ($scope, $translate, $timeout, bladeNavigationService, bladeUtils, uiGridHelper, videos) {
                var blade = $scope.blade;
                blade.headIcon = 'fab fa-youtube';
                blade.title = 'catalog.blades.video-list.title';
                blade.subtitle = 'catalog.blades.video-list.subtitle';
                var languages = blade.catalog.languages;

                blade.getSearchCriteria = function () {
                    return angular.extend(filter, {
                        searchPhrase: filter.keyword ? filter.keyword : undefined,
                        sort: uiGridHelper.getSortExpression($scope),
                        skip: ($scope.pageSettings.currentPage - 1) * $scope.pageSettings.itemsPerPageCount,
                        take: $scope.pageSettings.itemsPerPageCount
                    });
                };

                blade.refresh = function () {
                    blade.isLoading = true;
                    videos.options(null,
                        function (data) {
                            blade.videoOptions = data;
                        },
                        function (error) {
                            bladeNavigationService.setError('Error ' + error.status, blade);
                        });
                    videos.search(blade.getSearchCriteria(),
                        function (data) {
                            blade.isLoading = false;
                            $scope.pageSettings.totalItems = data.totalCount;
                            blade.currentEntities = data.results;
                        },
                        function (error) {
                            bladeNavigationService.setError('Error ' + error.status, blade);
                        });
                };

                // simple and advanced filtering
                var filter = $scope.filter = blade.filter || {
                    ownerIds: [blade.owner.id],
                    ownerType: blade.ownerType
                };

                filter.criteriaChanged = function () {
                    if ($scope.pageSettings.currentPage > 1) {
                        $scope.pageSettings.currentPage = 1;
                    } else {
                        blade.refresh();
                    }
                };

                $scope.toggleImageSelect = function(e, item) {
                    if (e.ctrlKey === 1) {
                        item.$selected = !item.$selected;
                    } else {
                        if (item.$selected) {
                            item.$selected = false;
                        } else {
                            item.$selected = true;
                        }
                    }
                };

                $scope.edit = function(item) {
                    var newBlade = {
                        id: 'videoDetail',
                        currentEntityId: item.id,
                        currentEntity: item,
                        controller: 'virtoCommerce.catalogModule.videoDetailController',
                        template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/video-detail.tpl.html'
                    };
                    bladeNavigationService.showBlade(newBlade, blade);
                };

                blade.selectNode = function(item) {
                    $scope.selectedNodeId = item.id;

                    var newBlade = {
                        id: 'videoDetail',
                        currentEntityId: item.id,
                        currentEntity: item,
                        controller: 'virtoCommerce.catalogModule.videoDetailController',
                        template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/video-detail.tpl.html'
                    };
                    bladeNavigationService.showBlade(newBlade, blade);
                }

                function addVideo() {
                    var newBlade = {
                        id: 'videoAdd',
                        owner: blade.owner,
                        ownerType: blade.ownerType,
                        videoOptions: blade.videoOptions,
                        defaultLanguage: blade.catalog.defaultLanguage,
                        controller: 'virtoCommerce.catalogModule.videoAddController',
                        template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/video-add.tpl.html'
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
                    },
                    {
                        name: "platform.commands.add",
                        icon: 'fa fa-plus',
                        permission: blade.updatePermission,
                        executeMethod: function() {
                            addVideo();
                        },
                        canExecuteMethod: function () {
                            return blade.owner && blade.ownerType;
                        }
                    },
                    {
                        name: "platform.commands.delete",
                        icon: 'fa fa-trash-o',
                        executeMethod: function() { $scope.removeAction(); },
                        canExecuteMethod: function() {
                            return blade.currentEntities && $scope.gridApi && $scope.gridApi.selection.getSelectedRows().length > 0;
                        }
                    },
                ];

                function getEntityGridIndex(item, gridApi) {
                    var index = -1;
                    if (gridApi) {
                        _.each(gridApi.grid.renderContainers.body.visibleRowCache,
                            function(row, idx) {
                                if (_.isEqual(row.entity, item)) {
                                    index = idx;
                                }
                            });
                    }
                    return index;
                }

                var priorityChanged = function(data) {
                    var newIndex = getEntityGridIndex(data.rowEntity, data.gridApi);
                    if (newIndex !== data.index) {
                        data.gridApi.cellNav.scrollToFocus(data.rowEntity, data.colDef);
                    }
                };

                // ui-grid
                $scope.setGridOptions = function (gridOptions) {
                    gridOptions.enableCellEditOnFocus = false;
                    uiGridHelper.initialize($scope, gridOptions, function (gridApi) {
                        gridApi.edit.on.afterCellEdit($scope,
                            function(rowEntity, colDef) {
                                var index = getEntityGridIndex(rowEntity, gridApi);
                                var data = {
                                    rowEntity: rowEntity,
                                    colDef: colDef,
                                    index: index,
                                    gridApi: gridApi
                                };
                                $timeout(priorityChanged, 100, true, data);
                            });
                        uiGridHelper.bindRefreshOnSortChanged($scope);
                    });
                };

                $scope.priorityValid = function(entity) {
                    return !_.isUndefined(entity.sortOrder) && entity.sortOrder >= 0;
                };

                $scope.isValid = true;

                $scope.$watch("blade.currentEntities",
                    function(data) {
                        var result = _.all(blade.currentEntities, $scope.priorityValid);
                        return $scope.isValid = result;
                    },
                    true);

                bladeUtils.initializePagination($scope, true);

                blade.refresh();
            }])
    .run(['platformWebApp.ui-grid.extension', 'uiGridValidateService', function(uiGridExtension, uiGridValidateService) {
            uiGridValidateService.setValidator('minPriorityValidator',
                function() {
                    return function(oldValue, newValue, rowEntity, colDef) {
                        return newValue >= 0;
                    };
                },
                function() { return 'Priority value should be equal or more than zero'; });
        }
    ]);
