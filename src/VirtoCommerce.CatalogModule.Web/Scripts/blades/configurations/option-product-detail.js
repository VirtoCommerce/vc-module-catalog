angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.configurationOptionProductDetailController', ['$scope', 'platformWebApp.bladeNavigationService',
    function ($scope, bladeNavigationService) {
        var blade = $scope.blade;
        blade.headIcon = blade.origEntity.productType === 'Digital' ? 'fa fa-file-zip-o' : 'fa fa-dropbox';
        blade.title = blade.origEntity.productName;
        $scope.isValid = false;

        $scope.$watch("blade.currentEntity", function () {
            $scope.isValid = $scope.formScope && $scope.formScope.$valid;
        }, true);

        blade.toolbarCommands = [
            {
                name: "platform.commands.reset",
                icon: 'fa fa-undo',
                executeMethod: function () { angular.copy(blade.origEntity, blade.currentEntity); },
                canExecuteMethod: isDirty,
                permission: 'catalog:configurations:update'
            }, {
                name: "catalog.commands.open-item",
                icon: 'fa fa-edit',
                executeMethod: function () { openCurrentItem(); },
                canExecuteMethod: function () { return true; }
            }
        ];

        $scope.setForm = function (form) { $scope.formScope = form; };

        $scope.saveChanges = function () {
            angular.copy(blade.currentEntity, blade.origEntity);

            $scope.bladeClose();
        };

        function isDirty() {
            return !angular.equals(blade.currentEntity, blade.origEntity);
        }

        function initialize(item) {
            blade.currentEntity = angular.copy(item);
            blade.isLoading = false;
        }

        function openCurrentItem() {
            var newBlade = {
                id: 'optionItemDetail',
                controller: 'virtoCommerce.catalogModule.itemDetailController',
                template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/item-detail.tpl.html',
                itemId: blade.currentEntity.productId,
                productType: blade.currentEntity.productType,
            };

            bladeNavigationService.showBlade(newBlade, blade);
        }

        initialize(blade.origEntity);
    }]);
