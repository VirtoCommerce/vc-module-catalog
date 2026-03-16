angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.configurationSectionDetailController',
        ['$scope', 'platformWebApp.bladeNavigationService', 'platformWebApp.metaFormsService',
        function ($scope, bladeNavigationService, metaFormsService) {
            var blade = $scope.blade;
            blade.headIcon = 'fas fa-puzzle-piece';
            blade.title = blade.origEntity.name ? blade.origEntity.name : 'catalog.blades.section-details.title';

            // Get metafields for the form
            blade.metaFields = metaFormsService.getMetaFields("configurationSectionDetail");

            // Section types for the dropdown
            blade.sectionTypes = ['Product', 'Variation', 'Text', 'File'];

            $scope.currentChild = undefined;

            var formScope;
            $scope.setForm = function (form) { formScope = form; };

            var previousAllowCustomText = null;
            var previousAllowPredefinedOptions = null;

            $scope.$watch("blade.currentEntity", function (entity) {
                if (!entity) {
                    return;
                }

                if (entity.type === 'Text') {
                    enforceTextToggleConstraint(entity);
                }

                previousAllowCustomText = entity.allowCustomText;
                previousAllowPredefinedOptions = entity.allowPredefinedOptions;
            }, true);

            function isDirty() {
                return !angular.equals(blade.currentEntity, blade.origEntity);
            }

            function canSave() {
                return isDirty() && formScope && formScope.$valid && blade.currentEntity.name;
            }

            function saveChanges() {
                var isNew = !blade.origEntity.name;
                angular.copy(blade.currentEntity, blade.origEntity);

                if (isNew && angular.isFunction(blade.onSaveNew)) {
                    blade.onSaveNew(blade.origEntity);
                }

                $scope.bladeClose();
            }

            blade.onClose = function (closeCallback) {
                bladeNavigationService.showConfirmationIfNeeded(isDirty(), canSave(), blade, saveChanges, closeCallback,
                    "catalog.dialogs.configuration-save.title", "catalog.dialogs.configuration-save.message");
            };

            blade.toolbarCommands = [
                {
                    name: "platform.commands.save",
                    icon: 'fas fa-save',
                    executeMethod: saveChanges,
                    canExecuteMethod: canSave,
                    permission: 'catalog:configurations:update'
                },
                {
                    name: "platform.commands.reset",
                    icon: 'fa fa-undo',
                    executeMethod: function () { angular.copy(blade.origEntity, blade.currentEntity); },
                    canExecuteMethod: isDirty,
                    permission: 'catalog:configurations:update'
                }
            ];

            $scope.openChild = function (childType) {
                var newBlade = { id: "sectionChild" };
                newBlade.sectionEntity = blade.currentEntity;

                switch (childType) {
                    case 'options':
                        newBlade.title = 'catalog.blades.section-options-list.title';
                        newBlade.subtitle = 'catalog.blades.section-options-list.subtitle';
                        newBlade.controller = 'virtoCommerce.catalogModule.configurationOptionsListController';
                        newBlade.template = 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/configurations/configuration-options-list.tpl.html';
                        break;
                }

                bladeNavigationService.showBlade(newBlade, blade);
                $scope.currentChild = childType;
            };

            // Function exposed on blade for template access
            blade.isTypeChangeDisabled = function() {
                if (blade.currentEntity == null || blade.currentEntity.type == null) {
                    return false;
                }

                if (blade.currentEntity.id == null && blade.currentEntity.type === 'File') {
                    return false;
                }

                if (blade.currentEntity.id == null && !_.any(blade.currentEntity.options)) {
                    return false;
                }

                return true;
            };

            blade.canShowOptions = function() {
                return blade.currentEntity.type === 'Product' ||
                    (blade.currentEntity.type === 'Text' && blade.currentEntity.allowPredefinedOptions);
            };

            // At least one of allowCustomText/allowPredefinedOptions must be true for Text sections.
            // When user turns one off, the other is forced on.
            function enforceTextToggleConstraint(entity) {
                if (previousAllowCustomText && !entity.allowCustomText && !entity.allowPredefinedOptions) {
                    entity.allowPredefinedOptions = true;
                } else if (previousAllowPredefinedOptions && !entity.allowPredefinedOptions && !entity.allowCustomText) {
                    entity.allowCustomText = true;
                }
            }

            function initialize(item) {
                if (item.options == null) {
                    item.options = [];
                }

                if (item.id == null) {
                    item.allowCustomText = true;
                }

                blade.currentEntity = angular.copy(item);

                // Initialize previous values for mutual exclusivity tracking
                previousAllowCustomText = blade.currentEntity.allowCustomText;
                previousAllowPredefinedOptions = blade.currentEntity.allowPredefinedOptions;

                blade.isLoading = false;
            }

            initialize(blade.origEntity);
        }]);
