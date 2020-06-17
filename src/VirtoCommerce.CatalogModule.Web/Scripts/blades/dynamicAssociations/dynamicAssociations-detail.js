angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.dynamicAssociationDetailController', ['$scope', 'platformWebApp.bladeNavigationService', 'virtoCommerce.catalogModule.dynamicAssociations', 'virtoCommerce.catalogModule.categories', 'virtoCommerce.storeModule.stores', function ($scope, bladeNavigationService, associations, categories, stores) {
        var blade = $scope.blade;
        var formScope;
        $scope.setForm = (form) => { formScope = form; };

        $scope.BlockMatchingRules = 'BlockMatchingRules';
        $scope.BlockResultingRules = 'BlockResultingRules';
        $scope.ConditionPropertyValues = 'ConditionPropertyValues';
        $scope.ConditionCategoryIs = 'ConditionCategoryIs';

        blade.currentEntity = {};

        blade.updatePermission = 'catalog:update';
        blade.isMatchingRulesExist = false;
        blade.isResultingRulesExist = false;


        blade.refresh = function(parentRefresh) {
            if (blade.isNew) {
                associations.new({},
                    data => {
                        blade.currentEntity = data;
                        initializeBlade(blade.currentEntity);
                    });
            } else {
                associations.get({ id: blade.currentEntityId }, (data) => {
                    initializeBlade(data);

                     if (parentRefresh) {
                         blade.parentBlade.refresh();
                     }

                    if (blade.currentEntity.storeId) {
                        //Need to pre filter catalog-category selector
                        stores.get({ id: blade.currentEntity.storeId }, data => {
                            blade.currentEntity.catalogId = blade.origEntity.catalogId = data.catalog;
                        });
                     }
                });
            }
        };

        function initializeBlade(data) {
            if (!blade.isNew) {
                blade.title = data.name;
            }
            blade.currentEntity = angular.copy(data);
            blade.origEntity = data;

            $scope.checkExistingRules();
            blade.isLoading = false;
        }

        $scope.checkExistingRules = function() {
            const matchingRules = _.find(blade.currentEntity.expressionTree.children, x => x.id === $scope.BlockMatchingRules);
            const matchingCondition = $scope.getCondition(matchingRules, $scope.ConditionPropertyValues);
            blade.isMatchingRulesExist = matchingCondition.properties.length > 0;

            const resultingRules = _.find(blade.currentEntity.expressionTree.children, x => x.id === $scope.BlockResultingRules);
            const resultingCondition = $scope.getCondition(resultingRules, $scope.ConditionPropertyValues);
            blade.isResultingRulesExist = resultingCondition.properties.length > 0;
        };

        $scope.isDirty = () => {
            return !angular.equals(blade.currentEntity, blade.origEntity) && blade.hasUpdatePermission();
        };

        $scope.canSave = () => {
            return $scope.isDirty() && formScope && formScope.$valid && blade.currentEntity.storeId && blade.currentEntity.associationType && blade.currentEntity.priority && blade.isMatchingRulesExist && blade.isResultingRulesExist;
        };

        $scope.mainParameters = function() {
            const parametersBlade = {
                id: "mainParameters",
                title: "catalog.blades.dynamicAssociation-parameters.title",
                subtitle: 'catalog.blades.dynamicAssociation-parameters.subtitle',
                controller: 'virtoCommerce.catalogModule.dynamicAssociationParametersController',
                template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/dynamicAssociations/mainParameters.tpl.html',
                originalEntity: blade.currentEntity,
                onSelected: function (entity) {
                    blade.currentEntity = entity;
                }

        };
            bladeNavigationService.showBlade(parametersBlade, blade);
        };

        $scope.cancelChanges = function () {
            angular.copy(blade.origEntity, blade.currentEntity);
            $scope.bladeClose();
        };

        $scope.saveChanges = function () {
            blade.isLoading = true;

            associations.save({}, [blade.currentEntity], (data) => {
                if (data && data.length > 0) {
                    blade.isNew = undefined;
                    blade.currentEntityId = data[0].id;
                    initializeBlade(data[0]);
                    initializeToolbar();
                    blade.refresh(true);
                } else {
                    bladeNavigationService.setError('Error while saving association rule' , blade);
                }
            });
        };

        blade.onClose = function (closeCallback) {
            bladeNavigationService.showConfirmationIfNeeded($scope.isDirty(), $scope.canSave(), blade, $scope.saveChanges, closeCallback, "catalog.dialogs.catalog-save.title", "catalog.dialogs.catalog-save.message");
        };

        function initializeToolbar() {
            if (!blade.isNew) {
                blade.toolbarCommands = [
                    {
                        name: "platform.commands.save", icon: 'fa fa-save',
                        executeMethod: function () {
                            $scope.saveChanges();
                        },
                        canExecuteMethod: $scope.canSave,
                        permission: blade.updatePermission
                    },
                    {
                        name: "platform.commands.reset", icon: 'fa fa-undo',
                        executeMethod: function () {
                            angular.copy(blade.origEntity, blade.currentEntity);
                        },
                        canExecuteMethod: $scope.isDirty,
                        permission: blade.updatePermission
                    }
                ];
            }
        }
        // for ui label - UI 3 of 5 properties filled
        $scope.$watch('blade.currentEntity', (data) => {
            if (data) {
                $scope.totalPropertiesCount = 5;
                $scope.filledPropertiesCount = 0;

                $scope.filledPropertiesCount += blade.currentEntity.startDate ? 1 : 0;
                $scope.filledPropertiesCount += blade.currentEntity.endDate ? 1 : 0;
                $scope.filledPropertiesCount += blade.currentEntity.storeId ? 1 : 0;
                $scope.filledPropertiesCount += blade.currentEntity.associationType ? 1 : 0;
                $scope.filledPropertiesCount += blade.currentEntity.priority ? 1 : 0;
            }
        }, true);

        /////
        $scope.getCondition = function(rulesBlock, conditionName) {
            const conditionCategoryIs = { id: $scope.ConditionCategoryIs, categoryIds: [], categoryNames: [] };
            const conditionPropertyValues = { id: $scope.ConditionPropertyValues, properties: [] };

            let categoryCondition = _.find(rulesBlock.children, x => x.id === conditionName);
            if (!categoryCondition) {
                switch (conditionName) {
                    case $scope.ConditionCategoryIs:
                        rulesBlock.children.push(conditionCategoryIs);
                        break;

                    case $scope.ConditionPropertyValues:
                        rulesBlock.children.push(conditionPropertyValues);
                        break;
                }
                
            }
            categoryCondition = _.find(rulesBlock.children, x => x.id === conditionName);
            return categoryCondition;
        };


        $scope.createProductFilter = function (rulesBlockName) {
            var allProperties = [];

            const rulesBlock = _.find(blade.currentEntity.expressionTree.children, x => x.id === rulesBlockName);
            let categoryCondition = $scope.getCondition(rulesBlock, $scope.ConditionCategoryIs);

            var options = {
                showCheckingMultiple: false,
                allowCheckingCategory: true,
                allowCheckingItem: false,
                selectedItemIds: categoryCondition.categoryIds,
                checkItemFn: (listItem, isSelected) => {
                    if (isSelected) {
                        if (!_.find(categoryCondition.categoryIds, (x) =>  x === listItem.id )) {
                            categoryCondition.categoryIds.push(listItem.id);
                        }
                    }
                    else {
                        categoryCondition.categoryIds = _.reject(categoryCondition.categoryIds, (x) => x === listItem.id);
                    }
                }
            };
            
            var newBlade = {
                id: "CatalogItemsSelect",
                controller: 'virtoCommerce.catalogModule.catalogItemSelectController',
                template: 'Modules/$(virtoCommerce.catalog)/Scripts/blades/common/catalog-items-select.tpl.html',
                title: 'catalog.selectors.blades.titles.select-categories',
                options: options,
                breadcrumbs: [],
                catalogId: blade.currentEntity.catalogId,
                toolbarCommands: [
                    {
                        name: "platform.commands.confirm", icon: 'fa fa-check',
                        executeMethod: function (pickingBlade) {
                            bladeNavigationService.closeBlade(pickingBlade);

                            categories.getByIds({ ids: categoryCondition.categoryIds },
                                data => {
                                    allProperties = _.unique(_.first(data.map(x => x.properties)));
                                    _.each(allProperties, prop => {
                                        prop.group = 'All properties';
                                        prop.UseDefaultUIForEdit = true;
                                        prop.values = [];
                                        prop.isReadOnly = false;
                                    });
                                    $scope.selectProperties(allProperties, rulesBlock);
                                });
                        },
                        canExecuteMethod: () => _.any(categoryCondition.categoryIds)
                    },
                    {
                        name: "platform.commands.reset", icon: 'fa fa-undo',
                        executeMethod: function (pickingBlade) {
                            categoryCondition.categoryIds = [];
                            $scope.selectedCount = 0;
                            bladeNavigationService.closeBlade(pickingBlade);
                        },
                        canExecuteMethod: () => _.any(categoryCondition.categoryIds)
                        
                    }]
            };
            bladeNavigationService.showBlade(newBlade, blade);

        }; 

        $scope.selectProperties = function (allProperties, rulesBlock) {
            let propertyCondition = $scope.getCondition(rulesBlock, $scope.ConditionPropertyValues);

            let selectedProperties = [];
            angular.copy(propertyCondition.properties, selectedProperties);
            _.each(selectedProperties, prop => {
                prop.group = 'All properties';
                prop.UseDefaultUIForEdit = true;
                prop.isReadOnly = false;
            });

            var newBlade = {
                id: 'propertiesSelector',
                controller: 'virtoCommerce.catalogModule.propertiesSelectorController',
                template: 'Modules/$(virtoCommerce.catalog)/Scripts/blades/step-select-properties.tpl.html',
                properties: allProperties,
                includedProperties: selectedProperties,
                onSelected: function (includedProperties) {
                    propertyCondition.properties = includedProperties;
                    blade.isPropertiesSelected = true;
                    $scope.editProperties(rulesBlock);
                }
            };

            bladeNavigationService.showBlade(newBlade, blade);
        };

        $scope.editProperties = function (rulesBlock) {
            let propertyCondition = $scope.getCondition(rulesBlock, $scope.ConditionPropertyValues);

            var newBlade = {
                id: 'propertiesEditor',
                controller: 'virtoCommerce.catalogModule.editPropertiesActionStepController',
                template: 'Modules/$(virtoCommerce.catalog)/Scripts/blades/step-edit-properties.tpl.html',
                properties: propertyCondition.properties,
                propGroups: [{ title: 'catalog.properties.product', type: 'Product' }, { title: 'catalog.properties.variation', type: 'Variation' }],
                onSelected: function (editedProps) {
                    propertyCondition.properties = editedProps;
                    $scope.checkExistingRules();
                },
                toolbarCommands: [
                    {
                        name: "platform.commands.preview", icon: 'fa fa-filter',
                        executeMethod: (pickingBlade) => {
                            //ToDo: show blade with filtered products
                            bladeNavigationService.closeBlade(pickingBlade);
                        },
                        canExecuteMethod: () => true
                    }]

            };
            bladeNavigationService.showBlade(newBlade, blade);
        };

        initializeToolbar();
        blade.refresh(false);
    }]);
