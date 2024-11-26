angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.configurationSectionDetailController', ['$scope', 'platformWebApp.bladeNavigationService', 'platformWebApp.uiGridHelper',
        function ($scope, bladeNavigationService, uiGridHelper) {
            var blade = $scope.blade;
            blade.headIcon = 'fas fa-puzzle-piece';
            blade.formScope = null;
            blade.toolbarCommands = [
                {
                    name: "platform.commands.reset",
                    icon: 'fa fa-undo',
                    executeMethod: function () { angular.copy(blade.origEntity, blade.currentEntity); },
                    canExecuteMethod: isDirty,
                    permission: 'configurations:update'
                },
                {
                    name: "catalog.blades.section-details.commands.add",
                    icon: 'fas fa-plus',
                    executeMethod: openOptionSelectorBlade,
                    canExecuteMethod: function () { return true; },
                    permission: 'configurations:update'
                },
                {
                    name: "platform.commands.delete",
                    icon: 'fas fa-trash-alt',
                    executeMethod: function () { deleteList($scope.gridApi.selection.getSelectedRows()); },
                    canExecuteMethod: isItemsChecked,
                    permission: 'configurations:delete'
                }
            ];

            $scope.isValid = false;

            $scope.$watch("blade.currentEntity", function () {
                $scope.isValid = blade.formScope && blade.formScope.$valid;
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
                    itemId: item.productId,
                    productType: item.productType,
                    controller: 'virtoCommerce.catalogModule.itemDetailController',
                    template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/item-detail.tpl.html'
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
                    title: item.productName,
                    origEntity: item,
                    controller: 'virtoCommerce.catalogModule.configurationOptionDetailController',
                    template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/configurations/option-detail.tpl.html'
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
                    title: "catalog.blades.option-details.title",
                    controller: 'virtoCommerce.catalogModule.catalogItemSelectController',
                    template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/common/catalog-items-select.tpl.html',
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
                blade.currentEntity = angular.copy(item);
                blade.isLoading = false;
            }

            initialize(blade.origEntity);
        }]);
