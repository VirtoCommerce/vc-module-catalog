angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.changeCategoryActionStepsController', ['$scope', '$translate', 'platformWebApp.bladeNavigationService', '$localStorage', function ($scope, $translate, bladeNavigationService, $localStorage) {
        var blade = $scope.blade;
        blade.canStartProcess = false;
        blade.isLoading = true;
        blade.categoryId = "";
        blade.categoryName = "Nothing";
        

        function initializeBlade() {
            blade.isLoading = false;
        }

     
        $scope.startAction = function () {
            var progressBlade = {
                id: 'actionProgress',
                title: 'catalog.blades.action-progress.title',
                controller: 'virtoCommerce.catalogModule.bulkActionProgressController',
                template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/bulk/bulk-action-progress.tpl.html',
                
                onCompleted: function () {
                    blade.isProcessing = false;
                }
            };

            bladeNavigationService.showBlade(progressBlade, blade);
        };

        $scope.selectCategory = function () {
            var selection = [blade.categoryId];
            var options = {
                showCheckingMultiple: false,
                allowCheckingCategory: true,
                allowCheckingItem: false,
                selectedItemIds: selection,
                checkItemFn: function (listItem, isSelected) {
                    if (isSelected) {
                        if (!_.find(selection, function (x) { return x === listItem.id; })) {
                            selection.push(listItem.id);
                            blade.categoryName = listItem.name;
                        }
                    }
                    else {
                        selection = _.reject(selection, function (x) { return x === listItem.id; });
                    }
                }
            };
            var newBlade = {
                id: "CatalogItemsSelect",
                controller: 'virtoCommerce.catalogModule.catalogItemSelectController',
                template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/common/catalog-items-select.tpl.html',
                title: 'catalog.selectors.blades.titles.select-categories',
                options: options,
                breadcrumbs: [],
                toolbarCommands: [
                    {
                        name: "platform.commands.confirm", icon: 'fa fa-check',
                        executeMethod: function (pickingBlade) {
                            blade.categoryId = selection[0];
                            $scope.selectedCount = 1;
                            blade.canStartProcess = true;
                            bladeNavigationService.closeBlade(pickingBlade);
                        },
                        canExecuteMethod: function () {
                            return _.any(selection);
                        }
                    },
                    {
                        name: "platform.commands.reset", icon: 'fa fa-undo',
                        executeMethod: function (pickingBlade) {
                            selection = [];
                            blade.categoryId = "";
                            $scope.selectedCount = 0;
                            bladeNavigationService.closeBlade(pickingBlade);
                        },
                        canExecuteMethod: function () {
                            return _.any(selection);
                        }
                    }]
            };
            bladeNavigationService.showBlade(newBlade, blade);
        };

        $scope.validateActionParameters = function () {
            return true;
        };

        $scope.blade.headIcon = 'fa-upload';
        initializeBlade();
    }]);
