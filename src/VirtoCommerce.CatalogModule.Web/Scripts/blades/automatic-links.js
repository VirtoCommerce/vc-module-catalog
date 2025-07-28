angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.automaticLinksController', [
        '$scope',
        'platformWebApp.dialogService',
        'platformWebApp.bladeNavigationService',
        'virtoCommerce.catalogModule.catalogs',
        'virtoCommerce.catalogModule.listEntries',
        'virtoCommerce.catalogModule.categories',
        'virtoCommerce.catalogModule.automaticLinkQueryApi',
        function ($scope, dialogService, bladeNavigationService, catalogs, listEntries, categories, automaticLinkQueryApi) {
            var blade = $scope.blade;
            blade.headIcon = 'fa fa-link';
            blade.title = 'catalog.blades.automatic-links.title';

            var formScope;
            $scope.setForm = function (form) {
                formScope = form;
            };

            $scope.fetchCatalogs = function (criteria) {
                criteria.isVirtual = false;
                return catalogs.search(criteria);
            };

            blade.refresh = function () {
                const criteria = {
                    targetCategoryId: blade.categoryId,
                    take: 1,
                };

                automaticLinkQueryApi.search(criteria, function (data) {
                    blade.isLoading = false;

                    if (data.results.length > 0) {
                        blade.originalEntity = data.results[0];
                    }
                    else {
                        blade.originalEntity = {
                            targetCategoryId: blade.categoryId,
                            sourceCatalogId: '',
                            sourceCatalogQuery: '',
                        };
                    }

                    blade.currentEntity = angular.copy(blade.originalEntity);
                });
            };

            blade.onClose = function (closeCallback) {
                bladeNavigationService.showConfirmationIfNeeded(isDirty(), canSave(), blade, saveChanges, closeCallback,
                    "catalog.dialogs.automatic-links-save.title",
                    "catalog.dialogs.automatic-links-save.message");
            };

            blade.toolbarCommands = [
                {
                    name: "platform.commands.save",
                    icon: 'fas fa-save',
                    canExecuteMethod: canSave,
                    executeMethod: saveChanges,
                    permission: blade.updatePermission
                },
                {
                    name: "platform.commands.reset",
                    icon: 'fa fa-undo',
                    canExecuteMethod: canReset,
                    executeMethod: reset,
                    permission: blade.updatePermission,
                    showSeparator: true,
                },
                {
                    name: "platform.commands.preview",
                    icon: 'fas fa-eye',
                    canExecuteMethod: canPreview,
                    executeMethod: preview,
                },
                {
                    name: "platform.commands.create",
                    icon: 'fas fa-link',
                    canExecuteMethod: canCreateLinks,
                    executeMethod: createAutomaticLinks,
                    permission: blade.updatePermission,
                },
                {
                    name: "platform.commands.delete",
                    icon: 'fas fa-trash-alt',
                    canExecuteMethod: canDeleteLinks,
                    executeMethod: deleteAutomaticLinks,
                    permission: blade.updatePermission,
                },
            ];

            function canSave() {
                return isDirty() && formScope && formScope.$valid;
            }

            function canReset() {
                return isDirty();
            }

            function canPreview() {
                return blade.currentEntity && blade.currentEntity.sourceCatalogId && blade.currentEntity.sourceCatalogQuery;
            }

            function canCreateLinks() {
                return !isDirty();
            }

            function canDeleteLinks() {
                return true;
            }

            function isDirty() {
                return !angular.equals(blade.currentEntity, blade.originalEntity) && blade.hasUpdatePermission();
            }

            function saveChanges() {
                blade.isLoading = true;
                automaticLinkQueryApi.update({}, blade.currentEntity, function () {
                    blade.refresh();
                });
            }

            function reset() {
                angular.copy(blade.originalEntity, blade.currentEntity);
            }

            function preview() {
                var newBlade = {
                    id: 'automaticLinksPreview',
                    controller: 'virtoCommerce.catalogModule.categoriesItemsListController',
                    template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/categories-items-list.tpl.html',
                    title: 'catalog.blades.automatic-links.preview-title',
                    catalogId: blade.currentEntity.sourceCatalogId,
                    filterKeyword: blade.currentEntity.sourceCatalogQuery,
                    objectTypes: ['CatalogProduct'],
                    breadcrumbs: [],
                };

                bladeNavigationService.showBlade(newBlade, blade);
            }

            function createAutomaticLinks() {
                blade.isLoading = true;

                const criteria = {
                    catalogId: blade.currentEntity.sourceCatalogId,
                    keyword: blade.currentEntity.sourceCatalogQuery,
                    objectTypes: ['CatalogProduct'],
                    searchInVariations: false,
                    take: 0,
                };

                const getProductsCountPromise = canPreview()
                    ? listEntries.listitemssearch(criteria).$promise.then(function (data) {
                        return data.totalCount;
                    })
                    : Promise.resolve(0);

                getProductsCountPromise.then(function (productsCount) {
                    blade.isLoading = false;

                    var dialog = {
                        id: "confirmCreateAutomaticLinks",
                        title: "catalog.dialogs.automatic-links-create.title",
                        message: "catalog.dialogs.automatic-links-create.message",
                        messageValues: { number: productsCount },
                        callback: function (confirm) {
                            if (confirm) {
                                blade.isLoading = true;

                                categories.updateAutomaticLinks({ id: blade.currentEntity.targetCategoryId }, {},
                                    function () {
                                        blade.isLoading = false;
                                    },
                                    function (error) {
                                        bladeNavigationService.setError('Error ' + error.status, blade);
                                    });
                            }
                        }
                    }

                    dialogService.showConfirmationDialog(dialog);
                });
            }

            function deleteAutomaticLinks() {
                const criteria = {
                    categoryIds: [blade.currentEntity.targetCategoryId],
                    objectType: 'CatalogProduct',
                    isAutomatic: true,
                    take: 0,
                };

                listEntries.searchlinks(criteria, function (data) {
                    blade.isLoading = false;

                    var dialog = {
                        id: "confirmDeleteAutomaticLinks",
                        title: "catalog.dialogs.automatic-links-delete.title",
                        message: "catalog.dialogs.automatic-links-delete.message",
                        messageValues: { number: data.totalCount },
                        callback: function (confirm) {
                            if (confirm) {
                                blade.isLoading = true;

                                categories.deleteAutomaticLinks({ id: blade.currentEntity.targetCategoryId }, {},
                                    function () {
                                        blade.isLoading = false;
                                    },
                                    function (error) {
                                        bladeNavigationService.setError('Error ' + error.status, blade);
                                    });
                            }
                        }
                    }

                    dialogService.showConfirmationDialog(dialog);
                });
            }

            blade.refresh();
        }]);
