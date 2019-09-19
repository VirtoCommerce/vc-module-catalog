angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.editPropertiesActionStepController', ['$scope', 'virtoCommerce.catalogModule.properties', 'platformWebApp.bladeNavigationService', 'virtoCommerce.catalogModule.propDictItems', 'virtoCommerce.customerModule.members', 'platformWebApp.settings', 'virtoCommerce.coreModule.packageType.packageTypeUtils', function ($scope, properties, bladeNavigationService, propDictItems, members, settings, packageTypeUtils) {
        var blade = $scope.blade;
        $scope.isValid = true;
        blade.refresh = function () {
            blade.parentBlade.refresh();
        };

        function initialize() {
            
            blade.subtitle = 'catalog.blades.property-list.subtitle';

            blade.currentEntities = angular.copy(blade.properties);
            _.each(blade.currentEntities,
                function(prop) {
                    if (prop.required) {
                        $scope.isValid = false;
                    }
                });

            initVendors();
            blade.taxTypes = settings.getValues({ id: 'VirtoCommerce.Core.General.TaxTypes' });
            blade.weightUnits = settings.getValues({ id: 'VirtoCommerce.Core.General.WeightUnits' });
            blade.measureUnits = settings.getValues({ id: 'VirtoCommerce.Core.General.MeasureUnits' });
            blade.packageTypes = packageTypeUtils.getPackageTypes();
        };

        $scope.saveChanges = function () {
            if (blade.onSelected) {
                blade.onSelected(blade.currentEntities);
            }
            $scope.bladeClose();
        };

        $scope.getPropertyDisplayName = function (prop) {
            return _.first(_.map(_.filter(prop.displayNames, function (x) { return x && x.languageCode.startsWith(blade.defaultLanguage); }), function (x) { return x.name; }));
        };

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

        var formScope;
        $scope.setForm = function (form) {
            formScope = form;
        }

        $scope.$watch("blade.currentEntities", function () {
            $scope.isValid = formScope ? formScope.$valid : $scope.isValid;
        }, true);

        //Property with own UI 
        $scope.$watch('blade.vendor', function(newValues) {
            $scope.isValid = formScope ? formScope.$valid : $scope.isValid;
            if (blade.currentEntities) {
                var vendor = _.find(blade.vendors, function(item) { return item.id === newValues });
                var vendorProp = _.find(blade.currentEntities, function(prop) { return prop.name === 'Vendor' });
                if (vendorProp && vendor) {
                    vendorProp.values = [{ valueId: vendor.id, value: vendor.name }];
                }
            }
        });

        $scope.$watch('blade.taxType', function (newValues) {
            $scope.isValid = formScope ? formScope.$valid : $scope.isValid;
            $scope.updatePropertyValue('TaxType', newValues);
        });

        $scope.$watch('blade.weightUnit', function (newValues) {
            $scope.isValid = formScope ? formScope.$valid : $scope.isValid;
            $scope.updatePropertyValue('WeightUnit', newValues);
        });

        $scope.$watch('blade.packageType', function (newValues) {
            $scope.isValid = formScope ? formScope.$valid : $scope.isValid;
            $scope.updatePropertyValue('PackageType', newValues);
        });

        $scope.$watch('blade.measureUnit', function (newValues) {
            $scope.isValid = formScope ? formScope.$valid : $scope.isValid;
            $scope.updatePropertyValue('MeasureUnit', newValues);
        });

        $scope.updatePropertyValue = function(propName, newValue) {
            if (blade.currentEntities) {
                var prop = _.find(blade.currentEntities, function (prop) { return prop.name === propName && !prop.id; });
                if (prop) {
                    prop.values = [{ value: newValue }];
                }
            }
        };


        blade.headIcon = 'fa-gear';

        blade.toolbarCommands = [];
        blade.isLoading = false;
        initialize();

        function initVendors() {
            members.search({ memberType: 'Vendor', sort: 'name:asc', take: 1000 }, function (data) {
                blade.vendors = data.results;
            });
        }
    }]);
