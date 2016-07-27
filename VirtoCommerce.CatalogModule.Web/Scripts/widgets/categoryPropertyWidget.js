angular.module('virtoCommerce.catalogModule')
.controller('virtoCommerce.catalogModule.categoryPropertyWidgetController', ['$scope', 'platformWebApp.bladeNavigationService', function ($scope, bladeNavigationService) {
	var blade = $scope.blade;

	$scope.openCategoryPropertyBlade = function () {
		var newBlade = {
			id: "categoryPropertyDetail",
			categoryId: blade.currentEntity.id,
			entityType: "category",
			propGroups: [{ title: 'catalog.properties.category', type: 'Category' }, { title: 'catalog.properties.product', type: 'Product' }, { title: 'catalog.properties.variation', type: 'Variation' }],
			currentEntity: blade.currentEntity,
			controller: 'virtoCommerce.catalogModule.propertyListController',
			template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/property-list.tpl.html'
		};

		bladeNavigationService.showBlade(newBlade, blade);
	};

}]);
