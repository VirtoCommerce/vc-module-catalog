angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.catalogPropertyGroupsWidgetController', ['$scope', 'platformWebApp.bladeNavigationService', function ($scope, bladeNavigationService) {
	var blade = $scope.blade;

        $scope.openBlade = function () {
		var newBlade = {
            id: "catalogPropertyGroupList",
            catalog: blade.currentEntity,
			catalogId: blade.currentEntity.id,
            languages: _.pluck(_.sortBy(blade.currentEntity.languages, function (x) {
                return x.priority;
            }), 'languageCode'),
			defaultLanguage: blade.currentEntity.defaultLanguage.languageCode,
            controller: 'virtoCommerce.catalogModule.propertyGroupListController',
            template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/property-group-list.tpl.html'
		};
		bladeNavigationService.showBlade(newBlade, blade);
	};
}]);
