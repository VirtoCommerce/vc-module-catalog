angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.brandSettingDetailsController',
        ['$scope', '$q', '$timeout', 'platformWebApp.bladeNavigationService', 'virtoCommerce.catalogModule.brandSettings', 'virtoCommerce.catalogModule.catalogs', 'virtoCommerce.catalogModule.aggregationProperties',
            function ($scope, $q, $timeout, bladeNavigationService, brandSettings, catalogs, aggregationProperties) {
                const blade = $scope.blade;
                blade.title = 'catalog.blades.brand-setting-details.title';
                blade.updatePermission = 'brands:update';
                blade.properties = [];
                $scope.brandSettingsLoaded = false;

                blade.refresh = function () {
                    blade.isLoading = true;

                    if (blade.store) {
                        var getStorePromise = brandSettings.getByStore({ storeId: blade.store.id }).$promise.then(function (data) {
                            return data;
                        });
                        var getPropertiesPromise = aggregationProperties.getProperties({ storeId: blade.store.id }).$promise.then(function (data) {
                            return data;
                        });

                        $q.all([
                            getStorePromise,
                            getPropertiesPromise,
                        ]).then(function (results) {
                            if (results) {
                                var data = results[0];
                                var properties = results[1];
                                if (data.brandCatalogId) {
                                    blade.catalog = catalogs.get({ id: data.brandCatalogId }, function (catalog) {
                                        //data.selectedCatalog = catalog;
                                        initialize(data, properties, catalog);
                                    });
                                }
                                else {
                                    initialize(data, properties);
                                }
                            }
                        });
                    }
                };

                function initialize(data, properties, catalog) {
                    blade.currentEntity = angular.copy(data);
                    blade.originalEntity = data;

                    blade.properties = properties;

                    if (catalog) {
                        blade.selectedCatalog = angular.copy(catalog);
                        blade.originalCatalog = catalog;
                    }

                    $timeout(resetBrandSettings, 0);
                    blade.isLoading = false;
                }

                blade.saveChanges = function () {
                    blade.isLoading = true;

                    let saveTasks = [];

                    if (selectedCatalogSeoIsDirty()) {
                        var updateCatalogPromise = catalogs.update(blade.selectedCatalog).$promise.then(function (data) {
                            return data;
                        });

                        saveTasks.push(updateCatalogPromise);
                    }

                    if (currentEntityIsDirty()) {
                        var updateSettingsPromise = brandSettings.updateSetting(blade.currentEntity).$promise.then(function () {
                            return undefined;
                        });
                        saveTasks.push(updateSettingsPromise);
                    }

                    $q.all(saveTasks).then(function () {
                        closeChildrenBlades();
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
                    return (currentEntityIsDirty() || selectedCatalogSeoIsDirty()) && blade.hasUpdatePermission();
                }

                function currentEntityIsDirty() {
                    return !angular.equals(blade.currentEntity, blade.originalEntity);
                }

                function selectedCatalogSeoIsDirty() {
                    return blade.selectedCatalog && blade.originalCatalog && !angular.equals(blade.selectedCatalog.seoInfos, blade.originalCatalog.seoInfos);
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

                blade.onSelectCatalog = function (item) {
                    closeChildrenBlades();
                    blade.selectedCatalog = item;
                }

                blade.onRemoveCatalog = function () {
                    closeChildrenBlades();
                    blade.selectedCatalog = undefined;
                }

                function resetBrandSettings() {
                    $scope.brandSettingsLoaded = true;
                }

                function closeChildrenBlades() {
                    angular.forEach(blade.childrenBlades.slice(), function (child) {
                        bladeNavigationService.closeBlade(child);
                    });
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
