angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.editorialReviewDetailController', ['$scope', 'platformWebApp.bladeNavigationService', 'FileUploader', 'platformWebApp.settings', '$timeout',
        function ($scope, bladeNavigationService, FileUploader, settings, $timeout) {
            var blade = $scope.blade;

            function initilize() {
                if (!blade.item.reviews) {
                    blade.item.reviews = [];
                };
                if (!blade.currentEntity) {
                    blade.currentEntity = {};
                };

                blade.origEntity = blade.currentEntity;
                blade.currentEntity = angular.copy(blade.currentEntity);
                if (!blade.currentEntity.languageCode) {
                    blade.currentEntity.languageCode = blade.catalog.defaultLanguage.languageCode;
                }

                $timeout(function () {
                    $scope.$broadcast('resetContent', { body: blade.currentEntity.content });
                    blade.isLoading = false;
                });
            }

            $scope.saveChanges = function () {
                
                var isValid = $scope.isValid();
                if (isValid) {
                    if (!blade.currentEntity.id)
                        blade.item.reviews.push(blade.currentEntity);
                    else {
                        _.extend(_.findWhere(blade.item.reviews, { id: blade.currentEntity.id }), blade.currentEntity);
                    }
                    angular.copy(blade.currentEntity, blade.origEntity);
                    $scope.bladeClose();
                }
            }

            $scope.isValid = function () {
                var result = formScope && formScope.$valid;

                //Check  duplicates for new editorial  reviews
                result = !_.any(blade.item.reviews, function (x) {
                    return (x.id !== blade.currentEntity.id || (!x.id && !blade.currentEntity.id)) // 1st check is for edit mode, 2nd is for adding to prevent multiple same descs to be added
                        && x.reviewType === blade.currentEntity.reviewType && x.languageCode === blade.currentEntity.languageCode;
                });

                return result;
            }

            blade.headIcon = 'fa-comments';
            blade.title = 'catalog.blades.editorialReview-detail.title';
            blade.subtitle = 'catalog.blades.editorialReview-detail.subtitle';
            blade.editAsMarkdown = true;
            blade.hasAssetCreatePermission = bladeNavigationService.checkPermission('platform:asset:create');

            if (blade.hasAssetCreatePermission) {
                $scope.fileUploader = new FileUploader({
                    url: 'api/platform/assets?folderUrl=catalog/' + blade.item.code,
                    headers: { Accept: 'application/json' },
                    autoUpload: true,
                    removeAfterUpload: true,
                    onBeforeUploadItem: function (fileItem) {
                        blade.isLoading = true;
                    },
                    onSuccessItem: function (fileItem, response) {
                        $scope.$broadcast('filesUploaded', { items: response });
                    },
                    onErrorItem: function (fileItem, response, status) {
                        bladeNavigationService.setError(fileItem._file.name + ' failed: ' + (response.message ? response.message : status), blade);
                    },
                    onCompleteAll: function () {
                        blade.isLoading = false;
                    }
                });
            }

            settings.getValues({ id: 'Catalog.EditorialReviewTypes' }, function (data) {
                $scope.types = data;
                if (!blade.currentEntity.reviewType) {
                    blade.currentEntity.reviewType = $scope.types[0];
                }
            });

            $scope.openDictionarySettingManagement = function () {
                var newBlade = new DictionarySettingDetailBlade('Catalog.EditorialReviewTypes');
                newBlade.parentRefresh = function (data) {
                    $scope.types = data;
                };
                bladeNavigationService.showBlade(newBlade, blade);
            };

            var formScope;
            $scope.setForm = function (form) { formScope = form; }

            function isDirty() {
                return !angular.equals(blade.currentEntity, blade.origEntity);
            };

            initilize();
        }]);
