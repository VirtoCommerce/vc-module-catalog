angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.propertyDictionaryListController',
        ['$scope', '$filter', 'platformWebApp.dialogService', 'platformWebApp.settings', 'platformWebApp.bladeNavigationService', function ($scope, $filter, dialogService, settings, bladeNavigationService) {
            var blade = $scope.blade;
            var pb = $scope.blade.parentBlade;
            $scope.blade.isLoading = false;
            
            blade.headIcon = 'fa-book';

            blade.toolbarCommands = [
                {
                    name: "platform.commands.add", icon: 'fa fa-plus',
                    executeMethod: function () {
                        var newBlade = {
                            id: 'propertyDictionaryDetails',
                            categoryId: blade.property.categoryId,
                            isNew: true,
                            title: 'catalog.blades.property-dictionary.labels.dictionary-edit',
                            catalogId: blade.property.catalogId,
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

            blade.refresh = function () {
                $scope.groupedDictionaryValues = _.map(_.groupBy(pb.currentEntity.dictionaryValues, 'alias'), function (values, key) {
                    return { alias: key, values: values };
                });
            };

            $scope.groupedDictionaryValues = _.map(_.groupBy(blade.property.dictionaryValues, 'alias'), function (values, key) {
                return { alias: key, values: values };
            });

            $scope.setSelectedNode = function (selectedNode) {
                $scope.selectedNode = selectedNode;
            };

            $scope.selectNode = function (property) {
                var newBlade = {
                    id: 'propertyDictionaryDetails',
                    title: 'catalog.blades.property-dictionary.labels.dictionary-edit',
                    categoryId: blade.property.categoryId,
                    catalogId: blade.property.catalogId,
                    controller: 'virtoCommerce.catalogModule.propertyDictionaryDetailsController',
                    template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/property-dictionary-details.tpl.html',
                    property: property
                };
                bladeNavigationService.showBlade(newBlade, blade);
            };

            blade.refresh();
        }]);
