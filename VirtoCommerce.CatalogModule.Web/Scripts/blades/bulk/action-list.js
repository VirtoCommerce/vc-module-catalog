angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.actionListController', ['$scope', 'platformWebApp.bladeNavigationService', 'virtoCommerce.catalogModule.catalogBulkActionService', 'virtoCommerce.catalogModule.bulkActions', function ($scope, bladeNavigationService, bulkActionServiceRegistrar, bulkActions) {
        var blade = $scope.blade;
        $scope.selectedNodeId = null;

        function initializeBlade() {
            bulkActions.getActions(function (data) {
                if (data) {
                    blade.actions = _.each(data, function (action) { blade.initializeAction(action); });
                }
            });

            blade.isLoading = false;
        };

        $scope.openBlade = function(data) {
            var newBlade = {};
            angular.copy(data, newBlade);

            var registrationInfo = bulkActionServiceRegistrar.getByName(data.name);
            if (!registrationInfo) {
                bladeNavigationService.setError(`Can't find controller for action ${data.name}`, blade);
            } else {
                newBlade.controller = registrationInfo.controller;
                newBlade.template = registrationInfo.template;

                bladeNavigationService.showBlade(newBlade, blade);
            }
        };

        $scope.blade.headIcon = 'fa-upload';
        $scope.blade.title = "Bulk action list";
        $scope.blade.subtitle = "Select action for bulk operation";


        blade.initializeAction = function(action) {
            action.title = `actions.types.${action.name}.title`;
            action.subtitle = `actions.types.${action.name}.subtitle`;
            action.id = action.name;
            action.icon = 'fa fa-cogs';
            action.actionDataContext = {
                actionName: action.name,
                contextTypeName: action.contextTypeName,
                dataQuery: blade.dataQuery
            };
        };

        initializeBlade();
    }]);
