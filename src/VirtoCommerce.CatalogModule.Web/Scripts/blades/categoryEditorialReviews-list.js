angular.module('virtoCommerce.catalogModule')
.controller('virtoCommerce.catalogModule.categoryEditorialReviewsListController', ['$timeout', '$scope', 'platformWebApp.bladeNavigationService', 'platformWebApp.uiGridHelper', 'platformWebApp.dialogService', function ($timeout, $scope, bladeNavigationService, uiGridHelper, dialogService) {
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
        blade.subtitle = 'catalog.blades.categoryEditorialReviews-list.subtitle';
        blade.selectNode = $scope.openBlade;
    };

    $scope.openBlade = function (node) {
        if (node) {
            $scope.selectedNodeId = node.id;
        }
        var newBlade = {
            id: 'categoryEditorialReview',
            currentEntity: node,
            category: blade.category,
            catalog: blade.catalog,
            languages: blade.catalog.languages,
            controller: 'virtoCommerce.catalogModule.categoryEditorialReviewDetailController',
            template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/categoryEditorialReview-detail.tpl.html'
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
                            blade.category.reviews.splice(blade.category.reviews.indexOf(x), 1);
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

    // open blade for new review 
    if (!_.some(blade.category.reviews)) {
        $timeout($scope.openBlade, 60, false);
    }

    initialize(blade.category);
}]);
