angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.propertyListController', ['$scope', 'virtoCommerce.catalogModule.properties', 'platformWebApp.bladeNavigationService', 'virtoCommerce.catalogModule.propDictItems', function ($scope, properties, bladeNavigationService, propDictItems) {
		var blade = $scope.blade;
		$scope.isValid = false;
		blade.refresh = function (entity) {
			if (entity) {
				initialize(entity);
			}
			else {
				blade.parentBlade.refresh();
			}
		};

		function initialize(entity) {
			blade.title = entity.name;
			blade.subtitle = 'catalog.blades.property-list.subtitle';
            blade.currentEntity = entity;
            blade.filtered = false;
			blade.currentEntities = angular.copy(entity.properties);

            _.each(blade.currentEntities,
                function (prop) {
                    prop.isChanged = false;
                    prop.group = 'All properties';
                });

            $scope.resetFilter();
        };

        $scope.resetFilter = function () {
            blade.filtered = false;
            _.each(blade.currentEntities,
                function (prop) {
                    prop.isSelected = true;
                });
        };

        $scope.hasChangedProperties = function (properties) {
            return _.filter(properties,
                function (prop) {
                    return prop.isChanged;
                }).length;
        };

		$scope.saveChanges = function () {
			blade.currentEntity.properties = blade.currentEntities;
			$scope.bladeClose();
		};

		$scope.getPropertyDisplayName = function (prop) {
			return _.first(_.map(_.filter(prop.displayNames, function (x) { return x && x.languageCode.startsWith(blade.defaultLanguage); }), function (x) { return x.name; }));
		};

		$scope.editProperty = function (prop) {
			if (prop.isManageable) {
				var newBlade = {
					id: 'editCategoryProperty',
					currentEntityId: prop ? prop.id : undefined,
					categoryId: blade.categoryId,
					catalogId: blade.catalogId,
					defaultLanguage: blade.defaultLanguage,
					languages: blade.languages,
					controller: 'virtoCommerce.catalogModule.propertyDetailController',
					template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/property-detail.tpl.html'
				};
				bladeNavigationService.showBlade(newBlade, blade);
			} else {
				editUnmanageable({
					title: 'catalog.blades.item-property-detail.title',
					origEntity: prop
				});
			}
		};

		function editUnmanageable(bladeData) {
			var newBlade = {
				id: 'editItemProperty',
				properties: blade.currentEntities,
				controller: 'virtoCommerce.catalogModule.itemPropertyDetailController',
				template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/item-property-detail.tpl.html'
			};
			angular.extend(newBlade, bladeData);

			bladeNavigationService.showBlade(newBlade, blade);
		}

        $scope.getPropValues = function (propId, keyword, countToSkip, countToTake) {
            return propDictItems.search({
                propertyIds: [propId],
                searchPhrase: keyword,
                skip: countToSkip,
                take: countToTake
            }).$promise.then(function(result) {
                return result;
            });
        };

		var formScope;
		$scope.setForm = function (form) {
			formScope = form;
		}

		$scope.$watch("blade.currentEntities", function () {
            $scope.isValid = formScope && formScope.$valid && $scope.hasChangedProperties(blade.currentEntities);
		}, true);

		blade.headIcon = 'fa-gear';

		blade.toolbarCommands = [
			{
				name: "catalog.commands.add-property", icon: 'fa fa-plus',
				executeMethod: function () {
					if (blade.entityType == "product") {
						editUnmanageable({
							isNew: true,
							title: 'catalog.blades.item-property-detail.title-new',
							origEntity: {
								type: "Product",
								valueType: "ShortText",
                                values: [],
                                isChanged: true,
                                isSelected: true
							}
						});
					} else {
						$scope.editProperty({ isManageable: true });
					};
				},
				canExecuteMethod: function () {
					return true;
				}
            },
            {
                name: "catalog.blades.property-list.labels.add-filter", icon: 'fa fa-filter',
                executeMethod: function () {
                    var newBlade = {
                        id: "propertySelector",
                        entityType: "product",
                        properties: blade.currentEntities,
                        controller: 'virtoCommerce.catalogModule.propertySelectorController',
                        template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/property-selector.tpl.html',
                        onSelected: function (includedProperties) {
                            blade.filtered = true;


                            _.each(blade.currentEntities,
                                function (property) {
                                    var foundProperty = _.find(includedProperties,
                                        function (selectedProperty) {
                                            return selectedProperty.id === property.id;
                                        });
                                    property.isSelected = foundProperty !== undefined ? true : false;
                                });
                        }
                    };
                    bladeNavigationService.showBlade(newBlade, blade);
                },
                canExecuteMethod: function () {
                    return true;
                }
            },
            {
                name: "catalog.blades.property-list.labels.reset-filter", icon: 'fa fa-undo',
                executeMethod: function () {
                    $scope.resetFilter();
                },
                canExecuteMethod: function () {
                    return blade.filtered;
                }
            }
		];
		blade.isLoading = false;
		initialize(blade.currentEntity);
	}]);
