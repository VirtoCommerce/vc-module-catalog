angular.module('virtoCommerce.catalogModule')
.controller('virtoCommerce.catalogModule.itemAssetController', ['$scope', '$translate', 'platformWebApp.bladeNavigationService', '$filter', 'FileUploader', function ($scope, $translate, bladeNavigationService, $filter, FileUploader) {
    var blade = $scope.blade;

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

        if (!$scope.uploader && blade.hasUpdatePermission()) {
            // create the uploader
            var uploader = $scope.uploader = new FileUploader({
                scope: $scope,
                headers: { Accept: 'application/json' },
                method: 'POST',
                autoUpload: true,
                removeAfterUpload: true
            });

           	uploader.url = 'api/platform/assets?folderUrl=catalog/' + item.code;

            uploader.onSuccessItem = function (fileItem, assets, status, headers) {
                angular.forEach(assets, function (asset) {
                    asset.itemId = blade.item.id;
                    //ADD uploaded asset to the item
                    blade.currentEntities.push(asset);
                });
            };

            uploader.onAfterAddingAll = function (addedItems) {
                bladeNavigationService.setError(null, blade);
            };

            uploader.onErrorItem = function (item, response, status, headers) {
                bladeNavigationService.setError(item._file.name + ' failed: ' + (response.message ? response.message : status), blade);
            };
        }
        blade.currentEntities = item.assets ? angular.copy(item.assets) : [];
    };

    $scope.toggleAssetSelect = function (e, asset) {
        if (e.ctrlKey == 1) {
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
        $translate('catalog.blades.item-asset-detail.labels.copy-url-prompt').then(function (promptMessage) {
            window.prompt(promptMessage, data.url);
        });
    }

    blade.headIcon = 'fa-chain';
    blade.toolbarCommands = [];

    initialize(blade.item);

}]);
