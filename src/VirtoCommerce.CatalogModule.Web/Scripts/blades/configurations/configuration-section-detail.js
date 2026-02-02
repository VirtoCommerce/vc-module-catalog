angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.configurationSectionDetailController',
        ['$scope', 'platformWebApp.bladeNavigationService', 'platformWebApp.uiGridHelper', 'platformWebApp.metaFormsService',
        function ($scope, bladeNavigationService, uiGridHelper, metaFormsService) {
            var blade = $scope.blade;
            blade.headIcon = 'fas fa-puzzle-piece';
            blade.title = blade.origEntity.name ? blade.origEntity.name : 'catalog.blades.section-details.title';
            blade.formScope = null;

            // Get metafields for the form
            blade.metaFields = metaFormsService.getMetaFields("configurationSectionDetail");

            // Section types for the dropdown
            blade.sectionTypes = ['Product', 'Variation', 'Text', 'File'];

            blade.toolbarCommands = [
                {
                    name: "platform.commands.reset",
                    icon: 'fa fa-undo',
                    executeMethod: function () { angular.copy(blade.origEntity, blade.currentEntity); },
                    canExecuteMethod: isDirty,
                    permission: 'catalog:configurations:update'
                },
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

            $scope.isValid = false;

            // Single $watch for currentEntity that handles all validation and mutual exclusivity logic
            var previousAllowCustomText = null;
            var previousAllowPredefinedOptions = null;

            $scope.$watch("blade.currentEntity", function (entity, oldEntity) {
                if (!entity) {
                    return;
                }

                // Form validation
                $scope.isValid = blade.formScope && blade.formScope.$valid && entity.name;
                if ($scope.isValid && blade.origEntity.name) {
                    // Update case (form is valid when changes exist)
                    $scope.isValid = !angular.equals(blade.origEntity, entity);
                }

                // Mutual exclusivity logic for allowCustomText and allowPredefinedOptions
                // At least one must be true when type is 'Text'
                if (entity.type === 'Text') {
                    // Check if allowCustomText changed from true to false
                    if (previousAllowCustomText === true && entity.allowCustomText === false) {
                        if (!entity.allowPredefinedOptions) {
                            entity.allowPredefinedOptions = true;
                        }
                    }
                    // Check if allowPredefinedOptions changed from true to false
                    else if (previousAllowPredefinedOptions === true && entity.allowPredefinedOptions === false) {
                        if (!entity.allowCustomText) {
                            entity.allowCustomText = true;
                        }
                    }
                }

                // Store current values for next comparison
                previousAllowCustomText = entity.allowCustomText;
                previousAllowPredefinedOptions = entity.allowPredefinedOptions;
            }, true);

            $scope.setForm = function (form) { blade.formScope = form; };

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

            $scope.saveChanges = function () {
                var isNew = !blade.origEntity.name;
                angular.copy(blade.currentEntity, blade.origEntity);

                if (isNew && angular.isFunction(blade.onSaveNew)) {
                    blade.onSaveNew(blade.origEntity);
                }

                $scope.bladeClose();
            };

            $scope.edit = function (item) {
                $scope.selectedNodeId = item.productId;

                if (blade.currentEntity.type === 'Product' || blade.currentEntity.type === 'Text') {
                    var newBlade = {};

                    if (blade.currentEntity.type === 'Product') {
                        newBlade = {
                            id: 'optionProductDetail',
                            controller: 'virtoCommerce.catalogModule.configurationOptionProductDetailController',
                            template:
                                'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/configurations/option-product-detail.tpl.html',
                            origEntity: item,
                        };
                    }

                    if (blade.currentEntity.type === 'Text') {
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
            }

            $scope.delete = function (data) {
                deleteList([data]);
            };

            // Function exposed on blade for template access
            blade.isTypeChangeDisabled = function() {
                if (blade.currentEntity == null || blade.currentEntity.type == null) {
                    return false;
                }

                if (blade.currentEntity.id == null && blade.currentEntity.type === 'File') {
                    return false;
                }

                if (blade.currentEntity.id == null && !_.any(blade.currentEntity.options)) {
                    return false;
                }

                return true;
            };

            function deleteList(list) {
                bladeNavigationService.closeChildrenBlades(blade,
                    function () {
                        var undeletedEntries = _.difference(blade.currentEntity.options, list);
                        blade.currentEntity.options = undeletedEntries;
                    });
            }

            function isItemsChecked() {
                return $scope.gridApi && _.any($scope.gridApi.selection.getSelectedRows());
            }

            function isDirty() {
                return !angular.equals(blade.currentEntity, blade.origEntity);
            }

            function canAddOptions() {
                return blade.currentEntity.type === 'Product' ||
                    blade.currentEntity.type === 'Text' && blade.currentEntity.allowPredefinedOptions;
            }

            function openOptionAddBlade() {
                var newBlade = {};

                if (blade.currentEntity.type === 'Product') {
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
                                    var currentSelection = _.map(blade.currentEntity.options,
                                        function(x) {
                                            return { id: x.productId, productType: x.productType, option: x }
                                        });

                                    currentSelection = _.uniq(_.union(currentSelection, selection),
                                        function(x) {
                                            return [x.productType, x.id].join();
                                        });

                                    blade.currentEntity.options = _.map(currentSelection,
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

                if (blade.currentEntity.type === 'Text') {
                    newBlade = {
                        id: 'optionTextDetail',
                        controller: 'virtoCommerce.catalogModule.configurationOptionTextDetailController',
                        template:
                            'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/configurations/option-text-detail.tpl.html',
                        origEntity: {},
                        onSaveNew: function (newOption) {
                            blade.currentEntity.options.push(newOption);
                        },
                    };
                }

                bladeNavigationService.showBlade(newBlade, blade);
            }

            function initialize(item) {
                if (item.options == null) {
                    item.options = [];
                }

                if (item.id == null) {
                    item.allowCustomText = true;
                }

                blade.currentEntity = angular.copy(item);
                
                // Initialize previous values for mutual exclusivity tracking
                previousAllowCustomText = blade.currentEntity.allowCustomText;
                previousAllowPredefinedOptions = blade.currentEntity.allowPredefinedOptions;
                
                blade.isLoading = false;
            }

            initialize(blade.origEntity);
        }]);
