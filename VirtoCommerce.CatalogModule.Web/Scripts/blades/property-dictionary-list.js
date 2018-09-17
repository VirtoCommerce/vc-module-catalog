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
                        $scope.selectNode({ values: [] })
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

                blade.languages = blade.property.multilanguage ? blade.languages : [blade.defaultLanguage];

                $scope.currentEntities = _.map(_.groupBy(blade.property.dictionaryValues, 'alias'), function (values, key) {
                    var dictItem = { alias: key };
                    blade.languages.forEach(function (lang) {
                        var dictValue = _.find(values, function (x) { return x.languageCode === lang || (!x.languageCode && lang === blade.defaultLanguage) });
                        dictItem[lang] = dictValue ? dictValue.value : undefined;
                    });
                    dictItem.label = dictItem[blade.defaultLanguage];
                    return dictItem;
                });
            };

            $scope.search = function (searchPhrase) {
                blade.refresh();
            };

            // ui-grid
            $scope.setGridOptions = function (gridOptions) {
                uiGridHelper.initialize($scope, gridOptions, function (gridApi) {
                    uiGridHelper.bindRefreshOnSortChanged($scope);
                });
            };

            function guid() {
                function s4() {
                    return Math.floor((1 + Math.random()) * 0x10000)
                        .toString(16)
                        .substring(1);
                }
                return s4() + s4() + s4() + s4() + s4() + s4() + s4() + s4();
            }

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
                    var dictPropValuePrototype = { alias: x.alias, propertyId: blade.property.id };
                    blade.languages.forEach(function (lang) {
                        var dictPropValue = angular.extend({}, dictPropValuePrototype);
                        dictPropValue.languageCode = blade.property.multilanguage ? lang : undefined;
                        dictPropValue.value = x[lang];
                        blade.property.dictionaryValues.push(dictPropValue);
                    });
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
                        if (!dictItem.alias) {
                            dictItem.alias = guid();
                            dictItem.label = dictItem[blade.defaultLanguage];
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
