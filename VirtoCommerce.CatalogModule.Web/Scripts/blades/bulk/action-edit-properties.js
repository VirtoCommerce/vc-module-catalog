angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.editPropertiesActionController', ['$scope', '$translate', 'platformWebApp.bladeNavigationService', '$localStorage', 'virtoCommerce.catalogModule.bulkActions', function ($scope, $translate, bladeNavigationService, $localStorage, bulkActions) {
        var blade = $scope.blade;
        blade.canStartProcess = false;
        blade.isLoading = true;
        blade.isPropertiesSelected = false;
        blade.properties = [];
        blade.includedProperties = [];
        blade.propertySelected = 0;

        blade.actionDataContext = angular.extend({
            editedProperties : {}
        }, blade.actionDataContext);

        function initializeBlade() {
            bulkActions.getActionData(blade.actionDataContext,
                function(data) {
                    blade.properties = data.properties;
                    blade.propertyTotal = data.properties.length;
                    _.each(blade.properties,
                        function (prop) {
                            if (!prop.id &&
                            (prop.name === 'Vendor' ||
                                prop.name === 'TaxType' ||
                                prop.name === 'MeasureUnit' ||
                                prop.name === 'PackageType' ||
                                prop.name === 'WeightUnit')) {
                                prop.UseDefaultUIForEdit = false;
                            } else {
                                prop.UseDefaultUIForEdit = true;
                            }
                            prop.values = [];
                            prop.group = 'All properties';
                        });
                    blade.isLoading = false;
                });
        }

        $scope.startAction = function () {

            var progressBlade = {
                id: 'actionProgress',
                title: 'catalog.blades.action-progress.title',
                controller: 'virtoCommerce.catalogModule.bulkActionProgressController',
                template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/bulk/bulk-action-progress.tpl.html',
                actionDataContext: blade.actionDataContext,
                onCompleted: function () {
                    blade.isProcessing = false;
                }
            };

            bladeNavigationService.showBlade(progressBlade, blade);
        };

        $scope.selectProperties = function () {
            var newBlade = {
                id: 'propertiesSelector',
                controller: 'virtoCommerce.catalogModule.propertiesSelectorController',
                template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/bulk/step-select-properties.tpl.html',
                properties: blade.properties,
                includedProperties : blade.includedProperties,
                onSelected: function (includedProperties) {
                    blade.includedProperties = includedProperties;
                    blade.propertySelected = includedProperties.length;
                    blade.isPropertiesSelected = true;
                    $scope.editProperties();
                }
            };

            bladeNavigationService.showBlade(newBlade, blade);
        };

        $scope.editProperties = function () {
            var newBlade = {
                id: 'propertiesEditor',
                controller: 'virtoCommerce.catalogModule.editPropertiesActionStepController',
                template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/bulk/step-edit-properties.tpl.html',
                properties: blade.includedProperties,
                propGroups: [{ title: 'catalog.properties.product', type: 'Product' }, { title: 'catalog.properties.variation', type: 'Variation' }],
                onSelected: function (editedProperties) {
                    blade.canStartProcess = true;
                    blade.actionDataContext.properties = editedProperties;
                }
            };
            bladeNavigationService.showBlade(newBlade, blade);
        };

        $scope.validateActionParameters = function () {
            return true;
        };

        $scope.blade.headIcon = 'fa-upload';

        initializeBlade();
    }]);
