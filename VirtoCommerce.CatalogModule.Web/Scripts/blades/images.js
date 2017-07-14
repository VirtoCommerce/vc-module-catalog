angular.module('virtoCommerce.catalogModule')
.controller('virtoCommerce.catalogModule.imagesController',
    ['$scope', '$filter', '$translate', 'platformWebApp.dialogService',
        'platformWebApp.bladeNavigationService', 'platformWebApp.authService',
        'platformWebApp.assets.api', 'virtoCommerce.catalogModule.imageTools', 'platformWebApp.settings', 'platformWebApp.bladeUtils', 'platformWebApp.uiGridHelper',
        function ($scope, $filter, $translate, dialogService, bladeNavigationService, authService, assets, imageTools, settings, bladeUtils, uiGridHelper) {
            var blade = $scope.blade;
            blade.headIcon = 'fa-image';

            blade.hasAssetCreatePermission = bladeNavigationService.checkPermission('platform:asset:create');

            blade.isLoading = false;

            blade.refresh = function (item) {
                initialize(item);
            }

            function initialize(item) {
                blade.item = item;
                blade.title = item.name;
                blade.subtitle = 'catalog.widgets.itemImage.blade-subtitle';
                $scope.imageTypes = settings.getValues({ id: 'Catalog.ImageCategories' });

                blade.currentEntities = item.images ? angular.copy(item.images) : [];
            };

            $scope.saveChanges = function () {
                blade.item.images = blade.currentEntities;
                $scope.bladeClose();
            };

            $scope.toggleImageSelect = function (e, image) {
                if (e.ctrlKey == 1) {
                    image.$selected = !image.$selected;
                } else {
                    if (image.$selected) {
                        image.$selected = false;
                    } else {
                        image.$selected = true;
                    }
                }
            }

            $scope.removeItem = function (image) {
                var idx = blade.currentEntities.indexOf(image);
                if (idx >= 0) {
                    blade.currentEntities.splice(idx, 1);
                }
            };

            $scope.copyUrl = function (data) {
                $translate('catalog.blades.images.labels.copy-url-prompt').then(function (promptMessage) {
                    window.prompt(promptMessage, data.url);
                });
            }

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

            blade.toolbarCommands = [
                {
                    name: 'platform.commands.remove',
                    icon: 'fa fa-trash-o',
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
                    name: 'catalog.commands.gallery',
                    icon: 'fa fa-image',
                    executeMethod: function () {
                        var dialog = {
                            images: blade.currentEntities,
                            currentImage: blade.currentEntities[0]
                        };
                        dialogService.showGalleryDialog(dialog);
                    },
                    canExecuteMethod: function () {
                        return blade.currentEntities && _.any(blade.currentEntities);
                    }
                },
                {
                    name: "Add",
                    icon: 'fa fa-plus',
                    executeMethod: function () {
                        var newBlade = {
                            item: blade.item,
                            onSelect: linkAssets,
                            controller: 'virtoCommerce.catalogModule.imagesAddController',
                            template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/images-add.tpl.html'
                        };
                        bladeNavigationService.showBlade(newBlade, blade);
                    },
                    canExecuteMethod: function () { return true; }
                },
                {
                    name: "Link",
                    icon: 'fa fa-link',
                    executeMethod: function () {
                        var newBlade = {
                            title: 'catalog.blades.asset-select.title',
                            //folder: "catalog",
                            onSelect: linkAssets,
                            controller: 'platformWebApp.assets.assetSelectController'
                        };
                        bladeNavigationService.showBlade(newBlade, blade);
                    },
                    canExecuteMethod: function () { return true; }
                }
            ];

            function linkAssets(assets) {
                var maxEntity = _.max(blade.currentEntities, function (entity) { return entity.sortOrder; });
                var max = maxEntity.sortOrder;
                _.each(assets, function (asset) {
                    if (asset.isImage) {
                        max++;
                        var image = angular.copy(asset);
                        image.sortOrder = max;
                        blade.currentEntities.push(image);
                    }
                }, max);
            }

            $scope.openDictionarySettingManagement = function () {
                var newBlade = {
                    id: 'settingDetailChild',
                    isApiSave: true,
                    currentEntityId: 'Catalog.ImageCategories',
                    parentRefresh: function (data) { $scope.imageTypes = data; },
                    controller: 'platformWebApp.settingDictionaryController',
                    template: '$(Platform)/Scripts/app/settings/blades/setting-dictionary.tpl.html'
                };
                bladeNavigationService.showBlade(newBlade, blade);
            };

            $scope.setGridOptions = function (gridOptions) {
                uiGridHelper.initialize($scope, gridOptions,
                    function (gridApi) {
                        $scope.$watch('pageSettings.currentPage', gridApi.pagination.seek);
                    });
            };

            $scope.priorityValid = function(entity) {
                return !_.isUndefined(entity.sortOrder) && entity.sortOrder >= 0;
            };

            $scope.isValid = true;

            $scope.$watch("blade.currentEntities", function (data) {
                var result = _.all(blade.currentEntities, $scope.priorityValid);
                return $scope.isValid = result;;
            }, true);

            bladeUtils.initializePagination($scope, true);

            initialize(blade.item);

        }]);
