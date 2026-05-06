angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.itemDetailController', [
        '$scope',
        '$injector',
        function ($scope, $injector) {
        var deps = getItemDetailDependencies($injector);
        var blade = $scope.blade;

        blade.updatePermission = 'catalog:update';
        blade.hasUpdatePermission = function () {
            return deps.authService.checkPermission('catalog:products:update', blade.securityScopes) ||
                   deps.authService.checkPermission('catalog:update', blade.securityScopes);
        };
        blade.hasCreatePermission = function () {
            return deps.authService.checkPermission('catalog:products:create', blade.securityScopes) ||
                   deps.authService.checkPermission('catalog:create', blade.securityScopes);
        };
        blade.currentEntityId = blade.itemId;

        blade.metaFields = deps.metaFormsService.getMetaFields("productDetail");
        blade.metaFields1 = deps.metaFormsService.getMetaFields("productDetail1");
        blade.metaFields2 = deps.metaFormsService.getMetaFields("productDetail2");

        blade.hasVendorsPermission = deps.bladeNavigationService.checkPermission('customer:read');

        blade.refresh = function (parentRefresh) {
            blade.isLoading = true;
            //2015 = Full ~& Variations do not load product variations
            return deps.items.get({ id: blade.itemId, respGroup: 2015 }, function (data) {
                if (!blade.catalog) {
                    deps.catalogs.get({ id: data.catalogId }, function (catalogResult) {
                        blade.catalog = catalogResult;
                        fillItem(blade, deps, data, parentRefresh);
                    });
                } else {
                    fillItem(blade, deps, data, parentRefresh);
                }
            },
                function (error) {
                    deps.bladeNavigationService.setError('Error ' + error.status, blade);
                });
        };

        blade.codeValidator = function (value) {
            var pattern = /[$+;=%{}[\]|@~!^*&()?'<>,]/;
            return !pattern.test(value);
        };

        function isDirty() {
            return isItemDirty(blade);
        }

        function canSave() {
            return canSaveItem(blade);
        }

        function saveChanges() {
            saveItemChanges(blade, deps);
        }

        blade.onClose = function (closeCallback) {
            deps.bladeNavigationService.showConfirmationIfNeeded(
                isDirty(),
                canSave(),
                blade,
                saveChanges,
                closeCallback,
                "catalog.dialogs.item-save.title",
                "catalog.dialogs.item-save.message");
        };

        blade.formScope = null;
        $scope.setForm = function (form) {
            blade.formScope = form;
        };

        blade.headIcon = blade.productType === 'Digital' ? 'fa fa-file-zip-o' : 'fa fa-dropbox';
        blade.toolbarCommands = createItemDetailToolbarCommands(blade, deps, isDirty, canSave, saveChanges);

        // datepicker
        blade.datepickers = {};
        blade.open = function ($event, which) {
            $event.preventDefault();
            $event.stopPropagation();
            blade.datepickers[which] = true;
        };

        blade.fetchVendors = function (criteria) {
            return fetchVendors(blade, deps, criteria);
        };

        blade.openVendorsManagement = function () {
            openVendorsManagement(blade, deps);
        };

        blade.openDictionarySettingManagement = function (setting) {
            openDictionarySettingManagement(blade, deps, setting);
        };

        $scope.$on("refresh-entity-by-id", function (event, id) {
            if (blade.currentEntityId === id) {
                blade.refresh();
            }
        });

        blade.associationsAdded = function (associations) {
            blade.currentEntity.associations = blade.currentEntity.associations.concat(associations);
            blade.origItem.associations = blade.origItem.associations.concat(associations);
        };

        blade.associationsDeleted = function (associationIds) {
            var predicate = function (association) {
                return _.contains(associationIds, association.id);
            };
            blade.currentEntity.associations = _.reject(blade.currentEntity.associations, predicate);
            blade.origItem.associations = _.reject(blade.origItem.associations, predicate);
        };

        blade.taxTypes = deps.settings.getValues({ id: 'VirtoCommerce.Core.General.TaxTypes' });

        blade.refresh(false);
    }]);

function getItemDetailDependencies($injector) {
    return {
        $translate: $injector.get('$translate'),
        bladeNavigationService: $injector.get('platformWebApp.bladeNavigationService'),
        settings: $injector.get('platformWebApp.settings'),
        items: $injector.get('virtoCommerce.catalogModule.items'),
        members: $injector.get('virtoCommerce.customerModule.members'),
        catalogs: $injector.get('virtoCommerce.catalogModule.catalogs'),
        metaFormsService: $injector.get('platformWebApp.metaFormsService'),
        authService: $injector.get('platformWebApp.authService')
    };
}

