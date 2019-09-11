angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.actionListController', ['$scope', 'platformWebApp.bladeNavigationService', 'virtoCommerce.catalogModule.catalogExportService', function ($scope, bladeNavigationService, catalogExportService) {
        var blade = $scope.blade;

        $scope.selectedNodeId = null;

        function initializeBlade() {
            $scope.actionList = [
                {
                    icon: 'fa fa-cogs',
                    name: 'Change Category',
                    description: 'Move products to another category',
                    controller: 'virtoCommerce.catalogModule.changeCategoryActionStepsController',
                    template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/bulk/action-change-category.tpl.html',
                    id: 'changeCatalogOrCategory',
                    title: 'Move selected products to category',
                    subtitle: 'Select category to move'
                    
                },
                {
                    icon: 'fa fa-cogs',
                    name: 'Edit Properties',
                    description: 'Update one ore more properties',
                    controller: 'virtoCommerce.catalogModule.editPropertiesActionController',
                    template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/bulk/action-edit-properties.tpl.html',
                    id: 'catalogGenericExport',
                    title: 'Set property value',
                    subtitle: 'Set one value for selected properties'
                    
                },
                {
                    icon: 'fa fa-cogs',
                    name: 'Add Property',
                    description: 'Add one property',
                    controller: 'virtoCommerce.catalogModule.changeCategoryActionStepsController',
                    template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/bulk/change-category-action-settings.tpl.html',
                    id: 'catalogGenericExport',
                    title: 'catalog.blades.exporter.productTitle',
                    subtitle: 'catalog.blades.exporter.productSubtitle'
                    
                }
            ];
            blade.isLoading = false;
        };

        $scope.openBlade = function (data) {
            var newBlade = {};
            angular.copy(data, newBlade);
            newBlade.selectedCategories = blade.selectedCategories;
            newBlade.selectedProducts = blade.selectedProducts;
            newBlade.catalog = blade.catalog;

            bladeNavigationService.showBlade(newBlade, blade);
        }

        $scope.blade.headIcon = 'fa-upload';
        $scope.blade.title = "Bulk action list";
        $scope.blade.subtitle = "Select action for bulk operation";


        initializeBlade();
    }]);
