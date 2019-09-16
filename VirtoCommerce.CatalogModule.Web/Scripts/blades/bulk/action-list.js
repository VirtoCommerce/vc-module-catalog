angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.actionListController', ['$scope', 'platformWebApp.bladeNavigationService', 'virtoCommerce.catalogModule.bulkActions', function ($scope, bladeNavigationService, bulkActions) {
        var blade = $scope.blade;
        $scope.selectedNodeId = null;

        function initializeBlade() {
            bulkActions.getActions(function(data) {
                if (data) {
                    blade.actions = _.each(data, function(action) { blade.initializeAction(action); });
                }
            });

            blade.isLoading = false;
        };

        $scope.openBlade = function(data) {
            var newBlade = {};
            angular.copy(data, newBlade);
            newBlade.selectedCategories = blade.selectedCategories;
            newBlade.selectedProducts = blade.selectedProducts;
            newBlade.catalog = blade.catalog;

            if (angular.isFunction(data.onInitialize)) {
                data.onInitialize(newBlade);
            }

            bladeNavigationService.showBlade(newBlade, blade);
        };

        $scope.blade.headIcon = 'fa-upload';
        $scope.blade.title = "Bulk action list";
        $scope.blade.subtitle = "Select action for bulk operation";

        blade.initializeAction = function (action) {
            action.title = `actions.types.${action.name}.title`;
            action.subtitle = `actions.types.${action.name}.subtitle`;
            action.id = action.name;
            action.icon = 'fa fa-cogs';
            action.actionDataContext = {
                actionName: action.name,
                contextTypeName: action.contextTypeName,
                dataQuery: {
                    dataQueryTypeName: action.dataQueryTypeName,
                    categoryIds: _.pluck(blade.selectedCategories, 'id'),
                    objectIds: _.pluck(blade.selectedProducts, 'id'),
                    catalogIds: [blade.catalog.id]
                }
            };

            if (action.name === 'ChangeCategoryBulkUpdateAction') {
                action.controller = 'virtoCommerce.catalogModule.changeCategoryActionStepsController';
                action.template = 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/bulk/action-change-category.tpl.html';
            }

            if (action.name === 'EditPropertiesBulkUpdateAction') {
                action.controller = 'virtoCommerce.catalogModule.editPropertiesActionController';
                action.template = 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/bulk/action-edit-properties.tpl.html';
            }
        };
        initializeBlade();
    }]);
