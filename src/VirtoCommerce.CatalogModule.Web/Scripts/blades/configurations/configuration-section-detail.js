angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.configurationSectionDetailController', ['$scope',
        function($scope) {
            var blade = $scope.blade;
            blade.headIcon = 'fas fa-puzzle-piece';
            blade.formScope = null;

            $scope.isValid = false;

            $scope.$watch("blade.currentEntity", function () {
                $scope.isValid = blade.formScope && blade.formScope.$valid;
            }, true);

            $scope.setForm = function(form) { blade.formScope = form; };

            $scope.saveChanges = function () {
                var isNew = !blade.origEntity.name;
                angular.copy(blade.currentEntity, blade.origEntity);

                if (isNew && angular.isFunction(blade.onSaveNew)) {
                    blade.onSaveNew(blade.origEntity);
                }

                $scope.bladeClose();
            };

            function initialize(item) {
                blade.currentEntity = angular.copy(item);
                blade.isLoading = false;
            };

            initialize(blade.origEntity);
        }]);
