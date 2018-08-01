angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.propertyDictionaryListController',
        ['$scope', '$filter', 'platformWebApp.dialogService', 'platformWebApp.settings', 'platformWebApp.bladeNavigationService', function ($scope, $filter, dialogService, settings, bladeNavigationService) {
            var blade = $scope.blade;
            var pb = $scope.blade.parentBlade;
            blade.headIcon = 'fa-book';

            blade.toolbarCommands = [
                {
                    name: "platform.commands.add", icon: 'fa fa-plus',
                    executeMethod: function () {
                        var newBlade = {
                            id: 'propertyDictionaryDetails',
                            categoryId: pb.categoryId,
                            title: 'catalog.blades.property-dictionary.labels.dictionary-edit',
                            catalogId: pb.catalogId,
                            defaultLanguage: pb.defaultLanguage,
                            controller: 'virtoCommerce.catalogModule.propertyDictionaryDetailsController',
                            template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/property-dictionary-details.tpl.html'
                        };
                        bladeNavigationService.showBlade(newBlade, blade);
                    },
                    canExecuteMethod: function () {
                        return true;
                    }
                }
            ];

            function showDetailBlade(bladeData) {
                var newBlade = {
                    id: 'propertyDictionaryDetails',
                    controller: 'virtoCommerce.catalogModule.propertyDictionaryDetailsController',
                    template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/property-dictionary-details.tpl.html'
                };
                angular.extend(newBlade, bladeData);
                bladeNavigationService.showBlade(newBlade, blade);
            };

            $scope.groupedDictionaryValues = _.map(_.groupBy(blade.property.dictionaryValues, 'alias'), function (values, key) {
                return { alias: key, values: values };
            });

            $scope.setSelectedNode = function (selectedNode) {
                $scope.selectedNode = selectedNode;
                debugger;
            };

            $scope.selectNode = function (property) {
                debugger;
                var newBlade = {
                    id: 'editCategoryProperty',
                    categoryId: blade.categoryId,
                    catalogId: blade.catalogId,
                    defaultLanguage: blade.defaultLanguage,
                    controller: 'virtoCommerce.catalogModule.propertyDictionaryDetailsController',
                    template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/property-dictionary-details.tpl.html'
                };
                bladeNavigationService.showBlade(newBlade, blade);
            };

            $scope.blade.isLoading = false;
        }]);
