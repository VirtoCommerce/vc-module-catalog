//Call this to register our module to main application
var catalogsModuleName = "virtoCommerce.catalogModule";

if (AppDependencies != undefined) {
    AppDependencies.push(catalogsModuleName);
}

angular.module(catalogsModuleName, ['ui.grid.validate', 'ui.grid.infiniteScroll'])
    .config(['$stateProvider', function ($stateProvider) {
        $stateProvider
            .state('workspace.catalog', {
                url: '/catalog',
                templateUrl: '$(Platform)/Scripts/common/templates/home.tpl.html',
                controller: [
                    '$scope', '$location', 'platformWebApp.bladeNavigationService', function ($scope, $location, bladeNavigationService) {

                        var productId = $location.search().productId;
                        var categoryId = $location.search().categoryId;
                        var catalogId = $location.search().catalogId;

                        if (productId) {
                            var productBlade = {
                                id: 'categories',
                                level: 0,
                                itemId: productId,
                                productType: 'Physical',
                                title: '',
                                catalog: '',
                                controller: 'virtoCommerce.catalogModule.itemDetailController',
                                template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/item-detail.tpl.html',
                                isClosingDisabled: true
                            };

                            bladeNavigationService.showBlade(productBlade);
                        }
                        else if (categoryId) {
                            var categoryBlade = {
                                id: 'categories',
                                level: 0,
                                isBrowsingLinkedCategory: false,
                                breadcrumbs: [],
                                title: 'catalog.blades.categories-items-list.title',
                                subtitle: 'catalog.blades.categories-items-list.subtitle',
                                subtitleValues: '',
                                catalogId: catalogId,
                                categoryId: categoryId,
                                catalog: '',
                                controller: 'virtoCommerce.catalogModule.categoriesItemsListController',
                                template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/categories-items-list.tpl.html',
                                isClosingDisabled: true
                            };
                            bladeNavigationService.showBlade(categoryBlade);
                        }
                        else {
                            var blade = {
                                id: 'categories',
                                title: 'catalog.blades.catalogs-list.title',
                                breadcrumbs: [],
                                subtitle: 'catalog.blades.catalogs-list.subtitle',
                                controller: 'virtoCommerce.catalogModule.catalogsListController',
                                template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/catalogs-list.tpl.html',
                                isClosingDisabled: true
                            };
                            bladeNavigationService.showBlade(blade);
                        }

                        $scope.moduleName = 'vc-catalog';
                    }
                ]
            })
            .state('workspace.measures', {
                url: '/measures',
                templateUrl: '$(Platform)/Scripts/common/templates/home.tpl.html',
                controller: [
                    '$scope', 'platformWebApp.bladeNavigationService', function ($scope, bladeNavigationService) {

                        var blade = {
                            id: 'measures',
                            controller: 'virtoCommerce.catalogModule.measuresListController',
                            template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/measures/measures-list.html',
                            isClosingDisabled: true
                        };
                        bladeNavigationService.showBlade(blade);
                        $scope.moduleName = 'vc-catalog-measure';
                    }
                ]
            });
    }])
    // define search filters to be accessible platform-wide
    .factory('virtoCommerce.catalogModule.predefinedSearchFilters', ['$localStorage', function ($localStorage) {
        $localStorage.catalogSearchFilters = $localStorage.catalogSearchFilters || [];

        return {
            register: function (currentFiltersUpdateTime, currentFiltersStorageKey, newFilters) {
                _.each(newFilters, function (newFilter) {
                    var found = _.find($localStorage.catalogSearchFilters, function (x) {
                        return x.id === newFilter.id;
                    });
                    if (found) {
                        if (!found.lastUpdateTime || found.lastUpdateTime < currentFiltersUpdateTime) {
                            angular.copy(newFilter, found);
                        }
                    } else if (!$localStorage[currentFiltersStorageKey] || $localStorage[currentFiltersStorageKey] < currentFiltersUpdateTime) {
                        $localStorage.catalogSearchFilters.splice(0, 0, newFilter);
                    }
                });

                $localStorage[currentFiltersStorageKey] = currentFiltersUpdateTime;
            }
        };
    }])

    .factory('virtoCommerce.catalogModule.itemTypesResolverService', function () {
        return {
            objects: [],
            registerType: function (itemTypeDefinition) {
                this.objects.push(itemTypeDefinition);
            },
            resolve: function (type) {
                return _.findWhere(this.objects, { productType: type });
            }
        };
    })

    .factory('virtoCommerce.catalogModule.catalogImagesFolderPathHelper', [function () {
        return {
            getImagesFolderPath: function (catalogId, code) {
                var catalogShortName = catalogId.length > 5 ? catalogId.substring(0, 5) : catalogId;
                return catalogShortName + '/' + code;
            }
        };
    }])

    .run(
        ['$injector', 'platformWebApp.mainMenuService', 'platformWebApp.widgetService', '$state', 'platformWebApp.bladeNavigationService', 'virtoCommerce.catalogModule.catalogExportService', 'platformWebApp.permissionScopeResolver', 'virtoCommerce.catalogModule.catalogs', 'virtoCommerce.catalogModule.predefinedSearchFilters', 'platformWebApp.metaFormsService', 'virtoCommerce.catalogModule.itemTypesResolverService', '$http', '$compile', 'platformWebApp.toolbarService', 'platformWebApp.breadcrumbHistoryService', 'platformWebApp.authService',
            function ($injector, mainMenuService, widgetService, $state, bladeNavigationService, catalogExportService, scopeResolver, catalogs, predefinedSearchFilters, metaFormsService, itemTypesResolverService, $http, $compile, toolbarService, breadcrumbHistoryService, authService) {

                //Register module in main menu
                var menuItem = {
                    path: 'browse/catalog',
                    icon: 'fa fa-folder',
                    title: 'catalog.main-menu-title',
                    priority: 20,
                    action: function () {
                        $state.go('workspace.catalog', {}, { reload: true });
                    },
                    permission: 'catalog:access'
                };
                mainMenuService.addMenuItem(menuItem);

                var measureMenuItem = {
                    path: 'browse/measures',
                    icon: 'fas fa-ruler-combined',
                    title: 'catalog.measure-menu-title',
                    priority: 100,
                    action: function () { $state.go('workspace.measures'); },
                    permission: 'measures:access'
                };
                mainMenuService.addMenuItem(measureMenuItem);


                // register back-button
                toolbarService.register(breadcrumbHistoryService.getBackButtonInstance(), 'virtoCommerce.catalogModule.categoriesItemsListController');
                toolbarService.register(breadcrumbHistoryService.getBackButtonInstance(), 'virtoCommerce.catalogModule.catalogItemSelectController');

                //Register image widget
                var entryImageWidget = {
                    controller: 'virtoCommerce.catalogModule.catalogEntryImageWidgetController',
                    size: [2, 2],
                    template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/widgets/catalogEntryImageWidget.tpl.html'
                };
                widgetService.registerWidget(entryImageWidget, 'itemDetail');

                //Register video widget
                var itemVideoWidget = {
                    controller: 'virtoCommerce.catalogModule.videoWidgetController',
                    template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/widgets/videoWidget.tpl.html'
                };
                widgetService.registerWidget(itemVideoWidget, 'itemDetail');

                //Register item property widget
                var itemPropertyWidget = {
                    controller: 'virtoCommerce.catalogModule.itemPropertyWidgetController',
                    template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/widgets/itemPropertyWidget.tpl.html'
                };
                widgetService.registerWidget(itemPropertyWidget, 'itemDetail');

                //Register item associations widget
                var itemAssociationsWidget = {
                    controller: 'virtoCommerce.catalogModule.itemAssociationsWidgetController',
                    template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/widgets/itemAssociationsWidget.tpl.html'
                };
                widgetService.registerWidget(itemAssociationsWidget, 'itemDetail');

                //Register item seo widget
                var itemSeoWidget = {
                    controller: 'virtoCommerce.coreModule.seo.seoWidgetController',
                    template: 'Modules/$(VirtoCommerce.Core)/Scripts/SEO/widgets/seoWidget.tpl.html',
                    objectType: 'CatalogProduct',
                    getDefaultContainerId: function (blade) { return undefined; },
                    getLanguages: function (blade) { return _.pluck(blade.catalog.languages, 'languageCode'); }
                };
                widgetService.registerWidget(itemSeoWidget, 'itemDetail');

                //Register dimensions widget
                var dimensionsWidget = {
                    controller: 'virtoCommerce.catalogModule.itemDimensionWidgetController',
                    isVisible: function (blade) { return blade.productType === 'Physical' || blade.productType === 'BillOfMaterials'; },
                    size: [2, 1],
                    template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/widgets/itemDimensionWidget.tpl.html'
                };
                widgetService.registerWidget(dimensionsWidget, 'itemDetail');

                //Register item editorialReview widget
                var editorialReviewWidget = {
                    controller: 'virtoCommerce.catalogModule.editorialReviewWidgetController',
                    template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/widgets/editorialReviewWidget.tpl.html'
                };
                widgetService.registerWidget(editorialReviewWidget, 'itemDetail');

                //Register variation widget
                var variationWidget = {
                    controller: 'virtoCommerce.catalogModule.itemVariationWidgetController',
                    isVisible: function (blade) { return blade.id !== 'variationDetail'; },
                    size: [1, 1],
                    template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/widgets/itemVariationWidget.tpl.html'
                };
                widgetService.registerWidget(variationWidget, 'itemDetail');

                //Register asset widget
                var itemAssetWidget = {
                    controller: 'virtoCommerce.catalogModule.itemAssetWidgetController',
                    template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/widgets/itemAssetWidget.tpl.html'
                };
                widgetService.registerWidget(itemAssetWidget, 'itemDetail');

                //Register Bill of materials widget
                var billOfMaterialsWidget = {
                    controller: 'virtoCommerce.catalogModule.billOfMaterialsWidgetController',
                    isVisible: function (blade) { return blade.productType === 'BillOfMaterials'; },
                    template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/widgets/billOfMaterialsWidget.tpl.html'
                };
                widgetService.registerWidget(billOfMaterialsWidget, 'itemDetail');

                //Register widgets to categoryDetail
                widgetService.registerWidget(entryImageWidget, 'categoryDetail');

                var categoryPropertyWidget = {
                    controller: 'virtoCommerce.catalogModule.categoryPropertyWidgetController',
                    template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/widgets/categoryPropertyWidget.tpl.html'
                };
                widgetService.registerWidget(categoryPropertyWidget, 'categoryDetail');

                //Register category seo widget
                var categorySeoWidget = {
                    controller: 'virtoCommerce.coreModule.seo.seoWidgetController',
                    template: 'Modules/$(VirtoCommerce.Core)/Scripts/SEO/widgets/seoWidget.tpl.html',
                    objectType: 'Category',
                    getDefaultContainerId: function (blade) { return undefined; },
                    getLanguages: function (blade) { return _.pluck(blade.catalog.languages, 'languageCode'); }
                };
                widgetService.registerWidget(categorySeoWidget, 'categoryDetail');

                //Register category description widget
                var categoryDescriptionWidget = {
                    controller: 'virtoCommerce.catalogModule.categoryDescriptionWidgetController',
                    template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/widgets/categoryDescriptionWidget.tpl.html'
                };
                widgetService.registerWidget(categoryDescriptionWidget, 'categoryDetail');

                //Register catalog widgets
                var catalogLanguagesWidget = {
                    controller: 'virtoCommerce.catalogModule.catalogLanguagesWidgetController',
                    template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/widgets/catalogLanguagesWidget.tpl.html'
                };
                widgetService.registerWidget(catalogLanguagesWidget, 'catalogDetail');

                var catalogPropertyWidget = {
                    isVisible: function (blade) { return !blade.isNew; },
                    controller: 'virtoCommerce.catalogModule.catalogPropertyWidgetController',
                    template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/widgets/catalogPropertyWidget.tpl.html'
                };
                widgetService.registerWidget(catalogPropertyWidget, 'catalogDetail');

                // Property Groups
                var propertyGroupsWidget = {
                    isVisible: function (blade) { return !blade.isNew; },
                    controller: 'virtoCommerce.catalogModule.catalogPropertyGroupsWidgetController',
                    template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/widgets/catalogPropertyGroupsWidget.tpl.html'
                };
                widgetService.registerWidget(propertyGroupsWidget, 'catalogDetail');

                var catalogSeoWidget = {
                    controller: 'virtoCommerce.coreModule.seo.seoWidgetController',
                    template: 'Modules/$(VirtoCommerce.Core)/Scripts/SEO/widgets/seoWidget.tpl.html',
                    objectType: 'Catalog',
                    getDefaultContainerId: function (blade) { return undefined; },
                    getLanguages: function (blade) { return _.pluck(blade.currentEntity.languages, 'languageCode'); }
                };

                widgetService.registerWidget(catalogSeoWidget, 'catalogDetail');

                //Register links widgets
                var categoryLinksWidget = {
                    controller: 'virtoCommerce.catalogModule.linksWidgetController',
                    template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/widgets/linksWidget.tpl.html'
                };
                widgetService.registerWidget(categoryLinksWidget, 'categoryDetail');

                var itemsLinksWidget = {
                    controller: 'virtoCommerce.catalogModule.linksWidgetController',
                    template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/widgets/linksWidget.tpl.html'
                };
                widgetService.registerWidget(itemsLinksWidget, 'itemDetail');

                var brandSettingWidget = {
                    controller: 'virtoCommerce.catalogModule.brandSettingWidgetController',
                    template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/widgets/brandSettingWidget.tpl.html',
                    isVisible: function (blade) { return !blade.isNew; }
                };
                widgetService.registerWidget(brandSettingWidget, 'storeDetail');

                //Security scopes
                //Register permission scopes templates used for scope bounded definition in role management ui

                var catalogSelectScope = {
                    type: 'SelectedCatalogScope',
                    title: 'catalog.permissions.catalog-scope.title',
                    selectFn: function (blade, callback) {
                        var newBlade = {
                            id: 'catalog-pick',
                            title: this.title,
                            subtitle: 'catalog.permissions.catalog-scope.blade.subtitle',
                            currentEntity: this,
                            onChangesConfirmedFn: callback,
                            dataService: catalogs,
                            controller: 'platformWebApp.security.scopeValuePickFromSimpleListController',
                            template: '$(Platform)/Scripts/app/security/blades/common/scope-value-pick-from-simple-list.tpl.html'
                        };
                        bladeNavigationService.showBlade(newBlade, blade);
                    }
                };
                scopeResolver.register(catalogSelectScope);

                var categorySelectScope = {
                    type: 'CatalogSelectedCategoryScope',
                    title: 'catalog.permissions.category-scope.title',
                    selectFn: function (blade, callback) {
                        var selectedListItems = _.map(this.assignedScopes, function (x) { return { id: x.scope, name: x.label }; });
                        var options = {
                            showCheckingMultiple: false,
                            allowCheckingItem: false,
                            allowCheckingCategory: true,
                            selectedItemIds: _.map(this.assignedScopes, function (x) { return x.scope; }),
                            checkItemFn: function (listItem, isSelected) {
                                if (isSelected) {
                                    if (_.all(selectedListItems, function (x) { return x.id !== listItem.id; })) {
                                        selectedListItems.push(listItem);
                                    }
                                }
                                else {
                                    selectedListItems = _.reject(selectedListItems, function (x) { return x.id === listItem.id; });
                                }
                            }
                        };
                        var scopeOriginal = this.scopeOriginal;
                        var newBlade = {
                            id: "CatalogItemsSelect",
                            title: "catalog.blades.catalog-items-select.title",
                            controller: 'virtoCommerce.catalogModule.catalogItemSelectController',
                            template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/common/catalog-items-select.tpl.html',
                            options: options,
                            breadcrumbs: [],
                            toolbarCommands: [
                                {
                                    name: "platform.commands.confirm",
                                    icon: 'fas fa-plus',
                                    executeMethod: function (blade) {
                                        var scopes = _.map(selectedListItems, function (x) {
                                            return angular.extend({ scope: x.id, label: x.name }, scopeOriginal);
                                        });
                                        callback(scopes);
                                        bladeNavigationService.closeBlade(blade);

                                    },
                                    canExecuteMethod: function () {
                                        return selectedListItems.length > 0;
                                    }
                                }]
                        };
                        bladeNavigationService.showBlade(newBlade, blade);
                    }
                };
                scopeResolver.register(categorySelectScope);


                // register WIDGETS
                var indexWidget = {
                    controller: 'virtoCommerce.searchModule.indexWidgetController',
                    // size: [3, 1],
                    template: 'Modules/$(VirtoCommerce.Search)/Scripts/widgets/index-widget.tpl.html'
                };

                // integration: index in product details
                var widgetToRegister = angular.extend({}, indexWidget, { documentType: 'Product' });
                widgetService.registerWidget(widgetToRegister, 'itemDetail');
                // integration: index in CATEGORY details
                widgetToRegister = angular.extend({}, indexWidget, { documentType: 'Category' });
                widgetService.registerWidget(widgetToRegister, 'categoryDetail');

                // Aggregation properties in store details
                widgetService.registerWidget({
                    controller: 'virtoCommerce.catalogModule.aggregationPropertiesWidgetController',
                    template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/widgets/aggregationPropertiesWidget.tpl.html'
                }, 'storeDetail');

                // predefine search filters for catalog search
                predefinedSearchFilters.register(1477584000000, 'catalogSearchFiltersDate', [
                    { name: 'catalog.blades.categories-items-list.labels.filter-new' },
                    { keyword: '', searchInVariations: true, id: 5, name: 'catalog.blades.categories-items-list.labels.filter-display-variations' },
                    { keyword: 'is:hidden', id: 4, name: 'catalog.blades.categories-items-list.labels.filter-notActive' },
                    { keyword: 'price_usd:[100 TO 200]', id: 3, name: 'catalog.blades.categories-items-list.labels.filter-priceRange' },
                    { keyword: 'is:priced', id: 2, name: 'catalog.blades.categories-items-list.labels.filter-withPrice' },
                    { keyword: 'is:unpriced', id: 1, name: 'catalog.blades.categories-items-list.labels.filter-priceless' }
                ]);

                // register item types
                itemTypesResolverService.registerType({
                    itemType: 'catalog.blades.categories-items-add.menu.physical-product.title',
                    description: 'catalog.blades.categories-items-add.menu.physical-product.description',
                    productType: 'Physical',
                    icon: 'fas fa-box'
                });
                itemTypesResolverService.registerType({
                    itemType: 'catalog.blades.categories-items-add.menu.digital-product.title',
                    description: 'catalog.blades.categories-items-add.menu.digital-product.description',
                    productType: 'Digital',
                    icon: 'fas fa-file-download'
                });
                itemTypesResolverService.registerType({
                    itemType: 'catalog.blades.categories-items-add.menu.bill-of-materials-product.title',
                    description: 'catalog.blades.categories-items-add.menu.bill-of-materials-product.description',
                    productType: 'BillOfMaterials',
                    icon: 'far fa-list-alt'
                });

                //meta-form used for all catalog, category and item fields input.
                metaFormsService.registerMetaFields("catalogDetail", [{
                    name: 'name',
                    title: "catalog.blades.catalog-detail.labels.catalog-name",
                    placeholder: "catalog.blades.catalog-detail.placeholders.enter-name",
                    colSpan: 2,
                    isRequired: true,
                    valueType: "ShortText"
                }]);

                // category detail meta-fields
                metaFormsService.registerMetaFields("categoryDetail", [
                    {
                        title: "catalog.blades.category-detail.labels.is-active",
                        colSpan: 2,
                        templateUrl: "category-details-is-active.html"
                    },
                    {
                        name: 'name',
                        title: "catalog.blades.category-detail.labels.name",
                        placeholder: "catalog.blades.category-detail.placeholders.name",
                        colSpan: 2,
                        isRequired: true,
                        valueType: "ShortText"
                    },
                    {
                        title: "catalog.blades.category-detail.labels.localizedName",
                        placeholder: "catalog.blades.category-detail.placeholders.name",
                        colSpan: 6,
                        templateUrl: "localizedName.html"
                    },
                    {
                        title: "catalog.blades.category-detail.labels.code",
                        templateUrl: "code.html"
                    },
                    {
                        colSpan: 1,
                        spanAllColumns: true,
                        templateUrl: "taxType.html"
                    },
                    {
                        name: 'priority',
                        title: "catalog.blades.category-detail.labels.priority",
                        placeholder: "catalog.blades.category-detail.placeholders.priority",
                        colSpan: 2,
                        valueType: "Integer"
                    }
                ]);

                // Item detail blade has 3 metaforms: productDetail, productDetail1 and productDetail2
                metaFormsService.registerMetaFields("productDetail", [
                    {
                        title: "catalog.blades.item-detail.labels.store-visible",
                        colSpan: 6,
                        templateUrl: "product-details-is-active.html"
                    },
                    {
                        title: "catalog.blades.item-detail.labels.name",
                        colSpan: 6,
                        templateUrl: "name.html"
                    },
                    {
                        title: "catalog.blades.item-detail.labels.localizedName",
                        colSpan: 6,
                        templateUrl: "localizedName.html"
                    }
                ]);

                metaFormsService.registerMetaFields("productDetail1", [
                    {
                        title: "catalog.blades.item-detail.labels.sku",
                        colSpan: 6,
                        templateUrl: "sku.html"
                    },
                    {
                        title: "catalog.blades.item-detail.labels.gtin",
                        colSpan: 3,
                        templateUrl: "gtin.html"
                        },
                    {
                        title: "catalog.blades.item-detail.labels.mpn",
                        colSpan: 3,
                        templateUrl: "mpn.html"
                    },
                    {
                        colSpan: 3,
                        spanAllColumns: true,
                        templateUrl: "vendor.html"
                    },
                    {
                        colSpan: 3,
                        spanAllColumns: true,
                        templateUrl: "taxType.html"
                    },
                    {
                        name: 'id',
                        title: "catalog.blades.item-detail.labels.id",
                        colSpan: 3,
                        isReadOnly: true,
                        valueType: "ShortText"
                    },
                    {
                        name: 'outerId',
                        title: "catalog.blades.item-detail.labels.outer-id",
                        colSpan: 3,
                        placeholder: " ",
                        isReadOnly: true,
                        valueType: "ShortText"
                    }
                ]);

                metaFormsService.registerMetaFields("productDetail2", [
                    {
                        name: 'isBuyable',
                        title: "catalog.blades.item-detail.labels.can-be-purchased",
                        colSpan: 2,
                        valueType: "Boolean"
                    },
                    {
                        name: 'trackInventory',
                        title: "catalog.blades.item-detail.labels.track-inventory",
                        colSpan: 2,
                        valueType: "Boolean"
                    },
                    {
                        name: '_priority',
                        title: "catalog.blades.item-detail.labels.priority",
                        placeholder: "catalog.blades.item-detail.placeholders.priority",
                        colSpan: 2,
                        valueType: "Integer"
                    },
                    {
                        name: 'minQuantity',
                        title: "catalog.blades.item-detail.labels.min-quantity",
                        colSpan: 2,
                        valueType: "Integer"
                    },
                    {
                        name: 'maxQuantity',
                        title: "catalog.blades.item-detail.labels.max-quantity",
                        colSpan: 2,
                        valueType: "Integer"
                    },
                    {
                        name: 'packSize',
                        title: "catalog.blades.item-detail.labels.pack-size",
                        colSpan: 2,
                        valueType: "Integer"
                    },
                    {
                        title: "catalog.blades.item-detail.labels.start-date",
                        colSpan: 3,
                        templateUrl: "startDate.html"
                    },
                    {
                        title: "catalog.blades.item-detail.labels.end-date",
                        colSpan: 3,
                        templateUrl: "endDate.html"
                    },
                    {
                        colSpan: 3,
                        title: "catalog.blades.item-detail.labels.download-type",
                        isVisibleFn: blade => blade.item.productType === 'Digital',
                        templateUrl: "downloadType.html"
                    },
                    {
                        name: 'hasUserAgreement',
                        title: "catalog.blades.item-detail.labels.has-user-agreement",
                        isVisibleFn: blade => blade.item.productType === 'Digital',
                        colSpan: 3,
                        valueType: "Boolean"
                    },
                    {
                        name: 'maxNumberOfDownload',
                        title: "catalog.blades.item-detail.labels.max-downloads",
                        isVisibleFn: blade => blade.item.productType === 'Digital',
                        colSpan: 3,
                        valueType: "Integer"
                    },
                    {
                        name: 'downloadExpiration',
                        title: "catalog.blades.item-detail.labels.expiration-date",
                        placeholder: "catalog.blades.item-detail.placeholders.expiration-date",
                        isVisibleFn: blade => blade.item.productType === 'Digital',
                        colSpan: 3,
                        valueType: "DateTime"
                    }
                ]);

                metaFormsService.registerMetaFields('VirtoCommerce.CatalogModule.Core.Model.Export.ExportableProduct' + 'ExportFilter', [
                    {
                        name: 'catalogSelector',
                        title: "catalog.selectors.titles.catalogs",
                        templateUrl: 'Modules/$(VirtoCommerce.Catalog)/Scripts/selectors/catalog-selector.tpl.html'
                    },
                    {
                        name: 'categorySelector',
                        title: "catalog.selectors.titles.categories",
                        templateUrl: 'Modules/$(VirtoCommerce.Catalog)/Scripts/selectors/category-selector.tpl.html'
                    },
                    {
                        name: 'searchInVariations',
                        title: "catalog.selectors.titles.search-in-variations",
                        valueType: "Boolean"
                    },
                    {
                        name: 'searchInChildren',
                        title: "catalog.selectors.titles.search-in-children",
                        valueType: "Boolean"
                    }
                ]);

                metaFormsService.registerMetaFields('VirtoCommerce.CatalogModule.Core.Model.Export.ExportableCatalogFull' + 'ExportFilter', [
                    {
                        name: 'catalogSelector',
                        title: "catalog.selectors.titles.catalogs",
                        templateUrl: 'Modules/$(VirtoCommerce.Catalog)/Scripts/selectors/catalog-selector.tpl.html'
                    }
                ]);

                // assume that since export module is loaded it's safe to use module services and controllers
                if ($injector.modules['virtoCommerce.exportModule']) {
                    var genericViewerItemService = $injector.get('virtoCommerce.exportModule.genericViewerItemService');

                    genericViewerItemService.registerViewer('CatalogProduct', function (item) {
                        var itemCopy = angular.copy(item);

                        return {
                            id: "itemmDetail",
                            itemId: itemCopy.id,
                            productType: itemCopy.productType,
                            title: itemCopy.name,
                            controller: 'virtoCommerce.catalogModule.itemDetailController',
                            template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/item-detail.tpl.html'
                        };
                    });

                    catalogExportService.register({
                        name: 'Generic Export',
                        description: 'Export products filtered by catalogs or categories to JSON or CSV',
                        icon: 'fa-fw fa fa-database',
                        controller: 'virtoCommerce.exportModule.exportSettingsController',
                        template: 'Modules/$(VirtoCommerce.Export)/Scripts/blades/export-settings.tpl.html',
                        id: 'catalogGenericExport',
                        title: 'catalog.blades.exporter.productTitle',
                        subtitle: 'catalog.blades.exporter.productSubtitle',
                        onInitialize: function (newBlade) {
                            var exportDataRequest = {
                                exportTypeName: 'VirtoCommerce.CatalogModule.Core.Model.Export.ExportableProduct',
                                dataQuery: {
                                    exportTypeName: 'ProductExportDataQuery',
                                    categoryIds: _.pluck(newBlade.selectedCategories, 'id'),
                                    objectIds: _.pluck(newBlade.selectedProducts, 'id'),
                                    catalogIds: [newBlade.catalog.id],
                                    searchInChildren: true,
                                    isAllSelected: true
                                }
                            };
                            newBlade.exportDataRequest = exportDataRequest;
                            newBlade.totalItemsCount = (newBlade.selectedProducts || []).length;
                        }
                    });
                }

                $http.get('Modules/$(VirtoCommerce.Catalog)/Scripts/directives/itemSearch.tpl.html').then(function (response) {
                    // compile the response, which will put stuff into the cache
                    $compile(response.data);
                });

                metaFormsService.registerMetaFields('measureDetails', [
                    {
                        name: 'name',
                        title: "catalog.blades.measure-details.labels.name",
                        colSpan: 6,
                        isRequired: true,
                        valueType: "ShortText"
                    },
                    {
                        colSpan: 6,
                        templateUrl: "measure-details-code.html"
                    },
                    {
                        colSpan: 6,
                        templateUrl: "measure-details-description.html"
                    }
                ]);

                var measureUnitsWidget = {
                    controller: 'virtoCommerce.catalogModule.measureUnitsWidgetController',
                    template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/widgets/measureUnitsWidget.html'
                };
                widgetService.registerWidget(measureUnitsWidget, 'measureDetails');

                metaFormsService.registerMetaFields('measureUnitDetails', [
                    {
                        colSpan: 6,
                        templateUrl: "measure-unit-details-code.html"
                    },
                    {
                        colSpan: 6,
                        templateUrl: "measure-unit-details-name.html"
                    },
                    {
                        colSpan: 6,
                        templateUrl: "measure-unit-details-conversion-factor.html"
                    },
                    {
                        colSpan: 6,
                        templateUrl: "measure-unit-details-symbol.html"
                    }
                ]);

                //Register product configuration widget
                var productConfigurationWidget = {
                    controller: 'virtoCommerce.catalogModule.productConfigurationWidgetController',
                    template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/widgets/productConfigurationWidget.tpl.html',
                    isVisible: function (blade) { return !blade.isNew && authService.checkPermission('catalog:configurations:read'); }
                };
                widgetService.registerWidget(productConfigurationWidget, 'itemDetail');

                metaFormsService.registerMetaFields("propertyGroupDetail", [
                    {
                        name: 'name',
                        title: "catalog.blades.property-group-details.labels.name",
                        colSpan: 6,
                        isRequired: true,
                        valueType: "ShortText"
                    },
                    {
                        name: 'displayOrder',
                        title: "catalog.blades.property-group-details.labels.display-order",
                        colSpan: 6,
                        isRequired: true,
                        valueType: "Integer"
                    },
                    {
                        title: "catalog.blades.property-group-details.labels.localized-name",
                        colSpan: 6,
                        templateUrl: "localizedName.html"
                    },
                    {
                        title: "catalog.blades.property-group-details.labels.localized-description",
                        colSpan: 6,
                        templateUrl: "localizedDescription.html"
                    }

                ]);
            }]);
