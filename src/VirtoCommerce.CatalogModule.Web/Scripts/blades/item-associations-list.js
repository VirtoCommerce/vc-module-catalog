angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.itemAssociationsListController', ['$scope', 'platformWebApp.bladeNavigationService', 'uiGridConstants', 'platformWebApp.uiGridHelper', 'virtoCommerce.catalogModule.catalogs', 'virtoCommerce.catalogModule.items', 'virtoCommerce.catalogModule.categories', function ($scope, bladeNavigationService, uiGridConstants, uiGridHelper, catalogs, items, categories) {
        $scope.uiGridConstants = uiGridConstants;
        var allCatalogIds = [];
        var blade = $scope.blade;
        blade.subtitle = 'catalog.widgets.itemAssociations.blade-subtitle';

        blade.isLoading = false;
        blade.refresh = function (item) {
            initialize(item);
        };

        function initialize(item) {
            blade.title = item.name;
            blade.item = item;
            populateProductCatalogs(item.associations);
        }

        function populateProductCatalogs(associations) {
            if (!_.some(associations)) {
                return;
            }

            blade.isLoading = true;

            allCatalogIds = [];
            processProductsAssociations(associations)
                .then(processCategoryAssociations(associations))
                .then(findCatalogs);
        }

        function processProductsAssociations(associations) {
            var productAssociations = _.filter(associations, function (association) { return association.associatedObjectType === 'product' });
            var itemIds = _.pluck(productAssociations, 'associatedObjectId');
            return items.plenty({ respGroup: 'ItemSmall' }, itemIds, function (data) {
                addAssociationProperties(productAssociations, data);
            }).$promise;
        }

        function processCategoryAssociations(associations) {
            var categoryAssociations = _.filter(associations, function (association) { return association.associatedObjectType !== 'product' });
            var categoryIds = _.pluck(categoryAssociations, 'associatedObjectId');
            return categories.plenty({ respGroup: 'Info' }, categoryIds, function (data) {
                addAssociationProperties(categoryAssociations, data);
            }).$promise;
        }

        function findCatalogs() {
            var uniqueCatalogIds = _.uniq(allCatalogIds);
            return catalogs.search({
                skip: 0,
                take: uniqueCatalogIds.length,
                catalogIds: uniqueCatalogIds
            }, function (data) {
                _.each(blade.item.associations, function (association) {
                    association.$$catalog = _.find(data.results, function (x) { return x.id === association.$$catalogId; });
                });
                blade.isLoading = false;
            }).$promise;
        }

        function addAssociationProperties(associations, data) {
            _.each(associations, function (x) {
                var item = _.find(data, function (d) { return d.id === x.associatedObjectId; });
                if (item) {
                    x.$$catalogId = item.catalogId;

                    if (x.associatedObjectType === 'product') {
                        x.$$productType = item.productType;
                    }
                }
            });
            allCatalogIds.push.apply(allCatalogIds, _.pluck(associations, '$$catalogId'));
        }

        $scope.selectNode = function (listItem) {
            $scope.selectedNodeId = listItem.associatedObjectId;
            var newBlade = {
                id: 'associationDetail',
                itemId: listItem.associatedObjectId,
                catalog: blade.catalog,
                controller: 'virtoCommerce.catalogModule.itemDetailController',
                template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/item-detail.tpl.html'
            };
            if (listItem.associatedObjectType === 'category') {
                newBlade.currentEntityId = listItem.associatedObjectId;
                newBlade.controller = 'virtoCommerce.catalogModule.categoryDetailController';
                newBlade.template = 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/category-detail.tpl.html';
            }
            if (listItem.associatedObjectType === 'product') {
                newBlade.productType = listItem.$$productType;
            }

            bladeNavigationService.showBlade(newBlade, blade);
        };

        $scope.deleteList = function (list) {
            bladeNavigationService.closeChildrenBlades(blade,
                function () {
                    var undeletedEntries = _.difference(blade.item.associations, list);
                    blade.item.associations = undeletedEntries;
                });
        };

        $scope.edit = function (listItem) {
            var newBlade = {
                id: 'associationEditDetail',
                title: listItem.associatedObjectName,
                subtitle: 'catalog.blades.item-association-detail.subtitle',
                origEntity: listItem,
                catalog: blade.catalog,
                controller: 'virtoCommerce.catalogModule.itemAssociationDetailController',
                template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/item-association-detail.tpl.html'
            };
            bladeNavigationService.showBlade(newBlade, blade);
        };

        function openAddEntityWizard() {
            var newBlade = {
                id: "associationWizard",
                catalog: blade.catalog,
                item: blade.item,
                onSaveChanges: function () {
                    populateProductCatalogs(blade.item.associations);
                },
                controller: 'virtoCommerce.catalogModule.associationWizardController',
                template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/wizards/newAssociation/association-wizard.tpl.html'
            };
            bladeNavigationService.showBlade(newBlade, blade);
        }

        blade.toolbarCommands = [
            {
                name: "platform.commands.add", icon: 'fas fa-plus',
                executeMethod: function () {
                    openAddEntityWizard();
                },
                canExecuteMethod: function () {
                    return true;
                }
            },
            {
                name: "platform.commands.delete", icon: 'fas fa-trash-alt',
                executeMethod: function () {
                    $scope.deleteList($scope.gridApi.selection.getSelectedRows());
                },
                canExecuteMethod: function () {
                    return $scope.gridApi && _.any($scope.gridApi.selection.getSelectedRows());
                }
            }
        ];

        // ui-grid
        $scope.setGridOptions = function (gridOptions) {
            uiGridHelper.initialize($scope, gridOptions, function (gridApi) {
                //update gridApi for current grid
                $scope.gridApi = gridApi;

                gridApi.draggableRows.on.rowFinishDrag($scope, function () {
                    for (var i = 0; i < blade.item.associations.length; i++) {
                        blade.item.associations[i].priority = i + 1;
                    }
                });
            });
        };

        initialize(blade.item);
    }]);
