angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.propertyDictionaryDetailsController',
        ['$scope', 'platformWebApp.dialogService', 'platformWebApp.bladeNavigationService', 'virtoCommerce.catalogModule.propDictItems', function ($scope, dialogService, bladeNavigationService, propDictItems) {
            var blade = $scope.blade;
            blade.headIcon = 'fa fa-book';
            blade.updatePermission = 'catalog:dictionary-property:edit';

            $scope.isValid = true;
            $scope.blade.isLoading = false;
            $scope.validationRules = blade.property.validationRule;

            $scope.setForm = function (form) {
                blade.formScope = form;
            }

            /* Color picker */
            $scope.openColorPicker = function ($event) {
                const id = $event.currentTarget.dataset.colorInputId;
                document.getElementById(id).click();
            };

            blade.toolbarCommands = [
                {
                    name: "platform.commands.save",
                    icon: 'fas fa-save',
                    executeMethod: saveChanges,
                    canExecuteMethod: canSave
                },
                {
                    name: "platform.commands.reset",
                    icon: 'fa fa-undo',
                    executeMethod: function () {
                        angular.copy(blade.origEntity, blade.currentEntity);
                    },
                    canExecuteMethod: isDirty,
                },
            ];

            function initializeBlade() {
                blade.currentEntity = angular.copy(blade.dictionaryItem);
                blade.origEntity = blade.dictionaryItem;
                blade.isLoading = false;
            }

            function isDirty() {
                return !angular.equals(blade.currentEntity, blade.origEntity) && blade.hasUpdatePermission();
            }

            function canSave() {
                return isDirty() && blade.formScope && blade.formScope.$valid;
            }

            blade.onClose = function (closeCallback) {
                bladeNavigationService.showConfirmationIfNeeded(isDirty(), $scope.isValid, blade, $scope.saveChanges, closeCallback,
                    "catalog.dialogs.property-save.title", "catalog.dialogs.property-save.message");
            };

            function saveChanges() {
                blade.currentEntity.localizedValues = [];
                blade.languages.forEach(function (lang) {
                    let dictValue = {};
                    dictValue.languageCode = blade.property.multilanguage ? lang : undefined;
                    dictValue.value = blade.currentEntity[lang];
                    blade.currentEntity.localizedValues.push(dictValue);
                });
                propDictItems.save([blade.currentEntity], function (response) {
                    // call parent onSave
                    blade.onSaveChanges();
                    angular.copy(blade.currentEntity, blade.origEntity);
                });
            }

            initializeBlade();
        }]);