function fillItem(blade, deps, data, parentRefresh) {
    blade.itemId = data.id;
    blade.title = data.code;
    blade.securityScopes = data.securityScopes;
    if (!data.productType) {
        data.productType = 'Physical';
    }

    blade.subtitle = 'catalog.blades.item-detail.subtitle';
    blade.subtitleValues = { productTypeName: deps.$translate.instant('catalog.product-types.' + data.productType) };

    var linkWithPriority = getLinkWithPriority(deps.bladeNavigationService, data);
    data._priority = (linkWithPriority ? linkWithPriority.priority : data.priority) || 0;

    blade.item = angular.copy(data);
    blade.currentEntity = blade.item;
    blade.origItem = data;
    blade.isLoading = false;
    if (!blade.hasUpdatePermission()) {
        blade.toolbarCommands = blade.toolbarCommands.filter(function (cmd) {
            return cmd.name !== 'platform.commands.save' && cmd.name !== 'platform.commands.reset';
        });
    }

    if (parentRefresh && blade.parentBlade && blade.parentBlade.refresh) {
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

function isItemDirty(blade) {
    return !angular.equals(blade.item, blade.origItem) && blade.hasUpdatePermission();
}

function canSaveItem(blade) {
    return isItemDirty(blade) && blade.formScope && blade.formScope.$valid && isValidQuantity(blade.item);
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

    if (minNumber > maxNumber) {
        return false;
    }

    if (packSize > maxNumber) {
        return false;
    }

    return true;
}

function isEmpty(value) {
    return value == null || value === '' || value === '0' || value === 0;
}

function saveItemChanges(blade, deps) {
    blade.isLoading = true;

    var linkWithPriority = getLinkWithPriority(deps.bladeNavigationService, blade.item);
    if (linkWithPriority) {
        linkWithPriority.priority = blade.item._priority;
    } else {
        blade.item.priority = blade.item._priority;
    }

    deps.items.update({}, blade.item, function () {
        blade.refresh(true);
    });
}

function getLinkWithPriority(bladeNavigationService, data) {
    var retVal;
    if (bladeNavigationService.catalogsSelectedCatalog && bladeNavigationService.catalogsSelectedCatalog.isVirtual) {
        retVal = _.find(data.links, function (l) {
            return l.catalogId === bladeNavigationService.catalogsSelectedCatalog.id &&
                (!bladeNavigationService.catalogsSelectedCategoryId ||
                    l.categoryId === bladeNavigationService.catalogsSelectedCategoryId);
        });
    }
    return retVal;
}

function createItemDetailToolbarCommands(blade, deps, isDirty, canSave, saveChanges) {
    return [
        {
            name: "platform.commands.save", icon: 'fas fa-save',
            executeMethod: saveChanges,
            canExecuteMethod: canSave
        },
        {
            name: "platform.commands.reset", icon: 'fa fa-undo',
            executeMethod: function () {
                angular.copy(blade.origItem, blade.item);
            },
            canExecuteMethod: isDirty
        },
        {
            name: "platform.commands.clone", icon: 'fas fa-clone',
            executeMethod: function () {
                cloneItem(blade, deps);
            },
            canExecuteMethod: function () {
                return !isDirty() && blade.hasCreatePermission();
            }
        }
    ];
}

function cloneItem(blade, deps) {
    blade.isLoading = true;
    deps.items.cloneItem({ itemId: blade.itemId }, function (data) {
        var newBlade = {
            id: blade.id,
            item: data,
            catalog: blade.catalog,
            title: "catalog.wizards.new-product.title",
            subtitle: 'catalog.wizards.new-product.subtitle',
            controller: 'virtoCommerce.catalogModule.newProductWizardController',
            template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/wizards/newProduct/new-product-wizard.tpl.html'
        };
        deps.bladeNavigationService.showBlade(newBlade, blade.parentBlade);
    },
        function (error) {
            deps.bladeNavigationService.setError('Error ' + error.status, blade);
        });
}

function fetchVendors(blade, deps, criteria) {
    return blade.hasVendorsPermission
        ? deps.members.search(criteria)
        : criteria.objectIds.map(function (x) {
            return { id: x, name: deps.$translate.instant('catalog.blades.item-detail.labels.vendor-denied') };
        });
}

function openVendorsManagement(blade, deps) {
    var newBlade = {
        id: 'vendorList',
        currentEntity: { id: null },
        controller: 'virtoCommerce.customerModule.memberListController',
        template: 'Modules/$(VirtoCommerce.Customer)/Scripts/blades/member-list.tpl.html'
    };
    deps.bladeNavigationService.showBlade(newBlade, blade);
}

function openDictionarySettingManagement(blade, deps, setting) {
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
                parentRefresh: function (data) {
                    blade.taxTypes = data;
                }
            });
            break;
    }

    deps.bladeNavigationService.showBlade(newBlade, blade);
}
