angular.module('virtoCommerce.catalogModule')
.controller('virtoCommerce.catalogModule.itemAssociationsListController', ['$scope', 'platformWebApp.bladeNavigationService', 'platformWebApp.dialogService',  'filterFilter', 'uiGridConstants', 'platformWebApp.uiGridHelper', function ($scope, bladeNavigationService, dialogService, filterFilter, uiGridConstants, uiGridHelper) {
    $scope.uiGridConstants = uiGridConstants;
    var blade = $scope.blade;

    blade.isLoading = false;
    blade.refresh = function (item) {
    	initialize(item);
    };

    function initialize(item) {
    	blade.title = item.name;
    	blade.subtitle = 'catalog.widgets.itemAssociations.blade-subtitle';
    	blade.item = item;
    };
   
    $scope.selectNode = function (listItem) {
    	$scope.selectedNodeId = listItem.associatedObjectId;
    	var newBlade = {
    		id: 'associationDetail',
    		itemId: listItem.associatedObjectId,
            catalog: blade.catalog,
    		controller: 'virtoCommerce.catalogModule.itemDetailController',
    		template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/item-detail.tpl.html'
    	};
        if (listItem.associatedObjectType == 'category')
        {
        	newBlade.currentEntityId = listItem.associatedObjectId;
        	newBlade.controller = 'virtoCommerce.catalogModule.categoryDetailController';
			newBlade.template = 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/category-detail.tpl.html';
        }        
        bladeNavigationService.showBlade(newBlade, blade);
    };

    $scope.deleteList = function (list) {
    	bladeNavigationService.closeChildrenBlades(blade, function () {
    		var undeletedEntries = _.difference(blade.item.associations, list);
    		blade.item.associations = undeletedEntries;
    	});
    }

    $scope.edit = function(listItem) {
        var newBlade = {
            id: 'associationEditDetail',
            title: listItem.associatedObjectName,
            subtitle: 'catalog.blades.item-association-detail.subtitle',
            origEntity: listItem,
            catalog: blade.catalog,
            controller: 'virtoCommerce.catalogModule.itemAssociationDetailController',
            template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/item-association-detail.tpl.html'
        };
        bladeNavigationService.showBlade(newBlade, blade);
    }

    function openAddEntityWizard() {
        var newBlade = {
            id: "associationWizard",
            catalog: blade.catalog,
            item : blade.item,
            controller: 'virtoCommerce.catalogModule.associationWizardController',
            template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/wizards/newAssociation/association-wizard.tpl.html'
        };
        bladeNavigationService.showBlade(newBlade, blade);
    }

    blade.toolbarCommands = [
        {
            name: "platform.commands.add", icon: 'fa fa-plus',
            executeMethod: function () {
                openAddEntityWizard();
            },
            canExecuteMethod: function () {
                return true;
            }
        },
        {
            name: "platform.commands.delete", icon: 'fa fa-trash-o',
            executeMethod: function () {
                $scope.deleteList($scope.gridApi.selection.getSelectedRows());
            },
            canExecuteMethod: function () {
                return $scope.gridApi && _.any($scope.gridApi.selection.getSelectedRows());
            }
        }
    ];

    // ui-grid
    $scope.setGridOptions = function (gridOptions) {
        uiGridHelper.initialize($scope, gridOptions,
        function (gridApi) {
            // gridApi.grid.registerRowsProcessor($scope.singleFilter, 90);
            gridApi.draggableRows.on.rowFinishDrag($scope, function () {
                for (var i = 0; i < blade.item.associations.length; i++) {
                	blade.item.associations[i].priority = i + 1;
                }               
            });
        });
    };

    initialize(blade.item);
}]);
