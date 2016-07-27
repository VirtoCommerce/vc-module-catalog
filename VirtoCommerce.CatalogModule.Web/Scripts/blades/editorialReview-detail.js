angular.module('virtoCommerce.catalogModule')
.controller('virtoCommerce.catalogModule.editorialReviewDetailController', ['$scope', 'platformWebApp.bladeNavigationService', 'virtoCommerce.catalogModule.items', 'platformWebApp.settings', function ($scope, bladeNavigationService, items, settings) {
	var blade = $scope.blade;
	blade.isLoading = false;

	blade.refresh = function (item) {
		initilize(item);
	};

	function initilize(item) {
		blade.title = 'catalog.blades.editorialReview-detail.title';
		blade.subtitle = 'catalog.blades.editorialReview-detail.subtitle';

		if (!blade.item.reviews) {
			blade.item.reviews = [];
		};
		if (!blade.currentEntity) {
			blade.currentEntity = {};
		};		

		blade.origEntity = blade.currentEntity;
		blade.currentEntity = angular.copy(blade.currentEntity);
		if(!blade.currentEntity.languageCode)
		{
			blade.currentEntity.languageCode = blade.item.catalog.defaultLanguage.languageCode;
		}
	};

    settings.getValues({ id: 'Catalog.EditorialReviewTypes' }, function (data) {
    	$scope.types = data;
    	if (!blade.currentEntity.reviewType) {
    		blade.currentEntity.reviewType = $scope.types[0];
    	}
    });
  
    $scope.isValid = true;

    $scope.saveChanges = function () {
    	var existReview = _.find(blade.item.reviews, function (x) { return x == blade.origEntity; });
    	if (!existReview) {
    		blade.item.reviews.push(blade.origEntity);
    	};
    	angular.copy(blade.currentEntity, blade.origEntity);		
    	$scope.bladeClose();
    };   

    blade.headIcon = 'fa-comments';

    blade.toolbarCommands = [];

    $scope.openDictionarySettingManagement = function () {
        var newBlade = {
            id: 'settingDetailChild',
            isApiSave: true,
            currentEntityId: 'Catalog.EditorialReviewTypes',
            parentRefresh: function (data) { $scope.types = data; },
            controller: 'platformWebApp.settingDictionaryController',
            template: '$(Platform)/Scripts/app/settings/blades/setting-dictionary.tpl.html'
        };
        bladeNavigationService.showBlade(newBlade, blade);
    };

    initilize(blade.currentEntity);

}]);
