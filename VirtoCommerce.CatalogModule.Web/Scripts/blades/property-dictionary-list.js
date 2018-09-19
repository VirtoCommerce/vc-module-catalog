angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.propertyDictionaryListController',
        ['$scope', '$filter', 'platformWebApp.dialogService', 'platformWebApp.settings', 'platformWebApp.bladeNavigationService', 'platformWebApp.uiGridHelper', function ($scope, $filter, dialogService, settings, bladeNavigationService, uiGridHelper) {
            var blade = $scope.blade;
            $scope.blade.isLoading = false;
            blade.headIcon = 'fa-book';
            $scope.currentEntities = [];
            $scope.searchPhrase = '';

            blade.toolbarCommands = [
                {
                    name: "platform.commands.add", icon: 'fa fa-plus',
                    executeMethod: function () {
                        $scope.selectNode({ values: [], isNew: true })
                    },
                    canExecuteMethod: function () {
                        return true;
                    }
                },
                {
                    name: "platform.commands.delete", icon: 'fa fa-trash-o',
                    executeMethod: function () {
                        $scope.deleteList($scope.gridApi.selection.getSelectedRows());
                    },
                    canExecuteMethod: function () {
                        return $scope.gridApi && _.any($scope.gridApi.selection.getSelectedRows());
                    },
                }
                
            ];

            blade.refresh = function () {

                blade.languages = blade.property.multilanguage ? blade.languages : [];
                $scope.search();
            };

            $scope.search = function (searchPhrase) {

                $scope.currentEntities = _.map(_.groupBy(blade.property.dictionaryValues, 'alias'), function (values, key) {
                    var dictItem = { alias: key, valueId: values[0].valueId };
                    blade.languages.forEach(function (lang) {
                        var dictValue = _.find(values, function (x) { return x.languageCode === lang || (!x.languageCode && lang === blade.defaultLanguage) });
                        dictItem[lang] = dictValue ? dictValue.value : undefined;
                    });
                    return dictItem;
                });

                if (searchPhrase) {
                    $scope.currentEntities = _.filter($scope.currentEntities, function (x) {
                        return _.some(blade.languages, function (lang) {
                            return x[lang] && x[lang].toLowerCase().indexOf(searchPhrase.toLowerCase()) !== -1;
                        });
                    });
                }
            };

            // ui-grid
            $scope.setGridOptions = function (gridOptions) {
                uiGridHelper.initialize($scope, gridOptions, function (gridApi) {
                    uiGridHelper.bindRefreshOnSortChanged($scope);
                });
            };
           
            $scope.deleteList = function (selection) {
                var dialog = {
                    id: "confirmDeletePropertyValue",
                    title: "catalog.dialogs.dictionary-values-delete.title",
                    message: "catalog.dialogs.dictionary-values-delete.message",
                    callback: function (remove) {
                        if (remove) {
                            bladeNavigationService.closeChildrenBlades(blade, function () {
                                selection.forEach(function (x) {
                                    const index = $scope.currentEntities.indexOf(x);
                                    $scope.currentEntities.splice(index, 1);     
                                });
                                $scope.saveChanges();                                                 
                            });
                        }
                    }
                };
                dialogService.showConfirmationDialog(dialog);
            };

            $scope.deleteDictItem = function (selectedDictItem) {
                $scope.deleteList([selectedDictItem]);
            }

            $scope.saveChanges = function () {
                blade.property.dictionaryValues = [];
                $scope.currentEntities.forEach(function (x) {                  
                    if (blade.property.multilanguage) {
                        var dictPropValuePrototype = { alias: x.alias, valueId: x.valueId };
                        blade.languages.forEach(function (lang) {
                            var dictPropValue = angular.extend({}, dictPropValuePrototype);
                            dictPropValue.languageCode = blade.property.multilanguage ? lang : undefined;
                            dictPropValue.value = x[lang];
                            blade.property.dictionaryValues.push(dictPropValue);
                        });
                    }
                    else {
                        x.value = x.alias;
                        blade.property.dictionaryValues.push(x);
                    }
                });
            };

            $scope.selectNode = function (selectedDictItem) {

                if (selectedDictItem.alias) {
                    $scope.selectedNodeId = selectedDictItem.alias;
                }
                var newBlade = {
                    id: 'propertyDictionaryDetails',
                    title: 'catalog.blades.property-dictionary.labels.dictionary-edit',
                    controller: 'virtoCommerce.catalogModule.propertyDictionaryDetailsController',
                    template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/property-dictionary-details.tpl.html',
                    dictionaryItem: selectedDictItem,
                    property: blade.property,
                    languages: blade.languages,
                    defaultLanguage: blade.defaultLanguage,

                    onSaveChanges: function (dictItem) {
                        angular.extend(selectedDictItem, dictItem);
                        if (dictItem.isNew) {
                            $scope.currentEntities.push(dictItem);
                            $scope.selectedNodeId = dictItem.alias;
                        }
                        else {
                            angular.copy(dictItem, selectedDictItem);
                        }
                        $scope.saveChanges();
                    },
                };
                bladeNavigationService.showBlade(newBlade, blade);
            };

            blade.refresh();

        }]);
