angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.imagesAddController',
        ['$scope', '$translate', 'FileUploader', 'platformWebApp.bladeNavigationService', 'platformWebApp.settings',
            function ($scope, $translate, FileUploader,  bladeNavigationService, settings) {
                var blade = $scope.blade;

                blade.hasAssetCreatePermission = bladeNavigationService.checkPermission('platform:asset:create');

                blade.headIcon = 'fa fa-image';

                $scope.isValid = false;

                blade.isLoading = false;

                blade.refresh = function (item) {
                    initialize(item);
                }

                var promise = settings.getValues({ id: 'VirtoCommerce.Core.General.Languages' }).$promise;

                $scope.languages = [];
                function initialize(item) {
                    promise.then(function (promiseData) {
                        $scope.languages = promiseData;
                    });

                    blade.item = item;
                    blade.title = 'catalog.blades.image-upload.title';
                    $scope.imageTypes = settings.getValues({ id: 'Catalog.ImageCategories' });

                    if (!$scope.uploader && blade.hasAssetCreatePermission) {

                        // create the uploader            
                        var uploader = $scope.uploader = new FileUploader({
                            scope: $scope,
                            headers: { Accept: 'application/json' },
                            autoUpload: true,
                            removeAfterUpload: true
                        });

                        uploader.url = getImageUrl(blade.folderPath, blade.imageType).relative;

                        uploader.onSuccessItem = function (fileItem, images, status, headers) {
                            angular.forEach(images, function (image) {
                                //ADD uploaded image
                                image.isImage = true;
                                image.group = blade.imageType;
                                blade.currentEntities.push(image);
                            });

                            $scope.isValid = true;
                        };

                        uploader.onAfterAddingAll = function (addedItems) {
                            bladeNavigationService.setError(null, blade);
                        };

                        uploader.onErrorItem = function (element, response, status, headers) {
                            $scope.isValid = false;
                            bladeNavigationService.setError(element._file.name + ' failed: ' + (response.message ? response.message : status), blade);
                        };
                    }
                    blade.currentEntities = [];
                }

                $scope.saveChanges = function () {
                    if (blade.onSelect) {
                        _.each(blade.currentEntities, function (entity) {
                            entity.languageCode = blade.selectedLanguageCode;
                        });
                        blade.onSelect(blade.currentEntities);
                    }

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

                $scope.copyUrl = function (data) {
                    $translate('catalog.blades.images.labels.copy-url-prompt').then(function (promptMessage) {
                        window.prompt(promptMessage, data.url);
                    });
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

                $scope.changeImageCategory = function ($item, $model) {
                    $scope.uploader.url = getImageUrl(blade.folderPath, blade.imageType).relative;
                };

                function getImageUrl(path, imageType) {
                    var folderUrl = 'catalog/' + (path + (imageType ? '/' + imageType : ''));
                    return { folderUrl: folderUrl, relative: 'api/assets?folderUrl=' + folderUrl };
                }

                initialize(blade.item);

            }]);
