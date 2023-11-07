angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.measureUnitsWidgetController',
        ['$scope', 'platformWebApp.bladeNavigationService', '$translate',
            function ($scope, bladeNavigationService, $translate) {
                var blade = $scope.widget.blade;

                $scope.openBlade = function () {
                    var newBlade = {
                        id: "pmPhoneBlade",
                        title: blade.title + $translate.instant('catalog.blades.measure-units-list.title'),
                        currentEntity: blade.currentEntity,
                        subtitle: 'catalog.blades.measure-units-list.subtitle',
                        controller: 'virtoCommerce.catalogModule.measureUnitsListController',
                        template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/measures/measure-units-list.tpl.html'
                    };
                    bladeNavigationService.showBlade(newBlade, $scope.blade);
                };
            }
        ]
    );
