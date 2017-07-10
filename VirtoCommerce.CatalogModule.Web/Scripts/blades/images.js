angular.module('virtoCommerce.catalogModule')
.controller('virtoCommerce.catalogModule.imagesController',
    ['$scope', '$filter', '$translate', 'platformWebApp.dialogService',
        'platformWebApp.bladeNavigationService', 'platformWebApp.authService',
        'platformWebApp.assets.api', 'virtoCommerce.catalogModule.imageTools', 'platformWebApp.settings', 'platformWebApp.bladeUtils', 'platformWebApp.uiGridHelper',
        function ($scope, $filter, $translate, dialogService, bladeNavigationService, authService, assets, imageTools, settings, bladeUtils, uiGridHelper) {
            var blade = $scope.blade;
            blade.headIcon = 'fa-image';

            blade.hasAssetCreatePermission = bladeNavigationService.checkPermission('platform:asset:create');

            $scope.isValid = true;

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

            $scope.removeAction = function (selectedImages) {
                if (selectedImages == undefined) {
                    selectedImages = $filter('filter')(blade.currentEntities, { $selected: true });
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
                        if (blade.currentEntities) {
                            retVal = _.filter(blade.currentEntities, function (x) { return x.$selected; }).length > 0;
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
                            title: 'platform.blades.asset-seletc.title',
                            folder: "catalog",
                            onSelect: linkAssets,
                            controller: 'platformWebApp.assets.assetSelectController'
                        };
                        bladeNavigationService.showBlade(newBlade, blade);
                    },
                    canExecuteMethod: function () { return true; }
                }
            ];

            function linkAssets(assets) {
                _.each(assets, function (asset) {
                    var image = angular.copy(asset);
                    blade.currentEntities.push(image);
                });
            }

            $scope.sortableOptions = {
                update: function (e, ui) {
                },
                stop: function (e, ui) {
                }
            };

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
            bladeUtils.initializePagination($scope, true);

            initialize(blade.item);

        }]);
