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

            _.each(blade.currentEntities,
                function (prop) {
                    prop.isChanged = false;
                    prop.group = 'All properties';
                });

            $scope.resetFilter();

            if ($localStorage.entryPropertyFilters) {
                var savedFilteredProperties = $localStorage.entryPropertyFilters[authService.id][blade.currentEntity.categoryId];
                if (savedFilteredProperties && savedFilteredProperties.length > 0) {
                    blade.filtered = true;
                    _.each(blade.currentEntities,
                        function (prop) {
                            prop.isSelected = false;
                        });
                    _.each(savedFilteredProperties,
                        function (propertyName) {
                            var filteredProperties = _.filter(blade.currentEntities,
                                function(property) {
                                    return property.name.toLocaleLowerCase() === propertyName.toLocaleLowerCase();
                                });

                            if (filteredProperties.length) {
                                _.each(filteredProperties,
                                    function(prop) {
                                        prop.isSelected = true;
                                    });
                            } else {
                                $scope.resetFilter();
                            }
                        });
                }
            }
        }

        $scope.resetFilter = function () {
            blade.filtered = false;
            _.each(blade.currentEntities,
                function (prop) {
                    prop.isSelected = true;
                });
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

		$scope.$watch("blade.currentEntities", function () {
            $scope.isValid = formScope && formScope.$valid && $scope.hasChangedProperties(blade.currentEntities);
		}, true);

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
                        categoryId: blade.currentEntity.categoryId,
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
                    if ($localStorage.entryPropertyFilters) {
                        
                        if ($localStorage.entryPropertyFilters[authService.id][blade.currentEntity.categoryId]) {
                            $localStorage.entryPropertyFilters[authService.id][blade.currentEntity.categoryId] = [];
                        }
                            
                    }
                    $scope.resetFilter();
                },
                canExecuteMethod: function () {
                    return blade.filtered;
                }
            }
        ];

        $scope.$watch('blade.currentEntities', function (changedProperties, oldProperties) {
            _.each(changedProperties,
                function(changedItem) {
                    var oldItem = _.find(oldProperties, function (item) { return item.id === changedItem.id; });
                    _.each(changedItem.values,
                        function(newValue) {
                            var oldValue = _.find(oldItem.values, function (value) {
                                return angular.equals(value, newValue);
                            });
                            if (!oldValue) {
                                changedItem.isChanged = true;
                            }
                        });
                });
        }, true);

		blade.isLoading = false;
		initialize(blade.currentEntity);
	}]);
