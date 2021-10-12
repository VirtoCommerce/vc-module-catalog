angular.module('virtoCommerce.catalogModule')
.controller('virtoCommerce.catalogModule.categoryDescriptionDetailController', ['$scope', 'platformWebApp.bladeNavigationService', 'FileUploader', 'platformWebApp.settings', '$timeout',
    function ($scope, bladeNavigationService, FileUploader, settings, $timeout) {
        var blade = $scope.blade;

        function initilize() {
            if (!blade.category.descriptions) {
                blade.category.descriptions = [];
            }
            if (!blade.currentEntity) {
                blade.currentEntity = {
                    languageCode: blade.catalog.defaultLanguage.languageCode
                };
            }

            blade.origEntity = blade.currentEntity;
            blade.currentEntity = angular.copy(blade.currentEntity);

            $timeout(function () {
                $scope.$broadcast('resetContent', { body: blade.currentEntity.content });
                blade.isLoading = false;
            });
        }

        $scope.isValid = true;

        $scope.saveChanges = function () {
            var existDescription = _.find(blade.category.descriptions, function (x) { return x == blade.origEntity; });
            if (!existDescription) {
                blade.category.descriptions.push(blade.origEntity);
            }
            angular.copy(blade.currentEntity, blade.origEntity);
            $scope.bladeClose();
        };

        blade.headIcon = 'fa fa-comments';
        blade.title = 'catalog.blades.categoryDescription-detail.title';
        blade.subtitle = 'catalog.blades.categoryDescription-detail.subtitle';
        blade.editAsMarkdown = true;
        blade.hasAssetCreatePermission = bladeNavigationService.checkPermission('platform:asset:create');

        if (blade.hasAssetCreatePermission) {
            $scope.fileUploader = new FileUploader({
                url: 'api/assets?folderUrl=catalog/' + blade.category.code,
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

        settings.getValues({ id: 'Catalog.CategoryDescriptionTypes' }, function (data) {
            $scope.types = data;
            if (!blade.currentEntity.descriptionType) {
                blade.currentEntity.descriptionType = $scope.types[0];
            }
        });

        $scope.openDictionarySettingManagement = function () {
            var newBlade = {
                id: 'settingDetailChild',
                isApiSave: true,
                currentEntityId: 'Catalog.CategoryDescriptionTypes',
                parentRefresh: function (data) { $scope.types = data; },
                controller: 'platformWebApp.settingDictionaryController',
                template: '$(Platform)/Scripts/app/settings/blades/setting-dictionary.tpl.html'
            };
            bladeNavigationService.showBlade(newBlade, blade);
        };

        var formScope;
        $scope.setForm = function (form) { formScope = form; }

        blade.toolbarCommands = [
            {
                name: "platform.commands.save", icon: 'fas fa-save',
                executeMethod: saveChanges,
                canExecuteMethod: canSave
            },
            {
                name: "platform.commands.reset", icon: 'fa fa-undo',
                executeMethod: function () {
                    angular.copy(blade.origEntity, blade.currentEntity);
                    $scope.$broadcast('resetContent', { body: blade.currentEntity.content });
                },
                canExecuteMethod: isDirty
            }
        ];

        function saveChanges() {
            var existDescription = _.find(blade.category.descriptions, function (x) { return x === blade.origEntity; });
            if (!existDescription) {
                blade.category.descriptions.push(blade.origEntity);
            }
            angular.copy(blade.currentEntity, blade.origEntity);
        }

        function isDirty() {
            return !angular.equals(blade.currentEntity, blade.origEntity);
        }

        function canSave() {
            return isDirty() && formScope && formScope.$valid;
        }

        blade.onClose = function (closeCallback) {
            bladeNavigationService.showConfirmationIfNeeded(isDirty(), canSave(), blade, saveChanges, closeCallback, "catalog.dialogs.review-save.title", "catalog.dialogs.review-save.message");
        };

        initilize();
    }]);
