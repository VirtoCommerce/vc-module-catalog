angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.editPropertiesActionController', ['$scope', '$translate', 'platformWebApp.bladeNavigationService', '$localStorage', 'virtoCommerce.catalogModule.bulkActions', function ($scope, $translate, bladeNavigationService, $localStorage, bulkActions) {
        var blade = $scope.blade;
        blade.canStartProcess = false;
        blade.isLoading = true;
        blade.isPropertiesSelected = false;
        blade.properties = [];
        blade.includedProperties = [];
        blade.propertyTotal = 11;
        blade.propertySelected = 0;

        
        blade.actionDataContext = angular.extend({
            editedProperties : {}
        }, blade.actionDataContext);



        function initializeBlade() {
            blade.isLoading = false;

            bulkActions.getActionData(blade.actionDataContext,
                function(data) {
                    blade.properties = data.properties;

                    for (var prop in data.properties) {
                        prop.values = [];
                    }
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
                propGroups: [{ title: 'Product fields', type: 'own' },{ title: 'catalog.properties.product', type: 'Product' }, { title: 'catalog.properties.variation', type: 'Variation' }],
                onSelected: function (editedProperties) {
                    blade.canStartProcess = true;
                    blade.actionDataContext.editedProperties = editedProperties;
                }
            };
            bladeNavigationService.showBlade(newBlade, blade);
        };

        $scope.validateActionParameters = function () {
            return true;
        };

        $scope.blade.headIcon = 'fa-upload';

        $scope.getProductsProperties = function() {
            blade.properties = [
                {
                    "group": "Product properties",
                    "name": "isBuyable",
                    "id": "a0642cdc35e54dba8def35a9d2066117",
                    "type": "own",
                    "valueType": "Boolean",
                    "isReadOnly": false,
                    "values": [
                        {
                            "propertyName": "isBuyable",
                            "valueType": "Boolean",
                            "value" : "true"
                        }
                    ]
                },
                {
                    "group": "Product properties",
                    "name": "isActive",
                    "id": "a0642cdc35e54dba8def35a9d2066117",
                    "type": "own",
                    "valueType": "Boolean",
                    "isReadOnly": false,
                    "values": [
                        {
                            "propertyName": "isActive",
                            "valueType": "Boolean",
                            "value": "true"
                        }
                    ]
                },
                {
                    "group": "Product properties",
                    "name": "trackInventory",
                    "id": "a0642cdc35e54dba8def35a9d2066117",
                    "type": "own",
                    "valueType": "Boolean",
                    "isReadOnly": false,
                    "values": [
                        {
                            "propertyName": "trackInventory",
                            "valueType": "Boolean",
                            "value": "true"
                        }
                    ]
                },
                {
                    "group": "Product properties",
                    "name": "startDate",
                    "id": "a0642cdc35e54dba8def35a9d2066117",
                    "type": "own",
                    "valueType": "DateTime",
                    "isReadOnly": false,
                    "values": [
                        {
                            "propertyName": "trackInventory",
                            "valueType": "DateTime"
                        }
                    ]
                },
                {
                    "group": "Product properties",
                    "isReadOnly": false,
                    "isManageable": true,
                    "isNew": false,
                    "catalogId": "b61aa9d1d0024bc4be12d79bf5786e9f",
                    "name": "Brand",
                    "required": false,
                    "dictionary": true,
                    "multivalue": false,
                    "multilanguage": false,
                    "hidden": false,
                    "valueType": "ShortText",
                    "type": "Product",
                    "values": [],
                    "attributes": [],
                    "displayNames": [
                        {
                            "languageCode": "en-US"
                        }
                    ],
                    "isInherited": true,
                    "id": "4ab75b89-221d-49e2-a741-42c239b18df9"
                },
                {
                    "group": "Product properties",
                    "isReadOnly": true,
                    "isManageable": true,
                    "isNew": false,
                    "catalogId": "b61aa9d1d0024bc4be12d79bf5786e9f",
                    "categoryId": "ac56b04c5da54f038c53852f62810f27",
                    "name": "Brand",
                    "required": false,
                    "dictionary": false,
                    "multivalue": false,
                    "multilanguage": false,
                    "hidden": false,
                    "valueType": "ShortText",
                    "type": "Category",
                    "values": [],
                    "attributes": [],
                    "displayNames": [
                        {
                            "languageCode": "en-US"
                        }
                    ],
                    "isInherited": true,
                    "id": "39ba9baa-8890-46ea-94e9-2e87f85dad91"
                },
                {
                    "group": "Product properties",
                    "isReadOnly": false,
                    "isManageable": true,
                    "isNew": false,
                    "catalogId": "b61aa9d1d0024bc4be12d79bf5786e9f",
                    "name": "Colour",
                    "required": false,
                    "dictionary": true,
                    "multivalue": false,
                    "multilanguage": false,
                    "hidden": false,
                    "valueType": "ShortText",
                    "type": "Variation",
                    "values": [
                        {
                            "propertyName": "Colour",
                            "propertyId": "daa0fafb-4447-4af0-9ec1-920162f5bfbb",
                            "languageCode": "en-US",
                            "valueType": "ShortText",
                            "valueId": "75c67b4cc9d74419be745360b4e318e3",
                            "isInherited": false,
                            "propertyMultivalue": false,
                            "id": "0ba3fa152c6a4263a41e7054c7ba2138"
                        }
                    ],
                    "attributes": [
                        {
                            "value": "Colour",
                            "name": "DisplayNameen-US",
                            "id": "a3b2985de60d493ab455328789b5b9a0"
                        }
                    ],
                    "displayNames": [
                        {
                            "name": "Colour",
                            "languageCode": "en-US"
                        }
                    ],
                    "isInherited": true,
                    "id": "daa0fafb-4447-4af0-9ec1-920162f5bfbb"
                },
                {
                    "group": "Product properties",
                    "isReadOnly": false,
                    "isManageable": true,
                    "isNew": false,
                    "catalogId": "b61aa9d1d0024bc4be12d79bf5786e9f",
                    "categoryId": "ac56b04c5da54f038c53852f62810f27",
                    "name": "Heel height",
                    "required": false,
                    "dictionary": false,
                    "multivalue": false,
                    "multilanguage": false,
                    "hidden": false,
                    "valueType": "Number",
                    "type": "Product",
                    "values": [
                      
                    ],
                    "attributes": [],
                    "displayNames": [
                        {
                            "languageCode": "en-US"
                        }
                    ],
                    "isInherited": true,
                    "id": "33b31ca8-b4c3-4055-9415-6195e9d10e3f"
                },
                {
                    "group": "Product properties",
                    "isReadOnly": false,
                    "isManageable": true,
                    "isNew": false,
                    "catalogId": "b61aa9d1d0024bc4be12d79bf5786e9f",
                    "name": "Size",
                    "required": false,
                    "dictionary": false,
                    "multivalue": false,
                    "multilanguage": false,
                    "hidden": false,
                    "valueType": "ShortText",
                    "type": "Variation",
                    "values": [
                        {
                            "propertyName": "Size",
                            "propertyId": "717d2f8d-da59-479c-9db5-10f6d6249cc5",
                            "valueType": "ShortText",
                            "isInherited": false,
                            "propertyMultivalue": false,
                            "id": "d9556369f08c4f5a8f7e429de6919dc6"
                        }
                    ],
                    "attributes": [
                        {
                            "value": "Size",
                            "name": "DisplayNameen-US",
                            "id": "02627aa1736741239a0b1bfb0422ab0a"
                        }
                    ],
                    "displayNames": [
                        {
                            "name": "Size",
                            "languageCode": "en-US"
                        }
                    ],
                    "isInherited": true,
                    "id": "717d2f8d-da59-479c-9db5-10f6d6249cc5"
                },
                {
                    "group": "Product properties",
                    "isReadOnly": false,
                    "isManageable": true,
                    "isNew": false,
                    "catalogId": "b61aa9d1d0024bc4be12d79bf5786e9f",
                    "name": "Style",
                    "required": false,
                    "dictionary": true,
                    "multivalue": true,
                    "multilanguage": false,
                    "hidden": false,
                    "valueType": "ShortText",
                    "type": "Product",
                    "values": [
                       
                    ],
                    "attributes": [
                        {
                            "value": "Style",
                            "name": "DisplayNameen-US",
                            "id": "fe4e5329bf9342738be7815e49de8043"
                        }
                    ],
                    "displayNames": [
                        {
                            "name": "Style",
                            "languageCode": "en-US"
                        }
                    ],
                    "isInherited": true,
                    "id": "ed41aaee-e695-470f-88d2-d8c0dcf31e2d"
                },
                {
                    "group": "Product properties",
                    "isReadOnly": false,
                    "isManageable": true,
                    "isNew": false,
                    "catalogId": "b61aa9d1d0024bc4be12d79bf5786e9f",
                    "categoryId": "ac56b04c5da54f038c53852f62810f27",
                    "name": "Type",
                    "required": false,
                    "dictionary": true,
                    "multivalue": true,
                    "multilanguage": false,
                    "hidden": false,
                    "valueType": "ShortText",
                    "type": "Product",
                    "values": [
                    ],
                    "attributes": [
                        {
                            "value": "Heel Type",
                            "name": "DisplayNameen-US",
                            "id": "86cbe83a121a48b980364bbae65e9f59"
                        }
                    ],
                    "displayNames": [
                        {
                            "name": "Heel Type",
                            "languageCode": "en-US"
                        }
                    ],
                    "isInherited": true,
                    "id": "85a85b5a-c197-4e2c-8505-0ee9cdebae3c"
                }
            ];

        };

        initializeBlade();
    }]);
