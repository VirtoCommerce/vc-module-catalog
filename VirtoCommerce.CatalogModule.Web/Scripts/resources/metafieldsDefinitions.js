angular.module('virtoCommerce.catalogModule')
    .factory('virtoCommerce.catalogModule.metafieldsDefinitions', function () {
        
        function isCurrentEntityPhysical(blade) {
            return blade.currentEntity ? blade.currentEntity.productType === 'Physical' : false;
        }

        function isCurrentEntityDigital(blade) {
            return blade.currentEntity ? blade.currentEntity.productType === 'Digital' : false;
        }

        return {
            categoryMetafields: [
                {
                    name: "isActive",
                    title: "catalog.blades.category-detail.labels.is-active",
                    valueType: "Boolean",
                    priority: 1
                },
                {
                    name: "name",
                    title: "catalog.blades.category-detail.labels.name",
                    valueType: "ShortText",
                    isRequired: true,
                    priority: 2
                },
                {
                    name: "code",
                    templateUrl: "categoryFormCode.html",
                    priority: 3
                },
                {
                    name: "taxType",
                    templateUrl: "categoryFormTaxType.html",
                    priority: 4
                }
            ],
            productMetafields: [
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
            ]
        };
    });
