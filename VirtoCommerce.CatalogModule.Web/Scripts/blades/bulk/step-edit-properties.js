angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.editPropertiesActionStepController', ['$scope', 'virtoCommerce.catalogModule.properties', 'platformWebApp.bladeNavigationService', 'virtoCommerce.catalogModule.propDictItems', function ($scope, properties, bladeNavigationService, propDictItems) {
        var blade = $scope.blade;
        $scope.isValid = false;
        blade.refresh = function () {
            blade.parentBlade.refresh();
        };

        function initialize() {
            
            blade.subtitle = 'catalog.blades.property-list.subtitle';
           // blade.currentEntity = entity;

            blade.currentEntities = angular.copy(blade.properties);

        };

        $scope.saveChanges = function () {
            if (blade.onSelected) {
                blade.onSelected(blade.currentEntities);
            }
            $scope.bladeClose();
        };

        $scope.getPropertyDisplayName = function (prop) {
            return _.first(_.map(_.filter(prop.displayNames, function (x) { return x && x.languageCode.startsWith(blade.defaultLanguage); }), function (x) { return x.name; }));
        };

        //$scope.editProperty = function (prop) {
        //    if (prop.isManageable) {
        //        var newBlade = {
        //            id: 'editCategoryProperty',
        //            currentEntityId: prop ? prop.id : undefined,
        //            categoryId: blade.categoryId,
        //            catalogId: blade.catalogId,
        //            defaultLanguage: blade.defaultLanguage,
        //            languages: blade.languages,
        //            controller: 'virtoCommerce.catalogModule.propertyDetailController',
        //            template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/property-detail.tpl.html'
        //        };
        //        bladeNavigationService.showBlade(newBlade, blade);
        //    } else {
        //        editUnmanageable({
        //            title: 'catalog.blades.item-property-detail.title',
        //            origEntity: prop
        //        });
        //    }
        //};

        //function editUnmanageable(bladeData) {
        //    var newBlade = {
        //        id: 'editItemProperty',
        //        properties: blade.currentEntities,
        //        controller: 'virtoCommerce.catalogModule.itemPropertyDetailController',
        //        template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/item-property-detail.tpl.html'
        //    };
        //    angular.extend(newBlade, bladeData);

        //    bladeNavigationService.showBlade(newBlade, blade);
        //}

        $scope.getPropValues = function (propId, keyword, countToSkip, countToTake) {
            return propDictItems.search({
                propertyIds: [propId],
                searchPhrase: keyword,
                skip: countToSkip,
                take: countToTake
            }).$promise.then(function (result) {
                return result;
            });
        };

        var formScope;
        $scope.setForm = function (form) {
            formScope = form;
        }

        $scope.$watch("blade.currentEntities", function () {
            $scope.isValid = formScope && formScope.$valid;
        }, true);

        blade.headIcon = 'fa-gear';

        blade.toolbarCommands = [];
        blade.isLoading = false;
        initialize(blade.currentEntity);
    }]);
