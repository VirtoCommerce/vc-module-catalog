angular.module('virtoCommerce.catalogModule')
.controller('virtoCommerce.catalogModule.catalogLanguagesController', ['$scope', 'platformWebApp.settings', 'platformWebApp.bladeNavigationService', function ($scope, settings, bladeNavigationService) {
    var blade = $scope.blade;
    blade.updatePermission = 'catalog:update';
    blade.headIcon = 'fa fa-language';
    var promise = settings.getValues({ id: 'VirtoCommerce.Core.General.Languages' }).$promise;
    $scope.languages = [];

    function initializeBlade(data) {
        blade.data = data;

        promise.then(function (promiseData) {
            $scope.languages = promiseData;

            // intialize priority
            var startPosition = 1;
            var needToInitialize = _.every(data.languages, function (x) {
                return x.priority === 0;
            });
            if (needToInitialize) {
                _.each(data.languages, function (x) {
                    if (!x.isDefault) {
                        x.priority = startPosition;
                        startPosition++;
                    }
                });
            }

            // create language map and union
            var valuesUnion = _.map(data.languages, function (x) {
                return {
                    isActive: true,
                    languageCode: x.languageCode,
                    priority: x.priority,

                    catalogId: x.catalogId,
                    id: x.id,
                    isDefault: x.isDefault
                }
            });

            valuesUnion = _.sortBy(valuesUnion, function (x) {
                return x.priority;
            });

            startPosition = _.max(valuesUnion, function (x) {
                return x.priority;
            }).priority;

            _.each($scope.languages, function (x) {
                var language = _.find(valuesUnion, function (y) {
                    return y.languageCode === x;
                });
                if (!language) {
                    valuesUnion.push({
                        isActive: false,
                        languageCode: x,
                        priority: ++startPosition,
                    });
                }
            });

            var newModel = {
                valuesUnion: valuesUnion
            };

            $scope.sortableOptions = {
                stop: function (e, ui) {
                    for (var i = 0; i < $scope.blade.currentEntity.valuesUnion.length; i++) {
                        $scope.blade.currentEntity.valuesUnion[i].priority = i;
                    }
                },
                axis: 'y',
                cursor: "move"
            };

            blade.origEntity = newModel;
            blade.currentEntity = angular.copy(newModel);
            blade.isLoading = false;
        });
    }

    blade.toolbarCommands = [
        {
            name: "platform.commands.save",
            icon: 'fas fa-save',
            executeMethod: function () {
                $scope.saveChanges();
            },
            canExecuteMethod: function () {
                return isDirty();
            },
            permission: blade.updatePermission
        },
        {
            name: "platform.commands.reset",
            icon: 'fa fa-undo',
            executeMethod: function () {
                angular.copy(blade.origEntity, blade.currentEntity);
            },
            canExecuteMethod: isDirty,
            permission: blade.updatePermission
        },
        {
            name: "catalog.blades.catalog-languages.labels.language-dictionary",
            icon: 'fas fa-wrench',
            executeMethod: function () {
                $scope.openDictionarySettingManagement();
            },
            canExecuteMethod: function () {
                return true;
            }
        }
    ];

    blade.onClose = function (closeCallback) {
        bladeNavigationService.showConfirmationIfNeeded(isDirty(), true, blade, $scope.saveChanges, closeCallback, "catalog.dialogs.language-save.title", "catalog.dialogs.language-save.message");
    };

    function isDirty() {
        return !angular.equals(blade.currentEntity, blade.origEntity) && blade.hasUpdatePermission();
    };

    $scope.cancelChanges = function () {
        blade.currentEntity = blade.origEntity;
        $scope.bladeClose();
    }

    $scope.saveChanges = function () {
        if (!blade.hasUpdatePermission()) return;

        var selectedValues = _.filter(blade.currentEntity.valuesUnion, function (x) {
            return x.isActive;
        });

        var defaultValue = _.first(selectedValues);
        if (defaultValue) {
            defaultValue.isDefault = true;
        } else {
            defaultValue = {
                languageCode: blade.currentEntity.defaultValue,
                isDefault: true
            };
            selectedValues.push(defaultValue);
        }

        blade.data.defaultLanguage = defaultValue;
        blade.data.languages = selectedValues;

        angular.copy(blade.currentEntity, blade.origEntity);
        $scope.bladeClose();
    };

    $scope.openDictionarySettingManagement = function () {
        var newBlade = {
            id: 'settingDetailChild',
            isApiSave: true,
            currentEntityId: 'VirtoCommerce.Core.General.Languages',
            parentRefresh: function (data) {
                // added
                _.each(data, function (x) {
                    var presentLanguage = _.find(blade.currentEntity.valuesUnion, function (y) {
                        return y.languageCode.toLowerCase() === x.toLowerCase();
                    });

                    if (!presentLanguage) {
                        var priority = _.max(blade.currentEntity.valuesUnion, function (v) {
                            return v.priority;
                        }).priority;

                        blade.currentEntity.valuesUnion.push({
                            isActive: false,
                            languageCode: x,
                            priority: ++priority
                        });
                    }
                });

                // deleted
                var unusedLanguages = _.filter(blade.currentEntity.valuesUnion, function (x) {
                    return !x.isActive;
                });

                _.each(unusedLanguages, function (x) {
                    var presentLanguage = _.find(data, function (y) {
                        return y.toLowerCase() === x.languageCode.toLowerCase();
                    });

                    if (!presentLanguage) {
                        var index = _.indexOf(blade.currentEntity.valuesUnion, x);
                        blade.currentEntity.valuesUnion.splice(index, 1)
                    }
                });

                $scope.languages = data;
            },
            controller: 'platformWebApp.settingDictionaryController',
            template: '$(Platform)/Scripts/app/settings/blades/setting-dictionary.tpl.html'
        };
        bladeNavigationService.showBlade(newBlade, blade);
    };

    $scope.$watch('blade.parentBlade.currentEntity', initializeBlade);

    // on load: 
    // $scope.$watch('blade.parentBlade.currentEntity' gets fired
}]);
