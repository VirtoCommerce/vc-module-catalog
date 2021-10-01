angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.categoryEditorialReviewWidgetController', ['$scope', 'platformWebApp.bladeNavigationService', function ($scope, bladeNavigationService) {
    $scope.currentBlade = $scope.widget.blade;

    $scope.openBlade = function () {
        var blade = {
            id: "categoryEditorialReviewsList",
            category: $scope.currentBlade.currentEntity,
            catalog: $scope.currentBlade.catalog,
            controller: 'virtoCommerce.catalogModule.categoryEditorialReviewsListController',
            template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/categoryEditorialReviews-list.tpl.html'
        };

        bladeNavigationService.showBlade(blade, $scope.currentBlade);
    };

}]);
