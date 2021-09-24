angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.editPropertiesActionController', [
        '$scope',
        '$translate',
        'platformWebApp.bladeNavigationService',
        '$localStorage',
        'virtoCommerce.catalogBulkActionsModule.webApi',
        function (
            $scope,
            $translate,
            bladeNavigationService,
            $localStorage,
            webApi)
        {
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
            webApi.getActionData(blade.actionDataContext,
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
                            if (prop.ownerName === 'Native properties') {
                                prop.group = 'Native product properties';
                            } else {
                                prop.group = 'Extended properties';
                            }
                            
                        });
                    blade.isLoading = false;
                });
        }

        $scope.startAction = function () {

            var progressBlade = {
                id: 'actionProgress',
                title: 'virtoCommerce.catalogBulkActionsModule.blades.action-progress.title',
                controller: 'virtoCommerce.catalogModule.bulkActionProgressController',
                template: 'Modules/$(virtoCommerce.catalog)/Scripts/blades/bulk-action-progress.tpl.html',
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
                template: 'Modules/$(virtoCommerce.catalog)/Scripts/blades/step-select-properties.tpl.html',
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
                template: 'Modules/$(virtoCommerce.catalog)/Scripts/blades/step-edit-properties.tpl.html',
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

        $scope.blade.headIcon = 'fa fa-upload';

        initializeBlade();
    }]);
