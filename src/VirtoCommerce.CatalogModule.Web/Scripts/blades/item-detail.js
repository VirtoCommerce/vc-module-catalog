angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.itemDetailController', ['$rootScope', '$scope', '$translate', 'platformWebApp.bladeNavigationService', 'platformWebApp.settings', 'virtoCommerce.catalogModule.items', 'virtoCommerce.customerModule.members', 'virtoCommerce.catalogModule.catalogs', 'platformWebApp.metaFormsService', 'virtoCommerce.catalogModule.categories', function ($rootScope, $scope, $translate, bladeNavigationService, settings, items, members, catalogs, metaFormsService, categories) {
        var blade = $scope.blade;
        blade.updatePermission = 'catalog:update';
        blade.currentEntityId = blade.itemId;

        blade.metaFields = metaFormsService.getMetaFields("productDetail");
        blade.metaFields1 = metaFormsService.getMetaFields("productDetail1");
        blade.metaFields2 = metaFormsService.getMetaFields("productDetail2");

        blade.hasVendorsPermission = bladeNavigationService.checkPermission('customer:read');

        blade.refresh = function (parentRefresh) {
            blade.isLoading = true;
            //2015 = Full ~& Variations do not load product variations
            return items.get({ id: blade.itemId, respGroup: 2015 }, function (data) {
                if (!blade.catalog) {
                    catalogs.get({ id: data.catalogId }, function (catalogResult) {
                        blade.catalog = catalogResult;
                        fillItem(data, parentRefresh);
                    });
                } else {
                    fillItem(data, parentRefresh);
                }
            },
                function (error) { bladeNavigationService.setError('Error ' + error.status, blade); });
        }

        function fillItem(data, parentRefresh) {
            blade.itemId = data.id;
            blade.title = data.code;
            blade.securityScopes = data.securityScopes;
            if (!data.productType) {
                data.productType = 'Physical';
            }

            blade.subtitle = 'catalog.blades.item-detail.subtitle';
            blade.subtitleValues = { productTypeName: $translate.instant('catalog.product-types.' + data.productType) };

            var linkWithPriority = getLinkWithPriority(data);
            data._priority = (linkWithPriority ? linkWithPriority.priority : data.priority) || 0;

            blade.item = angular.copy(data);
            blade.currentEntity = blade.item;
            blade.origItem = data;
            blade.isLoading = false;

            if (parentRefresh && blade.parentBlade.refresh) {
                blade.parentBlade.refresh();
            }
            if (blade.childrenBlades) {
                _.each(blade.childrenBlades, function (x) {
                    if (x.refresh) {
                        x.refresh(blade.item);
                    }
                });
            }
        }

        blade.codeValidator = function (value) {
            var pattern = /[$+;=%{}[\]|@~!^*&()?'<>,]/;
            return !pattern.test(value);
        };


        function isDirty() {
            return !angular.equals(blade.item, blade.origItem) && blade.hasUpdatePermission();
        }

        function canSave() {
            return isDirty() && blade.formScope && blade.formScope.$valid
                && isValidQuantity(blade.item);
        }

        function isValidQuantity(item) {
            const minEmpty = isEmpty(item.minQuantity);
            const maxEmpty = isEmpty(item.maxQuantity);
            const packSizeEmpty = isEmpty(item.packSize);

            let minNumber = parseInt(item.minQuantity, 10);
            let maxNumber = parseInt(item.maxQuantity, 10);
            let packSize = parseInt(item.packSize, 10);

            if (minEmpty) {
                minNumber = 0;
            }

            if (maxEmpty) {
                maxNumber = Number.MAX_VALUE;
            }

            if (packSizeEmpty) {
                return false;
            }

            if (minNumber < 0 || maxNumber < 0) {
                return false;
            }

            if (packSize < 1) {
                return false;
            }

            if (minNumber <= packSize && packSize <= maxNumber)
            {
                return true;
            }

            return false;
        }

        function isEmpty(value) {
            return value == null || value === '' || value === '0' || value === 0;
        }

        function saveChanges() {
            blade.isLoading = true;

            var linkWithPriority = getLinkWithPriority(blade.item);
            if (linkWithPriority) {
                linkWithPriority.priority = blade.item._priority;
            } else {
                blade.item.priority = blade.item._priority;
            }

            items.update({}, blade.item, function () {
                blade.refresh(true);
            });
        }

        blade.onClose = function (closeCallback) {
            bladeNavigationService.showConfirmationIfNeeded(isDirty(), canSave(), blade, saveChanges, closeCallback, "catalog.dialogs.item-save.title", "catalog.dialogs.item-save.message");
        };

        function getLinkWithPriority(data) {
            var retVal;
            if (bladeNavigationService.catalogsSelectedCatalog && bladeNavigationService.catalogsSelectedCatalog.isVirtual) {
                retVal = _.find(data.links, function (l) {
                    return l.catalogId === bladeNavigationService.catalogsSelectedCatalog.id &&
                        (!bladeNavigationService.catalogsSelectedCategoryId || l.categoryId === bladeNavigationService.catalogsSelectedCategoryId);
                });
            }
            return retVal;
        }

        blade.formScope = null;
        $scope.setForm = function (form) { blade.formScope = form; }

        blade.headIcon = blade.productType === 'Digital' ? 'fa fa-file-zip-o' : 'fa fa-dropbox';

        blade.toolbarCommands = [
            {
                name: "platform.commands.save", icon: 'fas fa-save',
                executeMethod: saveChanges,
                canExecuteMethod: canSave,
                permission: blade.updatePermission
            },
            {
                name: "platform.commands.reset", icon: 'fa fa-undo',
                executeMethod: function () {
                    angular.copy(blade.origItem, blade.item);
                },
                canExecuteMethod: isDirty,
                permission: blade.updatePermission
            },
            {
                name: "platform.commands.clone", icon: 'fas fa-clone',
                executeMethod: function () {
                    blade.isLoading = true;
                    items.cloneItem({ itemId: blade.itemId }, function (data) {
                        var newBlade = {
                            id: blade.id,
                            item: data,
                            catalog: blade.catalog,
                            title: "catalog.wizards.new-product.title",
                            subtitle: 'catalog.wizards.new-product.subtitle',
                            controller: 'virtoCommerce.catalogModule.newProductWizardController',
                            template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/wizards/newProduct/new-product-wizard.tpl.html'
                        };
                        bladeNavigationService.showBlade(newBlade, blade.parentBlade);
                    },
                        function (error) { bladeNavigationService.setError('Error ' + error.status, blade); });

                },
                canExecuteMethod: function () { return !isDirty(); },
                permission: 'catalog:create'
            }
        ];

        // datepicker
        blade.datepickers = {};
        blade.open = function ($event, which) {
            $event.preventDefault();
            $event.stopPropagation();
            blade.datepickers[which] = true;
        };

        blade.fetchVendors = function (criteria) {
            return blade.hasVendorsPermission
                ? members.search(criteria)
                : criteria.objectIds.map(x => ({ id: x, name: $translate.instant('catalog.blades.item-detail.labels.vendor-denied') }));
        }

        blade.openVendorsManagement = function () {
            var newBlade = {
                id: 'vendorList',
                currentEntity: { id: null },
                controller: 'virtoCommerce.customerModule.memberListController',
                template: 'Modules/$(VirtoCommerce.Customer)/Scripts/blades/member-list.tpl.html'
            };
            bladeNavigationService.showBlade(newBlade, blade);
        };

        blade.openDictionarySettingManagement = function (setting) {
            var newBlade = {
                id: 'settingDetailChild',
                isApiSave: true,
                controller: 'platformWebApp.settingDictionaryController',
                template: '$(Platform)/Scripts/app/settings/blades/setting-dictionary.tpl.html'
            };
            switch (setting) {
                case 'TaxTypes':
                    _.extend(newBlade, {
                        currentEntityId: 'VirtoCommerce.Core.General.TaxTypes',
                        parentRefresh: function (data) { blade.taxTypes = data; }
                    });
                    break;
            }

            bladeNavigationService.showBlade(newBlade, blade);
        };

        $scope.$on("refresh-entity-by-id", function (event, id) {
            if (blade.currentEntityId === id) {
                blade.refresh();
            }
        });

        blade.associationsAdded = function (associations) {
            blade.currentEntity.associations = blade.currentEntity.associations.concat(associations);
            blade.origItem.associations = blade.origItem.associations.concat(associations);
        }

        blade.associationsDeleted = function (associationIds) {
            var predicate = (association) => { return _.contains(associationIds, association.id) };
            blade.currentEntity.associations = _.reject(blade.currentEntity.associations, predicate);
            blade.origItem.associations = _.reject(blade.origItem.associations, predicate);
        }

        blade.taxTypes = settings.getValues({ id: 'VirtoCommerce.Core.General.TaxTypes' });

        blade.refresh(false);
    }]);
