angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.itemPropertyDetailController', ['$scope', '$q', 'virtoCommerce.catalogModule.properties', 'platformWebApp.bladeNavigationService', 'platformWebApp.dialogService', 'virtoCommerce.catalogModule.valueTypes', 'virtoCommerce.catalogModule.propertyValidators', function ($scope, $q, properties, bladeNavigationService, dialogService, valueTypes, propertyValidators) {
        var blade = $scope.blade;
        blade.availableValueTypes = valueTypes.get();
        $scope.isValid = false;

        blade.title = "catalog.blades.item-property-detail.title";
        blade.subtitle = "catalog.blades.item-property-detail.subtitle";

        $scope.$watch("blade.currentEntity", function () {
            $scope.isValid = $scope.formScope && $scope.formScope.$valid;
        }, true);

        function initialize(data) {
            if (data.valueType === 'Number' && data.dictionaryValues) {
                _.forEach(data.dictionaryValues, function (entry) {
                    entry.value = parseFloat(entry.value);
                });
            }
            blade.currentEntity = angular.copy(data);
            blade.isLoading = false;
        }

        $scope.doValidateNameAsync = value => {
            // common property name errors validation
            if (value && !propertyValidators.isNameValid(value)) {
                $scope.errorData = {
                    errorMessage: 'property-naming-error'
                }
                return $q.reject();
            }

            return properties.validateName({
                name: value,
                originalName: blade.origEntity.name,
                productId: blade.productId
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

        $scope.openBlade = newBladeData => {
            var newBlade = angular.extend({
                id: "duplicateVariationDetail",
                title: newBladeData.code,
                catalog: blade.catalog,
                controller: 'virtoCommerce.catalogModule.itemDetailController',
                template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/item-detail.tpl.html'
            }, newBladeData);

            bladeNavigationService.showBlade(newBlade, blade);
        };

        $scope.saveChanges = function () {
            angular.copy(blade.currentEntity, blade.origEntity);
            if (blade.isNew && blade.properties) {
                blade.properties.push(blade.origEntity);
            }
            $scope.bladeClose();
        };

        function removeProperty(prop) {
            var dialog = {
                id: "confirmDelete",
                title: "platform.dialogs.delete.title",
                message: 'catalog.dialogs.item-property-delete.message',
                messageValues: { name: prop.name },
                callback: function (remove) {
                    if (remove) {
                        var idx = blade.properties.indexOf(blade.origEntity);
                        blade.properties.splice(idx, 1);
                        $scope.bladeClose();
                    }
                }
            }
            dialogService.showConfirmationDialog(dialog);
        }

        $scope.setForm = function (form) { $scope.formScope = form; };

        if (!blade.isNew) {
            blade.headIcon = 'fa fa-gear';
            blade.toolbarCommands = [
                {
                    name: "platform.commands.delete", icon: 'fas fa-trash-alt',
                    executeMethod: function () {
                        removeProperty(blade.origEntity);
                    },
                    canExecuteMethod: function () { return true; }
                }
            ];
        }
        // actions on load    
        initialize(blade.origEntity);
    }]);
