angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.videoListController',
        ['$scope', '$translate', '$timeout', 'platformWebApp.bladeNavigationService', 'platformWebApp.bladeUtils', 'platformWebApp.dialogService', 'platformWebApp.uiGridHelper', 'virtoCommerce.catalogModule.videos', 'platformWebApp.settings',
            function ($scope, $translate, $timeout, bladeNavigationService, bladeUtils, dialogService, uiGridHelper, videos, settings) {
                var blade = $scope.blade;
                blade.updatePermission = 'catalog:update';
                blade.headIcon = 'fab fa-youtube';
                blade.title = 'catalog.blades.video-list.title';
                blade.subtitle = 'catalog.blades.video-list.subtitle';

                blade.getSearchCriteria = function () {
                    return angular.extend(filter, {
                        searchPhrase: filter.keyword ? filter.keyword : undefined,
                        sort: uiGridHelper.getSortExpression($scope),
                        skip: ($scope.pageSettings.currentPage - 1) * $scope.pageSettings.itemsPerPageCount,
                        take: $scope.pageSettings.itemsPerPageCount
                    });
                };

                blade.isCopyIdMenuVisible = false;
                settings.getValues({ id: 'Catalog.AllowToCopyID' }, function (data) {
                    if (data && data.length > 0) {
                        blade.isCopyIdMenuVisible = data[0];
                    }
                });

                blade.refresh = function () {
                    blade.isLoading = true;
                    videos.options(null,
                        function (data) {
                            blade.videoOptions = data;
                        },
                        function (error) {
                            bladeNavigationService.setError(error, blade);
                        });
                    videos.search(blade.getSearchCriteria(),
                        function (data) {
                            blade.isLoading = false;
                            $scope.pageSettings.totalItems = data.totalCount;
                            blade.currentEntities = data.results;
                        },
                        function (error) {
                            bladeNavigationService.setError(error, blade);
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

                $scope.copyItemID = function (data) {
                    navigator.clipboard.writeText(data.id).then().catch(e => console.error(e));
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
                }

                $scope.delete = function (item) {
                    deleteList([item], true);
                };

                function deleteList (selection, single) {
                    var dialog = {
                        id: "confirmDelete",
                        title: single ? "catalog.dialogs.video-delete.title" : "catalog.dialogs.video-list-delete.title",
                        message: single ? "catalog.dialogs.video-delete.message" : "catalog.dialogs.video-list-delete.message",
                        callback: function (remove) {
                            if (remove) {
                                blade.isLoading = true;
                                videos.remove({ ids: selection.map(item => item.id) },
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
                        permission: blade.updatePermission,
                        executeMethod: function() { deleteList($scope.gridApi.selection.getSelectedRows()); },
                        canExecuteMethod: function() {
                            return blade.currentEntities && $scope.gridApi && _.any($scope.gridApi.selection.getSelectedRows());
                        }
                    },
                ];

                var priorityChanged = function(data) {
                    blade.isLoading = true;
                    videos.save([data.rowEntity],
                        function (resp) {
                            blade.refresh();
                        },
                        function (error) {
                            bladeNavigationService.setError(error, blade);
                        });
                };

                // ui-grid
                $scope.setGridOptions = function (gridOptions) {
                    uiGridHelper.initialize($scope, gridOptions, function (gridApi) {
                        gridApi.edit.on.afterCellEdit($scope,
                            function(rowEntity, colDef, newValue, oldValue) {
                                if (newValue !== oldValue) {
                                    var data = {
                                        rowEntity: rowEntity,
                                        colDef: colDef,
                                        gridApi: gridApi
                                    };
                                    $timeout(priorityChanged, 100, true, data);
                                }
                            });
                        uiGridHelper.bindRefreshOnSortChanged($scope);
                    });
                    bladeUtils.initializePagination($scope);
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
            }])
    .run(['platformWebApp.ui-grid.extension', 'uiGridValidateService', function(uiGridExtension, uiGridValidateService) {
            uiGridValidateService.setValidator('minPriorityValidator',
                () => (oldValue, newValue) => newValue >= 0,
                () => 'Priority value should be equal or more than zero');
        }
    ]);
