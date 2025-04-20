angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.propertyListController', [
        '$scope', '$localStorage',
        'platformWebApp.bladeNavigationService', 'platformWebApp.authService',
        'virtoCommerce.catalogModule.propDictItems', 'virtoCommerce.catalogModule.measures',
        function ($scope, $localStorage, bladeNavigationService, authService, propDictItems, measures) {
            var blade = $scope.blade;
            $scope.isValid = false;
            blade.refresh = function (entity) {
                if (entity) {
                    initialize(entity);
                }
                else {
                    blade.parentBlade.refresh();
                }
            };

            blade.propertiesVisible = true;
            blade.propertyVisibleCommand = {
                name: 'catalog.blades.property-list.labels.hide-empty-values', icon: 'fas fa-eye-slash',
                executeMethod: function () {
                    $scope.switchPropertiesVisibility();
                },
                canExecuteMethod: function () {
                    return true;
                }
            };

            function initialize(entity) {
                blade.title = entity.name;
                blade.subtitle = 'catalog.blades.property-list.subtitle';
                blade.currentEntity = entity;
                blade.currentEntities = angular.copy(entity.properties);
                blade.propertiesForExclude = _.map(blade.currentEntities, (x) => { return { name: x.name } });
                blade.filteredProperties = [];
                blade.emptyProperties = [];
                //Apply stored filters
                if ($localStorage.propertyFilter) {
                    applyFilter($localStorage.propertyFilter[authService.userName]);
                }

                if (!blade.originalEntity) {
                    blade.originalEntity = {
                        properties: blade.currentEntity.properties
                    }
                }
            }

            $scope.isPropertyChanged = function (property) {
                if (property) {
                    var oldItem = _.find(blade.originalEntity.properties, function (x) { return x.name === property.name; });
                    if (oldItem) {
                        var propValues = property.values.filter(x => x.value)
                            .sort((x, y) => $scope.comparePropValues(x,y));

                        var oldValues = oldItem.values.filter(x => x.value)
                            .sort((x, y) => $scope.comparePropValues(x,y));

                        return !angular.equals(propValues, oldValues);
                    }
                    else {
                        return property.values.length > 0;
                    }
                }
                return false;
            }

            $scope.comparePropValues = function (x, y) {
                if (!x.valueId) {
                    if (x.value.localeCompare !== undefined) {
                        return x.value.localeCompare(y.value);
                    }
                    if (x.value === y.value && x.unitOfMeasureId === y.unitOfMeasureId) {
                        return 0;
                    }
                    return (x.value < y.value || x.unitOfMeasureId !== y.unitOfMeasureId) ? -1 : 1;
                }

                return x.valueId.localeCompare(y.valueId);
            }

            $scope.isPropertyVisible = function (property) {
                if (blade.filteredProperties && blade.filteredProperties.length > 0) {
                    return blade.filteredProperties.includes(property.name.toLowerCase());
                }
                return true;
            }

            $scope.isPropertyHasValues = function (property) {
                return !blade.emptyProperties.includes(property);
            }

            function applyFilter(filteredProperties) {
                if (filteredProperties && filteredProperties.length > 0) {
                    blade.filteredProperties = filteredProperties.map(function (x) { return x.toLowerCase(); });
                }
            }

            $scope.resetFilter = function () {
                saveFilter([]);
                blade.filteredProperties = [];
            };

            $scope.saveChanges = function () {
                blade.currentEntity.properties = blade.currentEntities;
                $scope.bladeClose();
            };

            $scope.getPropertyDisplayName = function (prop) {
                return _.first(_.map(_.filter(prop.displayNames, function (x) { return x && x.languageCode.startsWith(blade.defaultLanguage); }), function (x) { return x.name; }));
            };

            var hasEditPropertyPermission = bladeNavigationService.checkPermission('catalog:metadata-property:edit');
            var hasEditDictionaryPermission = bladeNavigationService.checkPermission('catalog:dictionary-property:edit');
            var hasEditCustomPropertyPermission = bladeNavigationService.checkPermission('catalog:custom-property:edit');

            $scope.canEditProperty = function (prop) {
                return (hasEditPropertyPermission && prop.id)
                    || (hasEditDictionaryPermission && prop.dictionary)
                    || (hasEditCustomPropertyPermission && !prop.id);
            };

            $scope.editProperty = function (prop) {
                if (hasEditPropertyPermission || (hasEditCustomPropertyPermission && !prop.id)) {
                    editProperty(prop);
                } else if (hasEditDictionaryPermission && prop.dictionary) {
                    editDictionary(prop);
                }
            };

            function editUnmanageable(bladeData) {
                var newBlade = {
                    id: 'editItemProperty' + blade.currentEntity.id,
                    properties: blade.currentEntities,
                    productId: blade.currentEntity.id,
                    catalog: blade.catalog,
                    controller: 'virtoCommerce.catalogModule.itemPropertyDetailController',
                    template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/item-property-detail.tpl.html'
                };
                angular.extend(newBlade, bladeData);

                bladeNavigationService.showBlade(newBlade, blade);
            }

            $scope.getPropValues = function (propId, keyword, countToSkip, countToTake) {
                return propDictItems.search({
                    propertyIds: [propId],
                    searchPhrase: keyword,
                    skip: countToSkip,
                    take: countToTake
                }).$promise.then(function (result) {
                    return result;
                });
            };

            $scope.getUnitsOfMeasure = function (measureId) {
                blade.isLoading = true;
                if (!measureId) {
                    blade.isLoading = false;
                    return Promise.resolve([]);
                }

                return measures.getMeasure({ id: measureId }).$promise.then(function (result) {
                    blade.isLoading = false;
                    return result.units;
                });
            };

            var formScope;
            $scope.setForm = function (form) {
                formScope = form;
            }

            var isValid = () => {
                console.log('isValid executed');
                var isFormValid = formScope && formScope.$valid;
                var isAnyChanges = blade.currentEntities.length !== blade.originalEntity.properties.length ||
                    blade.currentEntities.some(x => $scope.isPropertyChanged(x));

                $scope.isValid = isFormValid && isAnyChanges;
            };

            $scope.$watch("blade.currentEntities", isValid, true);
            $scope.$watch("blade.currentEntity", isValid, true);

            blade.headIcon = 'fa fa-gear';

            function setAddPropertyButtonNameKey() {
                if (blade.currentEntity.hasOwnProperty('productType')) {
                    return blade.currentEntity.mainProductId ? 'catalog.commands.add-variation-property' : 'catalog.commands.add-product-property';
                }
                return 'catalog.commands.add-property';
            }

            blade.addPropertyButtonNameKey = setAddPropertyButtonNameKey();
            blade.toolbarCommands = [
                {
                    name: "catalog.blades.property-list.labels.add-filter", icon: 'fa fa-filter',
                    executeMethod: function () {
                        $scope.editPropertyFilter();
                    },
                    canExecuteMethod: function () {
                        return true;
                    }
                },
                {
                    name: "catalog.blades.property-list.labels.reset-filter", icon: 'fa fa-undo',
                    executeMethod: function () {
                        $scope.resetFilter();
                    },
                    canExecuteMethod: function () {
                        return blade.filteredProperties.length > 0;
                    }
                },
                blade.propertyVisibleCommand
            ];

            if ((blade.entityType !== "product" && hasEditPropertyPermission) ||
                (blade.entityType === "product" && hasEditCustomPropertyPermission)) {
                blade.toolbarCommands.splice(0, 0, {
                    name: blade.addPropertyButtonNameKey,
                    icon: 'fas fa-plus',
                    executeMethod: function () {
                        if (blade.entityType == "product") {
                            editUnmanageable({
                                isNew: true,
                                title: 'catalog.blades.item-property-detail.title-new',
                                origEntity: {
                                    type: "Product",
                                    valueType: "ShortText",
                                    values: []
                                }
                            });
                        } else {
                            var newBlade = {
                                id: 'propertyTypesList',
                                currentEntityId: undefined,
                                categoryId: blade.categoryId,
                                catalogId: blade.catalogId,
                                defaultLanguage: blade.defaultLanguage,
                                languages: blade.languages,
                                controller: 'virtoCommerce.catalogModule.propertyTypeListController',
                                template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/property-type-list.tpl.html'
                            };
                            bladeNavigationService.showBlade(newBlade, blade);
                        }
                    },
                    canExecuteMethod: function () {
                        return true;
                    }
                });
            }
            $scope.editPropertyFilter = function () {
                var newBlade = {
                    id: "propertySelector",
                    entityType: "product",
                    properties: blade.currentEntities,
                    selectedProperties: blade.filteredProperties,
                    controller: 'virtoCommerce.catalogModule.propertySelectorController',
                    template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/property-selector.tpl.html',
                    onSelected: function (filteredProperties) {
                        var filteredPropertiesNames = filteredProperties.map(function (x) { return x.name; });
                        saveFilter(filteredPropertiesNames);
                        applyFilter(filteredPropertiesNames);
                    }
                };
                bladeNavigationService.showBlade(newBlade, blade);
            };

            $scope.switchPropertiesVisibility = function () {
                blade.propertiesVisible = !blade.propertiesVisible;

                if (blade.propertiesVisible) {
                    blade.propertyVisibleCommand.name = 'catalog.blades.property-list.labels.hide-empty-values';
                    blade.propertyVisibleCommand.icon = 'fas fa-eye-slash';

                    showEmptyProperties();
                }
                else {
                    blade.propertyVisibleCommand.name = 'catalog.blades.property-list.labels.show-empty-values';
                    blade.propertyVisibleCommand.icon = 'fas fa-eye';

                    hideEmptyProperties();
                }
            };

            function editProperty(prop) {
                if (prop.isManageable) {
                    var newBlade = {
                        id: 'editCategoryProperty',
                        currentEntityId: prop ? prop.id : undefined,
                        categoryId: blade.categoryId,
                        catalogId: blade.catalogId,
                        defaultLanguage: blade.defaultLanguage,
                        languages: blade.languages,
                        controller: 'virtoCommerce.catalogModule.propertyDetailController',
                        template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/property-detail.tpl.html'
                    };
                    bladeNavigationService.showBlade(newBlade, blade);
                } else {
                    editUnmanageable({
                        title: 'catalog.blades.item-property-detail.title',
                        origEntity: prop
                    });
                }
            }

            function editDictionary(prop) {
                var newBlade = { id: "propertyChild" };
                newBlade.property = prop;
                newBlade.languages = blade.languages;
                newBlade.defaultLanguage = blade.defaultLanguage;
                newBlade.title = 'catalog.blades.property-dictionary.title';
                newBlade.titleValues =
                    { name: prop.name };
                newBlade.subtitle = 'catalog.blades.property-dictionary.subtitle';
                newBlade.controller = 'virtoCommerce.catalogModule.propertyDictionaryListController';
                newBlade.template = 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/property-dictionary-list.tpl.html';
                bladeNavigationService.showBlade(newBlade, blade);
            }

            function hideEmptyProperties() {
                var propertiesByType = _.filter(blade.currentEntities, function (property) {
                    if (blade.entityType.toLowerCase() === 'product' && property.type.toLowerCase() === 'variation') {
                        return true;
                    }

                    return property.type.toLowerCase() === blade.entityType.toLowerCase();
                });

                // control visibility of multilanguage properties separately
                _.each(propertiesByType, function (property) {
                    if (property.multilanguage) {
                        property.$$hiddenLanguages = [];
                        _.each(blade.languages, function (language) {
                            var languageFound = _.some(property.values, function (propertyValue) {
                                return propertyValue.value && propertyValue.value !== '' && propertyValue.languageCode === language;
                            });

                            if (!languageFound) {
                                property.$$hiddenLanguages.push(language);
                            }
                        });
                    }
                })

                _.each(propertiesByType, function (property) {
                    // required properties and switchers can’t be hidden
                    if (!property.required &&
                        property.valueType !== 'Boolean' &&
                        allPropertiesEmpty(property.values)
                    ) {
                        blade.emptyProperties.push(property)
                    }
                });
            }

            function allPropertiesEmpty(propertyValues) {
                var result = _.all(propertyValues, function (value) {
                    return !value.value || value.value === '';
                });
                return result;
            }

            function showEmptyProperties() {
                _.each(blade.currentEntities, function (property) {
                    if (property.$$hiddenLanguages) {
                        property.$$hiddenLanguages = null;
                    }
                });

                blade.emptyProperties = [];
            }

            //save filters to localStorage
            function saveFilter(filteredPropertiesNames) {
                var filter = {};
                filter[authService.userName] = filteredPropertiesNames;
                if ($localStorage.propertyFilter) {
                    angular.extend($localStorage.propertyFilter, filter);
                } else {
                    $localStorage.propertyFilter = filter;
                }
            }

            blade.isLoading = false;
            initialize(blade.currentEntity);
        }
    ]);
