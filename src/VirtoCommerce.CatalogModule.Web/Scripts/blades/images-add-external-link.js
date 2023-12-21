angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.imagesAddExternalLinkController',
        ['$scope', '$translate', 'platformWebApp.bladeNavigationService', 'platformWebApp.settings',
            function ($scope, $translate, bladeNavigationService, settings) {
                var blade = $scope.blade;

                blade.headIcon = 'fa fa-external-link';

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
                    blade.title = 'catalog.blades.image-add-external-link.title';
                    $scope.imageTypes = settings.getValues({ id: 'Catalog.ImageCategories' });

                    blade.currentEntities = [];
                }

                $scope.addImageFromUrlHandler = function () {
                    $scope.isValid = true;

                    var image = {
                        isImage: true,
                        group: blade.imageType,
                        url: blade.newExternalImageUrl,
                        name: blade.newExternalImageUrl.split('/').pop(),
                        relativeUrl: blade.newExternalImageUrl
                    };
                    blade.currentEntities.push(image);
                };

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

                initialize(blade.item);

            }]);
