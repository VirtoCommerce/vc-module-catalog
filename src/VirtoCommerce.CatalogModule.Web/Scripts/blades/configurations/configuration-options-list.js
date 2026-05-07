angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.configurationOptionsListController',
        ['$scope', 'platformWebApp.bladeNavigationService', 'platformWebApp.uiGridHelper',
        function ($scope, bladeNavigationService, uiGridHelper) {
            var blade = $scope.blade;
            blade.headIcon = 'fas fa-list';
            blade.title = 'catalog.blades.section-options-list.title';
            blade.subtitle = 'catalog.blades.section-options-list.subtitle';

            blade.toolbarCommands = [
                {
                    name: "catalog.blades.section-details.commands.add",
                    icon: 'fas fa-plus',
                    executeMethod: openOptionAddBlade,
                    canExecuteMethod: canAddOptions,
                    permission: 'catalog:configurations:update'
                },
                {
                    name: "platform.commands.delete",
                    icon: 'fas fa-trash-alt',
                    executeMethod: function () { deleteList($scope.gridApi.selection.getSelectedRows()); },
                    canExecuteMethod: isItemsChecked,
                    permission: 'catalog:configurations:delete'
                }
            ];

            $scope.setGridOptions = function (gridOptions) {
                uiGridHelper.initialize($scope, gridOptions, function (gridApi) {
                    $scope.gridApi = gridApi;
                });
            };

            $scope.openItem = function (item) {
                $scope.selectedNodeId = item.productId;
                var newBlade = {
                    id: 'optionItemDetail',
                    controller: 'virtoCommerce.catalogModule.itemDetailController',
                    template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/item-detail.tpl.html',
                    itemId: item.productId,
                    productType: item.productType,
                };

                bladeNavigationService.showBlade(newBlade, blade);
            };

            $scope.edit = function (item) {
                $scope.selectedNodeId = item.productId;

                if (blade.sectionEntity.type === 'Product' || blade.sectionEntity.type === 'Text') {
                    var newBlade = {};

                    if (blade.sectionEntity.type === 'Product') {
                        newBlade = {
                            id: 'optionProductDetail',
                            controller: 'virtoCommerce.catalogModule.configurationOptionProductDetailController',
                            template:
                                'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/configurations/option-product-detail.tpl.html',
                            origEntity: item,
                        };
                    }

                    if (blade.sectionEntity.type === 'Text') {
                        newBlade = {
                            id: 'optionTextDetail',
                            controller: 'virtoCommerce.catalogModule.configurationOptionTextDetailController',
                            template:
                                'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/configurations/option-text-detail.tpl.html',
                            origEntity: item,
                        };
                    }

                    bladeNavigationService.showBlade(newBlade, blade);
                }
            };

            $scope.delete = function (data) {
                deleteList([data]);
            };

            $scope.toggleDefaultOption = function (item) {
                blade.setDefaultOption(item, item.isDefault);
            };

            blade.setDefaultOption = function (item, shouldSetAsDefault) {
                _.each(blade.sectionEntity.options, function (option) {
                    option.isDefault = shouldSetAsDefault && option === item;
                    syncOptionDetailBlade(option);
                });
            };

            function deleteList(list) {
                bladeNavigationService.closeChildrenBlades(blade,
                    function () {
                        var undeletedEntries = _.difference(blade.sectionEntity.options, list);
                        blade.sectionEntity.options = undeletedEntries;
                    });
            }

            function isItemsChecked() {
                return $scope.gridApi && _.any($scope.gridApi.selection.getSelectedRows());
            }

            function canAddOptions() {
                return blade.sectionEntity.type === 'Product' ||
                    blade.sectionEntity.type === 'Text' && blade.sectionEntity.allowPredefinedOptions;
            }

            function syncOptionDetailBlade(option) {
                angular.forEach(blade.childrenBlades || [], function (childBlade) {
                    if (childBlade.origEntity !== option) {
                        return;
                    }

                    childBlade.origEntity.isDefault = option.isDefault;

                    if (childBlade.currentEntity) {
                        childBlade.currentEntity.isDefault = option.isDefault;
                    }
                });
            }

            function openOptionAddBlade() {
                var newBlade = {};

                if (blade.sectionEntity.type === 'Product') {
                    var selection = [];
                    var options = {
                        allowCheckingCategory: false,
                        selectedItemIds: [],
                        checkItemFn: function(listItem, isSelected) {
                            if (isSelected) {
                                if (!_.find(selection, function(x) { return x.id === listItem.id; })) {
                                    selection.push(listItem);
                                }
                            } else {
                                selection = _.reject(selection, function(x) { return x.id === listItem.id; });
                            }
                        }
                    };

                    newBlade = {
                        id: "CatalogItemsSelect",
                        controller: 'virtoCommerce.catalogModule.catalogItemSelectController',
                        template:
                            'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/common/catalog-items-select.tpl.html',
                        title: "catalog.blades.option-details.title",
                        options: options,
                        headIcon: "fas fa-list",
                        breadcrumbs: [],
                        toolbarCommands: [
                            {
                                name: "platform.commands.confirm",
                                icon: 'fa fa-check',
                                executeMethod: function(pickingBlade) {
                                    var currentSelection = _.map(blade.sectionEntity.options,
                                        function(x) {
                                            return { id: x.productId, productType: x.productType, option: x }
                                        });

                                    currentSelection = _.uniq(_.union(currentSelection, selection),
                                        function(x) {
                                            return [x.productType, x.id].join();
                                        });

                                    blade.sectionEntity.options = _.map(currentSelection,
                                        function(x) {
                                            var option = x.option;
                                            if (!option) {
                                                option = {
                                                    productType: x.productType,
                                                    productId: x.id,
                                                    productName: x.name,
                                                    productImageUrl: x.imageUrl,
                                                    quantity: 1
                                                };
                                            }
                                            return option;
                                        });

                                    bladeNavigationService.closeBlade(pickingBlade);
                                },
                                canExecuteMethod: function() { return _.any(selection); }
                            }
                        ]
                    };
                }

                if (blade.sectionEntity.type === 'Text') {
                    newBlade = {
                        id: 'optionTextDetail',
                        controller: 'virtoCommerce.catalogModule.configurationOptionTextDetailController',
                        template:
                            'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/configurations/option-text-detail.tpl.html',
                        origEntity: {},
                        onSaveNew: function (newOption) {
                            blade.sectionEntity.options.push(newOption);
                        },
                    };
                }

                bladeNavigationService.showBlade(newBlade, blade);
            }

            blade.isLoading = false;
        }]);
