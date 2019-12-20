angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.propertyListController', ['$scope', 'virtoCommerce.catalogModule.properties', 'platformWebApp.bladeNavigationService', 'virtoCommerce.catalogModule.propDictItems', '$localStorage', 'platformWebApp.authService', function ($scope, properties, bladeNavigationService, propDictItems, $localStorage, authService) {
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
            blade.filteredProperties = angular.copy(entity.properties);
            _.each(blade.currentEntities,
                function (prop) {
                    prop.isChanged = false;
                });

            if ($localStorage.entryPropertyFilters) {
                var savedFilteredProperties = $localStorage.entryPropertyFilters[authService.id];
                if (savedFilteredProperties && savedFilteredProperties.length > 0) {
                    savedFilteredProperties = savedFilteredProperties.map(function(x) { return x.toLowerCase(); });
                    blade.filtered = true;
                    blade.filteredProperties = _.filter(blade.currentEntities,
                        function (property) {
                            return savedFilteredProperties.includes(property.name.toLowerCase());
                        });
                }
            }
        }

        $scope.resetFilter = function () {
            blade.filtered = false;
            blade.filteredProperties = angular.copy(blade.currentEntities);
        };

        $scope.hasChangedProperties = function (propertyList) {
            return _.filter(propertyList,
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

        $scope.propertySearch = function(row) {
            var filteredPropertiesNames = blade.filteredProperties.map(function (x) { return x.name });
            if (filteredPropertiesNames.includes(row.name)) {
                return true;
            }
            return false;
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
        $scope.setForm = function(form) {
            formScope = form;
        };

		blade.headIcon = 'fa-gear';

		blade.toolbarCommands = [
			{
				name: "catalog.commands.add-property", icon: 'fa fa-plus',
				executeMethod: function () {
					if (blade.entityType === "product") {
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
					}
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
                        selectedProperties : blade.filteredProperties,
                        controller: 'virtoCommerce.catalogModule.propertySelectorController',
                        template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/property-selector.tpl.html',
                        onSelected: function (includedProperties) {
                            blade.filtered = true;
                            var includedPropertiesIds = includedProperties.map(function (x) { return x.id; });
                            blade.filteredProperties = _.filter(blade.currentEntities,
                                function (property) {
                                    return includedPropertiesIds.includes(property.id);
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
                    if ($localStorage.entryPropertyFilters) {
                        $localStorage.entryPropertyFilters[authService.id] = [];
                    }
                    $scope.resetFilter();
                },
                canExecuteMethod: function () {
                    return blade.filtered;
                }
            }
        ];

        $scope.$watch('blade.currentEntities', function (changedProperties, oldProperties) {
            $scope.isValid = formScope && formScope.$valid && $scope.hasChangedProperties(blade.currentEntities);
            _.each(changedProperties,
                function (changedItem) {
                    var oldItem = _.find(oldProperties, function (item) { return item.id === changedItem.id; });
                    if (oldItem) {
                        _.each(changedItem.values,
                            function (newValue) {
                                var oldValue = _.find(oldItem.values, function (value) {
                                    return angular.equals(value, newValue);
                                });
                                if (!oldValue) {
                                    changedItem.isChanged = true;
                                }
                            });
                    }
                });
        }, true);

		blade.isLoading = false;
		initialize(blade.currentEntity);
	}]);
