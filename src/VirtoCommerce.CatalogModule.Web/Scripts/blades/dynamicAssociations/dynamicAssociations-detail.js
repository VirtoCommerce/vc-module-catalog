angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.dynamicAssociationDetailController', ['$scope', 'platformWebApp.bladeNavigationService', 'virtoCommerce.catalogModule.dynamicAssociations', 'virtoCommerce.catalogModule.categories', 'virtoCommerce.storeModule.stores', function ($scope, bladeNavigationService, associations, categories, stores) {
        var blade = $scope.blade;
        var parametersBlade = null;
        var formScope;
        $scope.setForm = (form) => { formScope = form; };

        blade.selectedCategoryToMatchIds = [];
        blade.selectedCategoryToDisplayIds = [];

        blade.filteredPropertiesToMatch = [];
        blade.filteredPropertiesToDisplay = [];

        blade.editedPropertiesToMatch = [];
        blade.editedPropertiesToDisplay = [];

        blade.toMatchIsActive = true;
        blade.currentEntity = {};

        blade.updatePermission = 'catalog:update';

        blade.conditionPropertyValues = {
            id: 'ConditionPropertyValues',
            propertyNameValues: {}
        };

        blade.conditionCategoryIs = {
            id: 'ConditionCategoryIs',
            categoryIds: [],
            categoryNames: []
        };

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
            blade.isLoading = false;
        }

        $scope.isDirty = () => {
            return !angular.equals(blade.currentEntity, blade.origEntity) && blade.hasUpdatePermission();
        };

        $scope.canSave = () => {
            return $scope.isDirty() && formScope && formScope.$valid && parametersBlade && angular.isFunction(parametersBlade.isValid) && parametersBlade.isValid();
        };

        $scope.mainParameters = function() {
            parametersBlade = {
                id: "mainParameters",
                title: "catalog.blades.dynamicAssociation-parameters.title",
                subtitle: 'catalog.blades.dynamicAssociation-parameters.subtitle',
                controller: 'virtoCommerce.catalogModule.dynamicAssociationParametersController',
                template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/dynamicAssociations/mainParameters.tpl.html',
                currentEntity: blade.currentEntity
        };
            bladeNavigationService.showBlade(parametersBlade, blade);
        };

        $scope.cancelChanges = function () {
            angular.copy(blade.origEntity, blade.currentEntity);
            $scope.bladeClose();
        };
        $scope.saveChanges = function () {
            blade.isLoading = true;

            blade.conditionCategoryIs.categoryIds.push(blade.selectedCategoryToMatchIds);
            blade.conditionPropertyValues.properties.push(blade.editedPropertiesToMatch);


            let blockMatchingRules = _.find(blade.currentEntity.expressionTree.children, x => x.id === 'BlockMatchingRules');
            if (blockMatchingRules) {
                blockMatchingRules.children.push(blade.conditionCategoryIs);
                blockMatchingRules.children.push(blade.conditionPropertyValues);
            }

            associations.save({}, [blade.currentEntity], (data) => {
                if (data && data.length > 0) {
                    //close main parameters blade
                    parametersBlade && bladeNavigationService.closeBlade(parametersBlade);
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
        $scope.createProductFilter = function (categoryIds, toMatchIsActive) {
            blade.toMatchIsActive = toMatchIsActive;
            var allProperties = [];
            var options = {
                showCheckingMultiple: false,
                allowCheckingCategory: true,
                allowCheckingItem: false,
                selectedItemIds: categoryIds,
                checkItemFn: (listItem, isSelected) => {
                    if (isSelected) {
                        if (!_.find(categoryIds, (x) =>  x === listItem.id )) {
                            categoryIds.push(listItem.id);
                        }
                    }
                    else {
                        categoryIds = _.reject(categoryIds, (x) => x === listItem.id);
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

                            categories.getByIds({ ids: categoryIds },
                                data => {
                                    allProperties = _.unique(_.first(data.map(x => x.properties)));
                                    _.each(allProperties, prop => {
                                        prop.group = 'All properties';
                                        prop.UseDefaultUIForEdit = true;
                                        prop.values = [];
                                        prop.isReadOnly = false;
                                    });
                                    $scope.selectProperties(allProperties);
                                });
                        },
                        canExecuteMethod: () => _.any(categoryIds)
                    },
                    {
                        name: "platform.commands.reset", icon: 'fa fa-undo',
                        executeMethod: function (pickingBlade) {
                            categoryIds = [];
                            $scope.selectedCount = 0;
                            bladeNavigationService.closeBlade(pickingBlade);
                        },
                        canExecuteMethod: () => _.any(categoryIds)
                        
                    }]
            };
            bladeNavigationService.showBlade(newBlade, blade);

        }; 

        $scope.selectProperties = function (allProperties) {
            let filteredProps = blade.toMatchIsActive
                ? blade.filteredPropertiesToMatch
                : blade.filteredPropertiesToDisplay;
            var newBlade = {
                id: 'propertiesSelector',
                controller: 'virtoCommerce.catalogModule.propertiesSelectorController',
                template: 'Modules/$(virtoCommerce.catalog)/Scripts/blades/step-select-properties.tpl.html',
                properties: allProperties,
                includedProperties: filteredProps,
                onSelected: function (includedProperties) {
                    if (blade.toMatchIsActive) {
                        blade.filteredPropertiesToMatch = includedProperties;
                    } else {
                        blade.filteredPropertiesToDisplay = includedProperties;
                    }
                    blade.isPropertiesSelected = true;
                    $scope.editProperties();
                }
            };

            bladeNavigationService.showBlade(newBlade, blade);
        };

        $scope.editProperties = function () {
            let filteredProps = blade.toMatchIsActive
                ? blade.filteredPropertiesToMatch
                : blade.filteredPropertiesToDisplay;

            var newBlade = {
                id: 'propertiesEditor',
                controller: 'virtoCommerce.catalogModule.editPropertiesActionStepController',
                template: 'Modules/$(virtoCommerce.catalog)/Scripts/blades/step-edit-properties.tpl.html',
                properties: filteredProps,
                propGroups: [{ title: 'catalog.properties.product', type: 'Product' }, { title: 'catalog.properties.variation', type: 'Variation' }],
                onSelected: function (editedProps) {
                    if (blade.toMatchIsActive) {
                        blade.editedPropertiesToMatch = editedProps;
                    } else {
                        blade.editedPropertiesToDisplay = editedProps;
                    }

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
