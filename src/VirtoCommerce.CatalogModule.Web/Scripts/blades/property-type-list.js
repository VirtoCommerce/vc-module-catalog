angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.propertyTypeListController', ['$scope', 'platformWebApp.bladeNavigationService',
        function ($scope, bladeNavigationService) {
            var blade = $scope.blade;
            blade.title = "catalog.blades.property-type-list.title";
            blade.subtitle = "catalog.blades.property-type-list.subtitle";

            blade.selectedType = null;

            blade.refresh = function () {
                blade.parentBlade.refresh();
            };

            blade.createProperty = function (propertyType) {
                blade.selectedType = propertyType;

                var newBlade = {
                    id: 'editCategoryProperty',
                    currentEntityId: undefined,
                    categoryId: blade.categoryId,
                    catalogId: blade.catalogId,
                    defaultLanguage: blade.defaultLanguage,
                    languages: blade.languages,
                    propertyType: propertyType,
                    controller: 'virtoCommerce.catalogModule.propertyDetailController',
                    template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/property-detail.tpl.html'
                };
                bladeNavigationService.showBlade(newBlade, blade);
            }

            blade.toolbarCommands = [
                {
                    name: "platform.commands.cancel",
                    icon: 'fas fa-ban',
                    executeMethod: function () {
                        bladeNavigationService.closeBlade(blade);
                    },
                    canExecuteMethod: function () {
                        return true;
                    }
                }
            ];

            function initialize() {
                blade.availablePropertyTypes = [{
                    typeName: "Category",
                    icon: "fas fa-folder"
                },
                {
                    typeName: "Product",
                    icon: "fas fa-sliders-h"
                },
                {
                    typeName: "Variation",
                    icon: "fas fa-box-open"
                }]

                if (blade.catalogId) {
                    blade.availablePropertyTypes.unshift({
                        typeName: "Catalog",
                        icon: "fas fa-archive"
                    });
                }

                blade.isLoading = false;
            }

            initialize();
        }]);
