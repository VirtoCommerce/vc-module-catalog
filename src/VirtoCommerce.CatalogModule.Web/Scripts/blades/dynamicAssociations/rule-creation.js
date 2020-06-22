angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.ruleCreationController', [
        '$scope',
        'platformWebApp.bladeNavigationService',
        'virtoCommerce.catalogModule.categories',
        function ($scope, bladeNavigationService, categories) {
            var blade = $scope.blade;
            blade.isLoading = true;
            var allProperties = [];
            blade.categoryCount = 0;
            blade.propertiesCount = 0;
            blade.editedPropertiesCount = 0;

            blade.isPropertiesSelected = false;

            function initializeBlade() {
                blade.categoryCount = blade.categoryIds.length;

                if (blade.categoryIds.length > 0) {

                    categories.getByIds({ ids: blade.categoryIds },
                        data => {
                            allProperties = $scope.prepareProperties(_.unique(_.first(data.map(x => x.properties))));
                            blade.propertiesCount = allProperties.length;
                        });
                }

                blade.editedProperties = $scope.prepareProperties(blade.editedProperties, false);
                blade.editedPropertiesCount = blade.editedProperties.length;
                $scope.selectCategories();
                blade.isLoading = false;
            }

            $scope.prepareProperties = function(properties, needClear = true) {
                _.each(properties,
                    prop => {
                        prop.group = 'All properties';
                        prop.required = false;
                        prop.UseDefaultUIForEdit = true;
                        if (needClear) {
                            prop.values = [];
                        }
                        prop.isReadOnly = false;
                        if (prop.dictionary) {
                            prop.multivalue = true;
                        }
                    });
                return properties;
            };

            $scope.selectCategories = function() {
                const options = {
                    showCheckingMultiple: false,
                    allowCheckingCategory: true,
                    allowCheckingItem: false,
                    selectedItemIds: blade.categoryIds,
                    checkItemFn: (listItem, isSelected) => {
                        if (isSelected) {
                            if (!_.find(blade.categoryIds, (x) => x === listItem.id)) {
                                blade.categoryIds.push(listItem.id);
                            }
                        }
                        else {
                            blade.categoryIds = _.reject(blade.categoryIds, (x) => x === listItem.id);
                        }
                    }
                };

                const newBlade = {
                    id: "CatalogItemsSelect",
                    controller: 'virtoCommerce.catalogModule.catalogItemSelectController',
                    template: 'Modules/$(virtoCommerce.catalog)/Scripts/blades/common/catalog-items-select.tpl.html',
                    title: 'catalog.selectors.blades.titles.select-categories',
                    options: options,
                    breadcrumbs: [],
                    catalogId: blade.catalogId,
                    toolbarCommands: [
                        {
                            name: "platform.commands.confirm", icon: 'fa fa-check',
                            executeMethod: function (pickingBlade) {
                                bladeNavigationService.closeBlade(pickingBlade);
                                blade.categoryCount = blade.categoryIds.length;
                                categories.getByIds({ ids: blade.categoryIds },
                                    data => {
                                        allProperties = $scope.prepareProperties(_.unique(_.first(data.map(x => x.properties))));
                                        $scope.selectProperties();
                                    });
                            },
                            canExecuteMethod: () => _.any(blade.categoryIds)
                        },
                        {
                            name: "platform.commands.reset", icon: 'fa fa-undo',
                            executeMethod: function (pickingBlade) {
                                blade.categoryIds = [];
                                $scope.selectedCount = 0;
                                bladeNavigationService.closeBlade(pickingBlade);
                            },
                            canExecuteMethod: () => _.any(blade.categoryIds)

                        }]
                };
                bladeNavigationService.showBlade(newBlade, blade);

            };
            $scope.selectProperties = function () {
                var newBlade = {
                    id: 'propertiesSelector',
                    controller: 'virtoCommerce.catalogModule.propertiesSelectorController',
                    template: 'Modules/$(virtoCommerce.catalog)/Scripts/blades/step-select-properties.tpl.html',
                    properties: allProperties,
                    includedProperties: blade.editedProperties,
                    onSelected: function (includedProperties) {
                        blade.editedProperties = includedProperties;
                        blade.editedPropertiesCount = blade.editedProperties.length;
                        $scope.editProperties();
                    }
                };

                bladeNavigationService.showBlade(newBlade, blade);
            };

            $scope.editProperties = function () {
                var newBlade = {
                    id: 'propertiesEditor',
                    controller: 'virtoCommerce.catalogModule.editPropertiesActionStepController',
                    template: 'Modules/$(virtoCommerce.catalog)/Scripts/blades/step-edit-properties.tpl.html',
                    properties: blade.editedProperties,
                    propGroups: [{ title: 'catalog.properties.product', type: 'Product' }, { title: 'catalog.properties.variation', type: 'Variation' }],
                    onSelected: function (editedProps) {
                        blade.editedProperties = editedProps;
                    },
                    toolbarCommands: [
                        {
                            name: "platform.commands.preview", icon: 'fa fa-filter',
                            executeMethod: (pickingBlade) => {
                                var viewerBlade = {
                                    id: 'propertiesSelector',
                                    controller: 'virtoCommerce.catalogModule.dynamicAssociationViewerController',
                                    template: 'Modules/$(virtoCommerce.catalog)/Scripts/blades/dynamicAssociations/dynamicAssociation-viewer.tpl.html',
                                    categoryIds: blade.categoryIds,
                                    properties: pickingBlade.currentEntities
                                };
                                bladeNavigationService.showBlade(viewerBlade, pickingBlade);
                            },
                            canExecuteMethod: () => true
                        }]

                };
                bladeNavigationService.showBlade(newBlade, blade);
            };

            $scope.saveRule = function() {
                if (blade.onSelected) {
                    blade.onSelected(blade.categoryIds, blade.editedProperties);
                }
                bladeNavigationService.closeBlade(blade);
            };

            $scope.blade.headIcon = 'fa-upload';
            $scope.blade.title = 'Create matching rule';

            initializeBlade();

            blade.toolbarCommands = [
                {
                    name: "platform.commands.preview",
                    icon: 'fa fa-filter',
                    executeMethod: () => {
                        var viewerBlade = {
                            id: 'propertiesSelector',
                            controller: 'virtoCommerce.catalogModule.dynamicAssociationViewerController',
                            template:
                                'Modules/$(virtoCommerce.catalog)/Scripts/blades/dynamicAssociations/dynamicAssociation-viewer.tpl.html',
                            categoryIds: blade.categoryIds,
                            properties: blade.editedProperties
                        };
                        bladeNavigationService.showBlade(viewerBlade, blade);
                    },
                    canExecuteMethod: () => blade.editedPropertiesCount > 0
                }
            ];
        }]);
