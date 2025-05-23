angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.propertyDetailController', [
        '$scope', '$q', '$timeout',
        'platformWebApp.bladeNavigationService', 'platformWebApp.dialogService',
        'virtoCommerce.catalogModule.properties', 'virtoCommerce.catalogModule.valueTypes',
        'virtoCommerce.catalogModule.propertyValidators', 'virtoCommerce.catalogModule.measures', 'virtoCommerce.catalogModule.propertyGroups',
        function ($scope, $q, $timeout, bladeNavigationService, dialogService, properties, valueTypes, propertyValidators, measures, propertyGroups) {
            var blade = $scope.blade;
            blade.updatePermission = 'catalog:metadata-property:edit';
            blade.origEntity = {};
            $scope.currentChild = undefined;
            blade.title = 'catalog.blades.property-detail.title';
            blade.subtitle = 'catalog.blades.property-detail.subtitle';
            blade.availableValueTypes = valueTypes.get();
            $scope.propertyGroupSelectorShown = false;

            blade.hasMultivalue = true;
            blade.hasDictionary = true;
            blade.hasMultilanguage = true;

            blade.availablePropertyTypes = blade.catalogId ? ['Product', 'Variation', 'Category', 'Catalog'] : ['Product', 'Variation', 'Category'];

            $scope.$watch('blade.currentEntity.valueType', function (newValue, oldValue) {
                blade.hasMultivalue = true;
                blade.hasDictionary = true;
                blade.hasMultilanguage = true;
                switch (newValue) {
                    case 'DateTime':
                    case 'Boolean':
                        blade.hasMultivalue = blade.currentEntity.multivalue = false;
                        blade.hasDictionary = blade.currentEntity.dictionary = false;
                        blade.hasMultilanguage = blade.currentEntity.multilanguage = false;
                        break;
                    case 'Integer':
                    case 'GeoPoint':
                    case 'Number':
                        blade.hasDictionary = blade.currentEntity.dictionary = false;
                        blade.hasMultilanguage = blade.currentEntity.multilanguage = false;
                        break;
                    case 'LongText':
                        blade.hasDictionary = blade.currentEntity.dictionary = false;
                        break;
                    case 'Html':
                        blade.hasDictionary = blade.currentEntity.dictionary = false;
                        blade.hasMultivalue = blade.currentEntity.multivalue = false;
                        break;
                    case 'Measure':
                        blade.hasMultivalue = blade.currentEntity.multivalue = false;
                        blade.hasDictionary = blade.currentEntity.dictionary = false;
                        blade.hasMultilanguage = blade.currentEntity.multilanguage = false;
                        blade.isLoading = true;
                        measures.searchMeasures({
                            sort: 'name:desc',
                            skip: 0,
                            take: 100
                        }, function (data) {
                            blade.measures = data.results;
                            blade.isLoading = false;
                        });
                        break;
                }
            });

            $scope.doValidateNameAsync = value => {
                // common property name errors validation
                if (value && !propertyValidators.isNameValid(value)) {
                    $scope.errorData = {
                        errorMessage: 'property-naming-error'
                    }
                    return $q.reject();
                }

                // validation for category properties
                // skips backend property name validation for current entity edit when the old name equals new name
                if (!blade.origEntity.isNew && value === blade.origEntity.name) {
                    $scope.errorData = null;
                    return $q.resolve();
                }

                return properties.validateCategoryPropertyName({
                    propertyName: value,
                    propertyValueType: blade.currentEntity.valueType,
                    propertyType: blade.origEntity.type,
                    categoryId: blade.origEntity.categoryId,
                    catalogId: blade.origEntity.catalogId
                }).$promise.then(result => {
                    if (result.isValid) {
                        $scope.errorData = null;
                        return $q.resolve();
                    } else {
                        $scope.errorData = result.errors[0];
                        return $q.reject();
                    }
                });
            };

            blade.refresh = function (parentRefresh) {
                if (blade.currentEntityId) {
                    properties.get({ propertyId: blade.currentEntityId }, function (data) {
                        initializeBlade(data);
                        if (parentRefresh) {
                            blade.parentBlade.refresh();
                        }
                    },
                        function (error) { bladeNavigationService.setError('Error ' + error.status, blade); });
                } else if (blade.categoryId) {
                    properties.newCategoryProperty({ categoryId: blade.categoryId }, function (data) {
                        initializeBlade(data);
                    },
                        function (error) { bladeNavigationService.setError('Error ' + error.status, blade); });
                }
                else if (blade.catalogId) {
                    properties.newCatalogProperty({ catalogId: blade.catalogId }, function (data) {
                        initializeBlade(data);
                    },
                        function (error) { bladeNavigationService.setError('Error ' + error.status, blade); });
                }
            };

            $scope.openChild = function (childType) {
                var newBlade = { id: "propertyChild" };
                newBlade.property = blade.currentEntity;
                newBlade.languages = blade.languages;
                newBlade.defaultLanguage = blade.defaultLanguage;
                switch (childType) {
                    case 'attr':
                        newBlade.title = 'catalog.blades.property-attributes.title';
                        newBlade.titleValues =
                            { name: blade.origEntity.name ? blade.origEntity.name : blade.currentEntity.name };
                        newBlade.subtitle = 'catalog.blades.property-attributes.subtitle';
                        newBlade.controller = 'virtoCommerce.catalogModule.propertyAttributesController';
                        newBlade.template = 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/property-attributes.tpl.html';
                        break;
                    case 'rules':
                        newBlade.title = 'catalog.blades.property-validationRule.title';
                        newBlade.titleValues =
                            { name: blade.origEntity.name ? blade.origEntity.name : blade.currentEntity.name };
                        newBlade.subtitle = 'catalog.blades.property-validationRule.subtitle';
                        newBlade.controller = 'virtoCommerce.catalogModule.propertyValidationRulesController';
                        newBlade.template = 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/property-validationRules.tpl.html';
                        break;
                    case 'dict':
                        newBlade.title = 'catalog.blades.property-dictionary.title';
                        newBlade.titleValues =
                            { name: blade.origEntity.name ? blade.origEntity.name : blade.currentEntity.name };
                        newBlade.subtitle = 'catalog.blades.property-dictionary.subtitle';
                        newBlade.controller = 'virtoCommerce.catalogModule.propertyDictionaryListController';
                        newBlade.template = 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/property-dictionary-list.tpl.html';
                        break;
                }
                bladeNavigationService.showBlade(newBlade, blade);
                $scope.currentChild = childType;
            };

            function initializeBlade(data) {
                properties.values({ propertyId: data.id }, function (response) {
                    data.dictionaryValues = response;
                    if (data.valueType === 'Number' && data.dictionaryValues) {
                        _.forEach(data.dictionaryValues, function (entry) {
                            entry.value = parseFloat(entry.value);
                        });
                    }

                    if (blade.propertyType) {
                        data.type = blade.propertyType;
                    }

                    // sort display names by languages array
                    if (data.displayNames) {
                        data.displayNames = _.sortBy(data.displayNames, function (x) {
                            return _.indexOf(blade.languages, x.languageCode);
                        });
                    }

                    blade.currentEntity = angular.copy(data);

                    if (blade.currentEntity.type !== 'Product' && blade.currentEntity.type !== 'Variation') {
                        blade.availableValueTypes = blade.availableValueTypes.filter(item => item.valueType !== 'Measure');
                    }

                    blade.origEntity = data;
                    $timeout(resetPropertyGroupSelector, 0);
                    blade.isLoading = false;
                });
            }

            function lockSave() {
                $scope.duplicatedName = angular.copy(blade.currentEntity.name);
            }

            function isSaveLocked() {
                var result = blade.currentEntity && $scope.duplicatedName === blade.currentEntity.name;

                return result;
            }

            function isDirty() {
                return !angular.equals(blade.currentEntity, blade.origEntity) && blade.hasUpdatePermission();
            }

            function canSave() {
                return (blade.origEntity.isNew || isDirty()) && formScope && formScope.$valid && !isSaveLocked();
            }
        
            function saveChanges() {
                blade.isLoading = true;

                $scope.doValidateNameAsync(blade.currentEntity.name).then(() => {
                    bladeNavigationService.closeChildrenBlades(blade);

                    delete blade.currentEntity.validationRule; // clear read-only property
                    if (blade.currentEntity.valueType !== 'ShortText' && blade.currentEntity.valueType !== 'LongText') {
                        delete blade.currentEntity.validationRules;
                    }

                    properties.update(blade.currentEntity, function (data, headers) {
                        blade.currentEntityId = data.id;
                        blade.refresh(true);
                    });
                }, () => {
                    lockSave();
                    blade.isLoading = false;
                });
            }

            function removeProperty(prop) {
                var dialog = {
                    id: 'confirmDelete',
                    messageValues: { name: prop.name },
                    callback: function (doDeleteValues) {
                        blade.isLoading = true;

                        properties.remove({ id: prop.id, doDeleteValues: doDeleteValues }, function () {
                            $scope.bladeClose();
                            blade.parentBlade.refresh();
                        });
                    }
                };
                dialogService.showDialog(dialog, 'Modules/$(VirtoCommerce.Catalog)/Scripts/dialogs/deleteProperty-dialog.tpl.html', 'platformWebApp.confirmDialogController');
            }

            var hasEditDictionaryPermission = bladeNavigationService.checkPermission('catalog:dictionary-property:edit');

            blade.canEditDictionary = function () {
                return blade.currentEntity && blade.currentEntity.dictionary && !blade.currentEntity.isNew && hasEditDictionaryPermission;
            }

            blade.onClose = function (closeCallback) {
                bladeNavigationService.showConfirmationIfNeeded(isDirty(), canSave(), blade, saveChanges, closeCallback, 'catalog.dialogs.property-save.title', 'catalog.dialogs.property-save.message');
            };

            var formScope;
            $scope.setForm = function (form) { formScope = form; }

            blade.headIcon = 'fa fa-gear';

            blade.toolbarCommands = [
                {
                    name: 'platform.commands.save',
                    icon: 'fas fa-save',
                    executeMethod: saveChanges,
                    canExecuteMethod: canSave
                },
                {
                    name: 'platform.commands.reset',
                    icon: 'fa fa-undo',
                    executeMethod: function () {
                        angular.copy(blade.origEntity, blade.currentEntity);
                    },
                    canExecuteMethod: isDirty
                },
                {
                    name: 'platform.commands.delete',
                    icon: 'fas fa-trash-alt',
                    executeMethod: function () {
                        removeProperty(blade.origEntity);
                    },
                    canExecuteMethod: function () {
                        return blade.origEntity.isManageable && !blade.origEntity.isNew;
                    }
                }
            ];

            function resetPropertyGroupSelector() {
                $scope.propertyGroupSelectorShown = true;
            }

            blade.fetchPropertyGroups = function (criteria) {
                criteria.catalogId = blade.origEntity.catalogId;
                return propertyGroups.search(criteria);
            }

            // actions on load    
            blade.refresh();
    }]);
