angular.module('virtoCommerce.catalogModule')
.controller('virtoCommerce.catalogModule.itemPropertyDetailController', ['$scope', 'platformWebApp.bladeNavigationService', 'platformWebApp.dialogService', function ($scope, bladeNavigationService, dialogService) {
    var blade = $scope.blade;
    $scope.currentChild = undefined;
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
    };

    $scope.openChild = function (childType) {
        var newBlade = { id: "propertyChild", property: blade.currentEntity };

        switch (childType) {
            case 'valType':
                newBlade.title = 'catalog.blades.property-valueType.title';
                newBlade.titleValues = { name: blade.origEntity.name ? blade.origEntity.name : blade.currentEntity.name };
                newBlade.subtitle = 'catalog.blades.property-valueType.subtitle';
                newBlade.controller = 'virtoCommerce.catalogModule.propertyValueTypeController';
                newBlade.template = 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/property-valueType.tpl.html';
                break;
        }
        bladeNavigationService.showBlade(newBlade, blade);
        $scope.currentChild = childType;
    }

   
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
            title: "catalog.dialogs.property-delete.title",
            message: 'catalog.dialogs.property-delete.message',
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
        blade.headIcon = 'fa-gear';
        blade.toolbarCommands = [           
            {
                name: "platform.commands.delete", icon: 'fa fa-trash-o',
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
