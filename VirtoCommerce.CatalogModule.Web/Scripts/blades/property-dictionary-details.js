angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.propertyDictionaryDetailsController',
        ['$scope', 'platformWebApp.dialogService', 'platformWebApp.bladeNavigationService', function ($scope, dialogService, bladeNavigationService) {
            var blade = $scope.blade;
            blade.headIcon = 'fa-book';

            $scope.isValid = true;
            $scope.blade.isLoading = false;
            $scope.validationRules = blade.property.validationRule;

          
            blade.formScope = null;
            $scope.setForm = function (form) { blade.formScope = form; }

            function initializeBlade() {
                blade.currentEntity = angular.copy(blade.dictionaryItem);
                blade.origEntity = blade.dictionaryItem;                
                blade.isLoading = false;
            };

            function isDirty() {
                return !angular.equals(blade.currentEntity, blade.origEntity) && blade.hasUpdatePermission();
            }

            $scope.$watch("blade.currentEntity", function () {
                $scope.isValid = isDirty() && blade.formScope && blade.formScope.$valid;
            }, true);

            blade.onClose = function (closeCallback) {
                bladeNavigationService.showConfirmationIfNeeded(isDirty(), $scope.isValid, blade, $scope.saveChanges, closeCallback, "catalog.dialogs.property-save.title", "catalog.dialogs.property-save.message");
            };

            $scope.saveChanges = function () {               
                blade.onSaveChanges(blade.currentEntity);
                angular.copy(blade.currentEntity, blade.origEntity);
                $scope.bladeClose();
            };            
            initializeBlade();
        }]);
