angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.configurationSectionDetailController', ['$scope', 'platformWebApp.bladeNavigationService', 'platformWebApp.uiGridHelper',
        function ($scope, bladeNavigationService, uiGridHelper) {
            var blade = $scope.blade;
            blade.headIcon = 'fas fa-puzzle-piece';
            blade.title = blade.origEntity.name ? blade.origEntity.name : 'catalog.blades.section-details.title';
            blade.formScope = null;

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
                    executeMethod: openOptionSelectorBlade,
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

            $scope.$watch("blade.currentEntity", function () {
                $scope.isValid = blade.formScope && blade.formScope.$valid;
            }, true);
            $scope.$watch("blade.currentEntity.allowCustomText", function (value) {
                if (!value && !blade.currentEntity.allowPredefinedOptions) {
                    blade.currentEntity.allowPredefinedOptions = true;
                }
            }, true);
            $scope.$watch("blade.currentEntity.allowPredefinedOptions", function (value) {
                if (!value && !blade.currentEntity.allowCustomText) {
                    blade.currentEntity.allowCustomText = true;
                }
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

                var newBlade = {
                    id: 'optionDetail',
                    controller: 'virtoCommerce.catalogModule.configurationOptionDetailController',
                    template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/configurations/option-detail.tpl.html',
                    origEntity: item,
                };

                bladeNavigationService.showBlade(newBlade, blade);
            }

            $scope.delete = function (data) {
                deleteList([data]);
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
                return blade.currentEntity.type === 'Product'
                    || blade.currentEntity.type === 'Text'
                    && blade.currentEntity.allowPredefinedOptions;
            }

            function openOptionSelectorBlade() {
                var selection = [];
                var options = {
                    allowCheckingCategory: false,
                    selectedItemIds: [],
                    checkItemFn: function (listItem, isSelected) {
                        if (isSelected) {
                            if (!_.find(selection, function (x) { return x.id === listItem.id; })) {
                                selection.push(listItem);
                            }
                        }
                        else {
                            selection = _.reject(selection, function (x) { return x.id === listItem.id; });
                        }
                    }
                };
                var newBlade = {
                    id: "CatalogItemsSelect",
                    controller: 'virtoCommerce.catalogModule.catalogItemSelectController',
                    template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/common/catalog-items-select.tpl.html',
                    title: "catalog.blades.option-details.title",
                    options: options,
                    headIcon: "fas fa-list",
                    breadcrumbs: [],
                    toolbarCommands: [
                        {
                            name: "platform.commands.confirm", icon: 'fa fa-check',
                            executeMethod: function (pickingBlade) {
                                var currentSelection = _.map(blade.currentEntity.options, function (x) {
                                    return { id: x.productId, productType: x.productType, option: x }
                                });

                                currentSelection = _.uniq(_.union(currentSelection, selection), function (x) {
                                    return [x.productType, x.id].join();
                                });

                                blade.currentEntity.options = _.map(currentSelection, function (x) {
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
                            canExecuteMethod: function () { return _.any(selection); }
                        }]
                };

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
                blade.isLoading = false;
                $scope.sectionTypes = ['Product', 'Text', 'File'];
            }

            initialize(blade.origEntity);
        }]);
