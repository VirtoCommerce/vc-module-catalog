angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.itemAssetController', ['$scope', '$translate', 'platformWebApp.bladeNavigationService', '$filter', 'platformWebApp.uiGridHelper', '$timeout', function ($scope, $translate, bladeNavigationService, $filter, uiGridHelper, $timeout) {
        var blade = $scope.blade;
        blade.headIcon = 'fa fa-chain';

        blade.toolbarCommands = [
            {
                name: 'platform.commands.remove',
                icon: 'fas fa-trash-alt',
                executeMethod: function () { $scope.removeAction(); },
                canExecuteMethod: function () {
                    var retVal = false;
                    if (blade.currentEntities && $scope.gridApi) {
                        retVal = $scope.gridApi.selection.getSelectedRows().length > 0;
                    }
                    return retVal;
                }
            },
            {
                name: "Add",
                icon: 'fas fa-plus',
                executeMethod: function () {
                    var newBlade = {
                        title: "catalog.blades.asset-upload.title",
                        item: blade.item,
                        onSelect: linkAssets,
                        controller: 'virtoCommerce.catalogModule.itemAssetAddController',
                        template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/item-asset-add.tpl.html'
                    };
                    bladeNavigationService.showBlade(newBlade, blade);
                },
                canExecuteMethod: function () { return true; }
            },
            {
                name: "Link",
                icon: 'fas fa-link',
                executeMethod: function () {
                    var newBlade = {
                        title: 'catalog.blades.asset-select.title',
                        //folder: "catalog",
                        onSelect: linkAssets,
                        controller: 'virtoCommerce.assetsModule.assetSelectController'
                    };
                    bladeNavigationService.showBlade(newBlade, blade);
                },
                canExecuteMethod: function () { return true; }
            }];

        function linkAssets(assets) {
            _.each(assets, function (asset) {
                var converted = angular.copy(asset);
                converted.mimeType = asset.contentType;
                blade.currentEntities.push(converted);
            });
        }

        blade.isLoading = false;

        blade.refresh = function (item) {
            initialize(item);
        }

        $scope.isValid = true;

        $scope.saveChanges = function () {
            blade.item.assets = blade.currentEntities;
            $scope.bladeClose();
        };

        function initialize(item) {
            blade.item = item;

            blade.title = item.name;
            blade.subtitle = 'catalog.widgets.itemAsset.blade-subtitle';

            blade.currentEntities = item.assets ? angular.copy(item.assets) : [];
        }

        $scope.toggleAssetSelect = function (e, asset) {
            if (e.ctrlKey === 1) {
                asset.selected = !asset.selected;
            } else {
                if (asset.selected) {
                    asset.selected = false;
                } else {
                    asset.selected = true;
                }
            }
        }

        $scope.removeAction = function (asset) {
            var idx = blade.currentEntities.indexOf(asset);
            if (idx >= 0) {
                blade.currentEntities.splice(idx, 1);
            }
        };

        $scope.copyUrl = function (data) {
            $translate('catalog.blades.item-asset-list.labels.copy-url-prompt').then(function (promptMessage) {
                window.prompt(promptMessage, data.url);
            });
        }


        function getEntityGridIndex(searchEntity, gridApi) {
            var index = -1;
            if (gridApi) {
                _.each(gridApi.grid.renderContainers.body.visibleRowCache, (row, idx) => {
                        if (_.isEqual(row.entity, searchEntity)) {
                            index = idx;
                            return;
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

        $scope.edit = function (entity) {
            if (!entity.catalogId) entity.catalogId = blade.item.catalogId;
            var newBlade = {
                id: 'itemDetailChild',
                origEntity: entity,
                controller: 'virtoCommerce.catalogModule.itemAssetsDetailController',
                template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/item-asset-detail.tpl.html'
            };
            bladeNavigationService.showBlade(newBlade, blade);
        };

        $scope.setGridOptions = function (gridOptions) {
            gridOptions.enableCellEditOnFocus = false;
            uiGridHelper.initialize($scope, gridOptions,
                function (gridApi) {
                    gridApi.edit.on.afterCellEdit($scope, function (rowEntity, colDef) {
                        var index = getEntityGridIndex(rowEntity, gridApi);
                        var data = {
                            rowEntity: rowEntity,
                            colDef: colDef,
                            index: index,
                            gridApi: gridApi
                        };
                        $timeout(priorityChanged, 100, true, data);
                    });
                });
        };


        $scope.removeItem = function (image) {
            var idx = blade.currentEntities.indexOf(image);
            if (idx >= 0) {
                blade.currentEntities.splice(idx, 1);
            }
        };


        $scope.downloadUrl = function (image) {
            window.open(image.url, '_blank');
        }

        $scope.removeAction = function (selectedImages) {
            if (selectedImages == undefined) {
                selectedImages = $scope.gridApi.selection.getSelectedRows();
            }

            angular.forEach(selectedImages, function (image) {
                var idx = blade.currentEntities.indexOf(image);
                if (idx >= 0) {
                    blade.currentEntities.splice(idx, 1);
                }
            });
        };

        initialize(blade.item);

    }]).run(
        ['platformWebApp.ui-grid.extension', 'uiGridValidateService', function (gridOptionExtension, uiGridValidateService) {
            uiGridValidateService.setValidator('minPriorityValidator',
                () => (oldValue, newValue) => newValue >= 0,
                () => 'Priority value should be equal or more than zero');
        }]);
