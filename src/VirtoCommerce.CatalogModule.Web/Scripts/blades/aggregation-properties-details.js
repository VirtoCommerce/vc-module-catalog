angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.aggregationPropertyDetailsController', [
        '$scope', 'platformWebApp.bladeNavigationService', 'virtoCommerce.catalogModule.aggregationProperties', 'virtoCommerce.catalogModule.measures',
        function ($scope, bladeNavigationService, aggregationProperties, measures) {
            var blade = $scope.blade;
            blade.updatePermission = 'store:update';
            blade.headIcon = 'fa fa-gear';

            var attributeType = 'Attribute';
            var rangeType = 'Range';
            var priceRangeType = 'PriceRange';
            blade.propertyTypes = [attributeType, rangeType];

            function initializeBlade() {
                $scope.isValid = true;
                blade.originalProperty = blade.property;
                blade.property = angular.copy(blade.property);

                if (blade.property.values == null)
                    blade.property.values = [];
                blade.values = [];

                aggregationProperties.getValues({ storeId: blade.storeId, propertyName: blade.originalProperty.name }, function (results) {
                    blade.values = results;

                    if (blade.property.measureId) {
                        measures.getMeasure({ id: blade.property.measureId }, function (data) {
                            blade.measure = data;
                            blade.units = data.units;
                            if (!blade.property.unitOfMeasureId) {
                                //set default unit of measure
                                const defaultUnit = _.find(blade.units, function (x) {
                                    return x.isDefault
                                });

                                if (defaultUnit) {
                                    blade.property.unitOfMeasureId = defaultUnit.id;
                                }
                            }
                            blade.isLoading = false;
                        });
                    } else {
                        blade.isLoading = false;
                    }
                }, function (error) {
                    bladeNavigationService.setError('Error: ' + error.status, blade);
                    blade.isLoading = false;
                });
            }

            blade.isRange = function () {
                return blade.property.type !== attributeType;
            };

            blade.canChangeType = function () {
                return blade.property.type !== priceRangeType;
            };

            blade.canChangeSize = function () {
                return !blade.isRange();
            };

            blade.getValues = function (search) {
                var result;

                if (blade.isRange()) {
                    result = [0, +(Math.max(...blade.property.values) | 0) + 100];
                    if (search && !isNaN(search) && angular.isNumber(+search)) {
                        result.unshift(search);
                    }
                } else {
                    result = blade.values;
                }

                return result;
            };

            blade.hasPredefinedValues = function () {
                return blade.isRange() || (!!blade.values && blade.values.length > 0);
            }

            function isDirty() {
                return !angular.equals(blade.property, blade.originalProperty) && blade.hasUpdatePermission();
            }

            blade.onClose = function (closeCallback) {
                bladeNavigationService.showConfirmationIfNeeded(isDirty(), true, blade, $scope.saveChanges, closeCallback,
                    'Save changes', 'The property settings have been modified. Do you want to save changes?');
            };

            $scope.saveChanges = function () {
                blade.property.valuesCount = blade.property.values.length;

                if (blade.property.unitOfMeasureId) {
                    const unit = _.find(blade.units, function (x) {
                        return x.id === blade.property.unitOfMeasureId;
                    });

                    if (!unit.isDefault) {
                        blade.property.indexFieldName = `${blade.property.name}-${unit.code}`;
                    } else {
                        blade.property.indexFieldName = blade.property.name;
                    }
                }

                angular.copy(blade.property, blade.originalProperty);
                $scope.bladeClose();
            };

            function openValueMapping() {
                var newBlade = {
                    id: 'valueMappingFieldDetails',
                    controller: 'virtoCommerce.searchModule.valueMappingFieldDetailsController',
                    template: 'Modules/$(VirtoCommerce.Search)/Scripts/blades/value-mapping-field-details.html',
                    data: {
                        documentType: 'Product',
                        fieldName: blade.property.name,
                        values: [],
                    },
                };
                bladeNavigationService.showBlade(newBlade, blade);
            }

            blade.toolbarCommands = [
                {
                    name: 'search.commands.value-mapping',
                    icon: 'fas fa-wrench',
                    executeMethod: openValueMapping,
                    canExecuteMethod: function () {
                        return true;
                    },
                    permission: 'search:index:manage',
                },
            ];

            initializeBlade();
        }
    ]);
