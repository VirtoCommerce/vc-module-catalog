angular.module('virtoCommerce.catalogModule')
.controller('virtoCommerce.catalogModule.categoryDescriptionsListController', ['$timeout', '$scope', 'platformWebApp.bladeNavigationService', 'platformWebApp.uiGridHelper', 'platformWebApp.dialogService', function ($timeout, $scope, bladeNavigationService, uiGridHelper, dialogService) {
    var blade = $scope.blade;

    $scope.selectedNodeId = null; // need to initialize to null
    blade.isLoading = false;
    blade.refresh = function (category) {
        initialize(category);
    };

    function initialize(category) {
        blade.headIcon = 'fa fa-comments';
        blade.category = category;
        blade.title = blade.category.name;
        blade.subtitle = 'catalog.blades.categoryDescriptions-list.subtitle';
        blade.selectNode = $scope.openBlade;
    }

    $scope.openBlade = function (node) {
        if (node) {
            $scope.selectedNodeId = node.id;
        }
        var newBlade = {
            id: 'categoryDescription',
            currentEntity: node,
            category: blade.category,
            catalog: blade.catalog,
            languages: blade.catalog.languages,
            controller: 'virtoCommerce.catalogModule.categoryDescriptionDetailController',
            template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/categoryDescription-detail.tpl.html'
        };
        bladeNavigationService.showBlade(newBlade, $scope.blade);
    }        
   

    $scope.delete = function (data) {
        deleteList([data]);
    };

    function deleteList(selection) {
        var dialog = {
            id: "confirmDelete",
            title: "catalog.dialogs.review-delete.title",
            message: "catalog.dialogs.review-delete.message",
            callback: function (remove) {
                if (remove) {
                    bladeNavigationService.closeChildrenBlades(blade, function () {
                        _.each(selection, function (x) {
                            blade.category.descriptions.splice(blade.category.descriptions.indexOf(x), 1);
                        });
                    });
                }
            }
        };
        dialogService.showConfirmationDialog(dialog);
    }

    blade.toolbarCommands = [
        {
            name: "platform.commands.add", icon: 'fas fa-plus',
            executeMethod: function () {
                $scope.openBlade();
            },
            canExecuteMethod: function () {
                return true;
            }
        },
        {
            name: "platform.commands.delete", icon: 'fas fa-trash-alt',
            executeMethod: function () { deleteList($scope.gridApi.selection.getSelectedRows()); },
            canExecuteMethod: function () {
                return $scope.gridApi && _.any($scope.gridApi.selection.getSelectedRows());
            }
        }

    ];

    // ui-grid
    $scope.setGridOptions = function (gridOptions) {
        uiGridHelper.initialize($scope, gridOptions);
    };

    // open blade for new description
    if (!_.some(blade.category.descriptions)) {
        $timeout($scope.openBlade, 60, false);
    }

    initialize(blade.category);
}]);
