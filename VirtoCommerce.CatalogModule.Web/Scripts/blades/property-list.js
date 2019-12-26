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
            blade.currentEntities = angular.copy(entity.properties);
            blade.filteredProperties = [];
            //Apply stored filters
            if ($localStorage.propertyFilter) {
                applyFilter($localStorage.propertyFilter[authService.userName]);
            }

        }

        $scope.isPropertyChanged = function (property) {
            if (property) {
                var oldItem = _.find(blade.currentEntity.properties, function (x) { return x.id === property.id; });
                if (oldItem) {
                    return !angular.equals(property, oldItem);
                }
            }
            return false;
        }

        $scope.isPropertyVisible = function (property) {
            if (blade.filteredProperties && blade.filteredProperties.length > 0) {
                return blade.filteredProperties.includes(property.name.toLowerCase());
            }
            return true;
        }

        function applyFilter(filteredProperties)
        {
            if (filteredProperties && filteredProperties.length > 0) {
                blade.filteredProperties = filteredProperties.map(function (x) { return x.toLowerCase(); });
            }
        }

        $scope.resetFilter = function () {
            saveFilter([]);
            blade.filteredProperties = [];
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

        $scope.$watch('blade.currentEntities', function () {
            $scope.isValid = formScope && formScope.$valid;
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
                                values: []                             
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
                    $scope.editPropertyFilter();             
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
                    return blade.filteredProperties.length > 0;
                }
            }
        ];

        $scope.editPropertyFilter = function () {
            var newBlade = {
                id: "propertySelector",
                entityType: "product",
                properties: blade.currentEntities,
                selectedProperties: blade.filteredProperties,
                controller: 'virtoCommerce.catalogModule.propertySelectorController',
                template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/property-selector.tpl.html',
                onSelected: function (filteredProperties) {
                    var filteredPropertiesNames = filteredProperties.map(function (x) { return x.name; });
                    saveFilter(filteredPropertiesNames);
                    applyFilter(filteredPropertiesNames);
                }
            };
            bladeNavigationService.showBlade(newBlade, blade);
        }
        //save filters to localStorage
        function saveFilter(filteredPropertiesNames) {
            var filter = [];
            filter[authService.userName] = filteredPropertiesNames;
            if ($localStorage.propertyFilter) {
                angular.extend($localStorage.propertyFilter, filter);
            } else {
                $localStorage.propertyFilter = filter;
            }
        }

		blade.isLoading = false;
		initialize(blade.currentEntity);
	}]);
