angular.module('virtoCommerce.catalogModule')
.controller('virtoCommerce.catalogModule.imagesAddController',
    ['$scope', '$filter', '$translate', 'FileUploader', 'platformWebApp.dialogService',
        'platformWebApp.bladeNavigationService', 'platformWebApp.authService',
        'platformWebApp.assets.api', 'virtoCommerce.catalogModule.imageTools', 'platformWebApp.settings',
        function ($scope, $filter, $translate, FileUploader, dialogService, bladeNavigationService, authService, assets, imageTools, settings) {
            var blade = $scope.blade;
            blade.hasAssetCreatePermission = bladeNavigationService.checkPermission('platform:asset:create');

            $scope.isValid = true;

            blade.isLoading = false;

            blade.refresh = function (item) {
                initialize(item);
            }

            function initialize(item) {
                blade.item = item;
                blade.title = 'catalog.widgets.itemImage.blade-title';
                blade.subtitle = 'catalog.widgets.itemImage.blade-subtitle';
                $scope.imageTypes = settings.getValues({ id: 'Catalog.ImageCategories' });

                if (!$scope.uploader && blade.hasAssetCreatePermission) {

                    // create the uploader            
                    var uploader = $scope.uploader = new FileUploader({
                        scope: $scope,
                        headers: { Accept: 'application/json' },
                        autoUpload: true,
                        removeAfterUpload: true
                    });

                    uploader.url = getImageUrl(item.code, blade.imageType).relative;

                    uploader.onSuccessItem = function (fileItem, images, status, headers) {
                        angular.forEach(images, function (image) {
                            //ADD uploaded image                
                            blade.currentEntities.push(image);
                            var request = { imageUrl: image.url, isRegenerateAll: true };

                            imageTools.generateThumbnails(request, function (response) {
                                if (!response || response.error) {
                                    bladeNavigationService.setError(response.error, blade);
                                }
                            });
                        });
                    };

                    uploader.onAfterAddingAll = function (addedItems) {
                        bladeNavigationService.setError(null, blade);
                    };

                    uploader.onErrorItem = function (item, response, status, headers) {
                        bladeNavigationService.setError(item._file.name + ' failed: ' + (response.message ? response.message : status), blade);
                    };
                }
                blade.currentEntities = item.images ? angular.copy(item.images) : [];
            };

            $scope.addImageFromUrl = function () {
                if (blade.newExternalImageUrl) {
                    assets.uploadFromUrl({ folderUrl: getImageUrl(blade.item.code, blade.imageType).folderUrl, url: blade.newExternalImageUrl }, function (data) {
                        _.each(data, function (x) {
                            blade.currentEntities.push(x);
                        });
                        blade.newExternalImageUrl = undefined;
                    });
                }
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

            blade.headIcon = 'fa-image';
         
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

            $scope.changeImageCategory = function ($item, $model) {
                $scope.uploader.url = getImageUrl(blade.item.code, blade.imageType).relative;
            };

            function getImageUrl(code, imageType) {
                var folderUrl = 'catalog/' + code + (imageType ? '/' + imageType : '');
                return { folderUrl: '/' + folderUrl, relative: 'api/platform/assets?folderUrl=' + folderUrl };
            };


            initialize(blade.item);

        }]);
