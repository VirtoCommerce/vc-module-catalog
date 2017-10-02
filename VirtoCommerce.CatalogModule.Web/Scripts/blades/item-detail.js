angular.module('virtoCommerce.catalogModule')
.controller('virtoCommerce.catalogModule.itemDetailController', ['$rootScope', '$scope', 'platformWebApp.bladeNavigationService', 'platformWebApp.settings', 'virtoCommerce.catalogModule.items', 'virtoCommerce.customerModule.members', 'virtoCommerce.catalogModule.catalogs', function ($rootScope, $scope, bladeNavigationService, settings, items, members, catalogs) {
    var blade = $scope.blade;
    blade.updatePermission = 'catalog:update';
    blade.currentEntityId = blade.itemId;

    blade.metaFields = getMetafields();

    function getMetafields() {
        return [
            {
                name: "code",
                templateUrl: "detailFormCode.html",
                priority: 1
            },
            {
                name: "_priority",
                title: "catalog.blades.item-detail.labels.priority",
                valueType: "Integer",
                priority: 2
            },
            {
                name: "name",
                title: "catalog.blades.item-detail.labels.name",
                valueType: "LongText",
                isRequired: true,
                priority: 3
            },
            {
                name: "gtin",
                templateUrl: "detailFormGtin.html",
                priority: 4
            },
            {
                name: "isBuyable",
                title: "catalog.blades.item-detail.labels.can-be-purchased",
                valueType: "Boolean",
                priority: 5
            },
            {
                name: "isActive",
                title: "catalog.blades.item-detail.labels.store-visible",
                valueType: "Boolean",
                priority: 6
            },
            {
                name: "trackInventory",
                title: "catalog.blades.item-detail.labels.track-inventory",
                valueType: "Boolean",
                isVisibleFn: isCurrentEntityPhysical,
                priority: 7
            },
            {
                name: "hasUserAgreement",
                title: "catalog.blades.item-detail.labels.has-user-agreement",
                valueType: "Boolean",
                isVisibleFn: isCurrentEntityDigital,
                priority: 8
            },

            {
                name: "minQuantity",
                title: "catalog.blades.item-detail.labels.min-quantity",
                valueType: "Integer",
                isVisibleFn: isCurrentEntityPhysical,
                priority: 9
            },
            {
                name: "maxQuantity",
                title: "catalog.blades.item-detail.labels.max-quantity",
                valueType: "Integer",
                isVisibleFn: isCurrentEntityPhysical,
                priority: 10
            },

            {
                name: "downloadType",
                templateUrl: "detailFormDownloadType.html",
                isVisibleFn: isCurrentEntityDigital,
                priority: 11
            },
            {
                name: "maxNumberOfDownload",
                title: "catalog.blades.item-detail.labels.max-downloads",
                valueType: "Integer",
                isVisibleFn: isCurrentEntityDigital,
                priority: 12
            },
            {
                name: "downloadExpiration",
                title: "catalog.blades.item-detail.labels.expiration-date",
                valueType: "DateTime",
                isVisibleFn: isCurrentEntityDigital,
                priority: 13
            },

            {
                name: "vendor",
                templateUrl: "detailFormVendor.html",
                isVisibleFn: isCurrentEntityPhysical,
                priority: 14
            },
            {
                name: "taxType",
                templateUrl: "detailFormTaxType.html",
                isVisibleFn: isCurrentEntityPhysical,
                priority: 15
            }
        ];
    }

    function isCurrentEntityPhysical() {
        return blade.currentEntity ? blade.currentEntity.productType === 'Physical' : false;
    }

    function isCurrentEntityDigital() {
        return blade.currentEntity ? blade.currentEntity.productType === 'Digital' : false;
    }

    blade.refresh = function (parentRefresh) {
        blade.isLoading = true;

        return items.get({ id: blade.itemId }, function (data) {

            if (!blade.catalog) {
                blade.catalog = catalogs.get({ id: data.catalogId });
            }

            blade.itemId = data.id;
            blade.title = data.code;
            blade.securityScopes = data.securityScopes;
            if (!data.productType) {
                data.productType = 'Physical';
            }
            blade.subtitle = data.productType + ' item details';

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
        },
        function (error) { bladeNavigationService.setError('Error ' + error.status, blade); });
    }


    blade.codeValidator = function (value) {
        var pattern = /[$+;=%{}[\]|\\\/@ ~!^*&()?:'<>,]/;
        return !pattern.test(value);
    };

    function isDirty() {
        return !angular.equals(blade.item, blade.origItem) && blade.hasUpdatePermission();
    };

    function canSave() {
        return isDirty() && blade.formScope && blade.formScope.$valid;
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
        }, function (error) { bladeNavigationService.setError('Error ' + error.status, blade); });
    };

    blade.onClose = function (closeCallback) {
        bladeNavigationService.showConfirmationIfNeeded(isDirty(), canSave(), blade, saveChanges, closeCallback, "catalog.dialogs.item-save.title", "catalog.dialogs.item-save.message");
    };

    function getLinkWithPriority(data) {
        var retVal;
        if (bladeNavigationService.catalogsSelectedCatalog && bladeNavigationService.catalogsSelectedCatalog.isVirtual) {
            retVal = _.find(data.links, function (l) {
                return l.catalogId == bladeNavigationService.catalogsSelectedCatalog.id &&
                        (!bladeNavigationService.catalogsSelectedCategoryId || l.categoryId === bladeNavigationService.catalogsSelectedCategoryId);
            });
        }
        return retVal;
    }

    blade.formScope = null;
    $scope.setForm = function (form) { blade.formScope = form; }

    blade.headIcon = blade.productType === 'Digital' ? 'fa-file-zip-o' : 'fa-dropbox';

    blade.toolbarCommands = [
	    {
	        name: "platform.commands.save", icon: 'fa fa-save',
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
            name: "platform.commands.clone", icon: 'fa fa-files-o',
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
    $scope.datepickers = {}
    $scope.open = function ($event, which) {
        $event.preventDefault();
        $event.stopPropagation();
        $scope.datepickers[which] = true;
    };

    function initVendors() {
        blade.vendors = members.search({
            memberType: 'Vendor',
            sort: 'name:asc',
            take: 1000
        });
    }

    blade.openVendorsManagement = function () {
        var newBlade = {
            memberType: 'Vendor',
            parentRefresh: initVendors,
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

    initVendors();
    blade.taxTypes = settings.getValues({ id: 'VirtoCommerce.Core.General.TaxTypes' });

    blade.refresh(false);
}]);
