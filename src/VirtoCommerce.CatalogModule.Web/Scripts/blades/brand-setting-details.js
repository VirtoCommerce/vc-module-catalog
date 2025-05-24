angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.brandSettingDetailsController',
        ['$scope', '$q', '$timeout', 'virtoCommerce.catalogModule.brandSettings', 'virtoCommerce.catalogModule.catalogs', 'virtoCommerce.catalogModule.aggregationProperties',
            function ($scope, $q, $timeout, brandSettings, catalogs, aggregationProperties) {
                const blade = $scope.blade;
                blade.title = 'catalog.blades.brand-setting-details.title';
                blade.updatePermission = 'brands:update';
                blade.properties = [];
                $scope.brandSettingsLoaded = false;

                blade.refresh = function () {
                    blade.isLoading = true;

                    if (blade.store) {
                        var getStorePromise = brandSettings.getByStore({ storeId: blade.store.id }).$promise.then(function (data) {
                            return data
                        });
                        var getPropertiesPromise = aggregationProperties.getProperties({ storeId: blade.store.id }).$promise.then(function (data) {
                            return data
                        });

                        $q.all([
                            getStorePromise,
                            getPropertiesPromise,
                        ]).then(function (results) {
                            if (results) {
                                initialize(results[0]);

                                blade.properties = results[1];

                                $timeout(resetBrandSettings, 0);
                                blade.isLoading = false;
                            }
                        });
                    }
                };

                function initialize(data) {
                    blade.currentEntity = angular.copy(data);
                    blade.originalEntity = data;
                }

                blade.saveChanges = function () {
                    blade.isLoading = true;

                    brandSettings.updateSetting(blade.currentEntity,
                        function () {
                            blade.isNew = false;
                            blade.refresh();
                        });
                };

                $scope.setForm = function (form) {
                    $scope.formScope = form;
                };

                function canSave() {
                    return isDirty() && $scope.formScope && $scope.formScope.$valid;
                }

                function isDirty() {
                    return !angular.equals(blade.currentEntity, blade.originalEntity) && blade.hasUpdatePermission();
                }

                blade.fetchCatalogs = function (criteria) {
                    criteria.storeId = blade.store.catalogId;
                    return catalogs.search(criteria);
                }

                blade.fetchProperties = function () {
                    return _.map(blade.properties, function (x) {
                        return {
                            id: x.name,
                            name: x.name,
                        }
                    });
                }

                function resetBrandSettings() {
                    $scope.brandSettingsLoaded = true;
                }

                blade.toolbarCommands = [
                    {
                        name: "platform.commands.save",
                        icon: 'fas fa-save',
                        executeMethod: blade.saveChanges,
                        canExecuteMethod: canSave,
                        permission: blade.updatePermission
                    }];

                blade.refresh();
            }]);
