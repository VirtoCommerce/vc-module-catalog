angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.automaticLinksWidgetController', [
        '$scope', 'platformWebApp.bladeNavigationService', 'virtoCommerce.catalogModule.listEntries',
        function ($scope, bladeNavigationService, listEntries) {
            var blade = $scope.blade;

            $scope.automaticLinksCount = '...';

            $scope.$watch('blade.currentEntity', function (category) {
                if (category) {
                    const criteria = {
                        categoryIds: [category.id],
                        objectType: 'CatalogProduct',
                        isAutomatic: true,
                        take: 0,
                    };

                    listEntries.searchlinks(criteria, function (data) {
                        $scope.automaticLinksCount = data.totalCount;
                    });
                }
            });

            $scope.openAutomaticLinksBlade = function () {
                var newBlade = {
                    id: "automaticLinks",
                    controller: 'virtoCommerce.catalogModule.automaticLinksController',
                    template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/automatic-links.html',
                    categoryId: blade.currentEntity.id,
                };
                bladeNavigationService.showBlade(newBlade, blade);
            };
        }]);
