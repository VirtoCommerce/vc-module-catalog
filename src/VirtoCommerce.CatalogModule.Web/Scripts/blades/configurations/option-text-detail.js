angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.configurationOptionTextDetailController', ['$scope',
    function ($scope) {
        var blade = $scope.blade;
        blade.headIcon = 'fa fa-font';
        blade.title = 'catalog.blades.option-details.labels.text';
        $scope.isValid = false;

        $scope.$watch("blade.currentEntity", function (entity) {
            $scope.isValid = $scope.formScope && $scope.formScope.$valid && entity.text;
            if ($scope.isValid && blade.origEntity.text) {
                // Update case (form is valid when changes exist)
                $scope.isValid = blade.origEntity.text !== entity.text;
            }
        }, true);

        blade.toolbarCommands = [
            {
                name: "platform.commands.reset",
                icon: 'fa fa-undo',
                executeMethod: function () { angular.copy(blade.origEntity, blade.currentEntity); },
                canExecuteMethod: isDirty,
                permission: 'catalog:configurations:update'
            }
        ];

        $scope.setForm = function (form) { $scope.formScope = form; };

        $scope.saveChanges = function () {
            var isNew = !blade.origEntity.text;
            angular.copy(blade.currentEntity, blade.origEntity);

            if (isNew && angular.isFunction(blade.onSaveNew)) {
                blade.onSaveNew(blade.origEntity);
            }

            $scope.bladeClose();
        };

        function isDirty() {
            return !angular.equals(blade.currentEntity, blade.origEntity);
        }

        function initialize(item) {
            blade.currentEntity = angular.copy(item);
            blade.isLoading = false;
        }

        initialize(blade.origEntity);
    }]);
