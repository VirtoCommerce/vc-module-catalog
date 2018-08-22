angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.editorialReviewDetailController', ['$scope', 'platformWebApp.bladeNavigationService', 'FileUploader', 'platformWebApp.settings', '$timeout',
        function ($scope, bladeNavigationService, FileUploader, settings, $timeout) {
            var blade = $scope.blade;

            function initilize() {
                if (!blade.item.reviews) {
                    blade.item.reviews = [];
                };
                if (!blade.currentEntity) {
                    blade.currentEntity = {};
                };

                blade.origEntity = blade.currentEntity;
                blade.currentEntity = angular.copy(blade.currentEntity);

                $scope.validatedLangs = blade.languages;

                $timeout(function () {
                    $scope.$broadcast('resetContent', { body: blade.currentEntity.content });
                    blade.isLoading = false;
                });
            }

            function calcCombinationsDiff(allCombinations, existingCombinations) {
                var resultDiff = [];
                _.any(allCombinations, function (combination) {
                    var existingItem = _.find(existingCombinations, function (item) {
                        return item.reviewType === combination[0] && item.languageCode === combination[1];
                    });
                    if (!existingItem) {
                        resultDiff.push(combination);
                    }
                });

                return resultDiff;
            }

            blade.toolbarCommands = [
                {
                    name: "platform.commands.reset", icon: 'fa fa-undo',
                    executeMethod: function () {
                        angular.copy(blade.origEntity, blade.currentEntity);
                        $scope.$broadcast('resetContent', { body: blade.currentEntity.content });
                    },
                    canExecuteMethod: isDirty
                }
            ];

            $scope.typeSelected = function (item) {
                var selectedType = item;
                blade.currentEntity.reviewType = selectedType;
                
                $scope.typeLangCombinationsDiff = calcCombinationsDiff($scope.typeLangCombinations, blade.item.reviews);
                var filteredLangs = $scope.typeLangCombinationsDiff.filter(function (x) {
                    return x[0] === selectedType;
                });

                var validatedLangs = filteredLangs.length > 0 ? filteredLangs.map(x => x[1]).filter(onlyUnique) : [];
                $scope.validatedLangsFull = getLanguagesFullInfo(validatedLangs);
                if (!blade.currentEntity.languageCode)
                    blade.currentEntity.languageCode = $scope.validatedLangsFull[0].languageCode; //resetting lang to first available one if not set yet
                if (!_.any($scope.validatedLangsFull, x => x.languageCode === blade.currentEntity.languageCode)) { // in case our selected value is not in the list
                    $scope.validatedLangsFull.push(blade.languages.find(x => x.languageCode === blade.currentEntity.languageCode))
                }
            }

            $scope.langSelected = function (item) {
                var selectedLang = item;
                blade.currentEntity.languageCode = selectedLang;
                $scope.typeLangCombinationsDiff = calcCombinationsDiff($scope.typeLangCombinations, blade.item.reviews);
                var filteredTypes = $scope.typeLangCombinationsDiff.filter(function (x) {
                    return x[1] === selectedLang;
                });
                
                var validatedTypes = filteredTypes.length > 0 ? filteredTypes.map(x => x[0]).filter(onlyUnique) : [];
                $scope.validatedTypes = validatedTypes;
                blade.currentEntity.reviewType = $scope.validatedTypes[0]; //resetting type to first available one
            }

            function cartesian() {
                var r = [], arg = arguments, max = arg.length - 1;
                function helper(arr, i) {
                    for (var j = 0, l = arg[i].length; j < l; j++) {
                        var a = arr.slice(0); // clone arr
                        a.push(arg[i][j]);
                        if (i === max)
                            r.push(a);
                        else
                            helper(a, i + 1);
                    }
                }
                helper([], 0);
                return r;
            }

            $scope.saveChanges = function () {
                
                var isValid = $scope.isValid();
                if (isValid) {
                    if (_.any(blade.item.reviews, x => x.reviewType === blade.currentEntity.reviewType && x.languageCode === blade.currentEntity.languageCode))
                        //if an item already exists in our list - modify it
                        _.extend(_.findWhere(blade.item.reviews, { reviewType: blade.currentEntity.reviewType, languageCode: blade.currentEntity.languageCode }), blade.currentEntity);
                    else {
                        blade.item.reviews.push(blade.currentEntity);
                    }
                    angular.copy(blade.currentEntity, blade.origEntity);
                    $scope.bladeClose();
                }
            }

            $scope.isValid = function () {
                var result = formScope && formScope.$valid;
                return result;
            }


            blade.headIcon = 'fa-comments';
            blade.title = 'catalog.blades.editorialReview-detail.title';
            blade.subtitle = 'catalog.blades.editorialReview-detail.subtitle';
            blade.editAsMarkdown = true;
            blade.hasAssetCreatePermission = bladeNavigationService.checkPermission('platform:asset:create');

            if (blade.hasAssetCreatePermission) {
                $scope.fileUploader = new FileUploader({
                    url: 'api/platform/assets?folderUrl=catalog/' + blade.item.code,
                    headers: { Accept: 'application/json' },
                    autoUpload: true,
                    removeAfterUpload: true,
                    onBeforeUploadItem: function (fileItem) {
                        blade.isLoading = true;
                    },
                    onSuccessItem: function (fileItem, response) {
                        $scope.$broadcast('filesUploaded', { items: response });
                    },
                    onErrorItem: function (fileItem, response, status) {
                        bladeNavigationService.setError(fileItem._file.name + ' failed: ' + (response.message ? response.message : status), blade);
                    },
                    onCompleteAll: function () {
                        blade.isLoading = false;
                    }
                });
            }

            function onlyUnique(value, index, self) {
                return self.indexOf(value) === index;
            }

            settings.getValues({ id: 'Catalog.EditorialReviewTypes' }, function (data) {
                $scope.types = data;
                $scope.typeLangCombinations = cartesian($scope.types, blade.languages.map(x => x.languageCode));
                $scope.typeLangCombinationsDiff = calcCombinationsDiff($scope.typeLangCombinations, blade.item.reviews);

                if (blade.currentEntity.id) { // edit mode case of existing in db records
                    blade.currentEntity.reviewType = blade.currentEntity.reviewType;
                    blade.currentEntity.languageCode = blade.currentEntity.languageCode;
                    
                    var filteredLangs = $scope.typeLangCombinationsDiff.filter(function (x) {
                        return x[0] === blade.currentEntity.reviewType;
                    });
                    $scope.validatedTypes = $scope.typeLangCombinationsDiff.map(x => x[0]).filter(onlyUnique);
                    var validatedLangsForEdit = filteredLangs.length > 0 ? filteredLangs.map(x => x[1]).filter(onlyUnique) : [];
                    $scope.validatedLangsFull = getLanguagesFullInfo(validatedLangsForEdit);
                } else { // editing just newly added records (not in db) or adding new ones                    
                    if (!blade.currentEntity.reviewType || !blade.currentEntity.languageCode) { // if editing just newly added records - skip it, select first combination only when adding new one
                        var selectedCombination = $scope.typeLangCombinationsDiff[0];// selecting first combination by default that we have
                        blade.currentEntity.reviewType = selectedCombination[0];
                        blade.currentEntity.languageCode = selectedCombination[1]; // in add new mode - just use default one
                    }
                    $scope.validatedTypes = $scope.typeLangCombinationsDiff.map(x => x[0]).filter(onlyUnique);
                    var validatedLangs = $scope.typeLangCombinationsDiff.map(x => x[1]).filter(onlyUnique);
                    $scope.validatedLangsFull = getLanguagesFullInfo(validatedLangs);
                }
                if (!_.any($scope.validatedLangsFull, x => x.languageCode === blade.currentEntity.languageCode)) { // in case our selected value is not in the list
                    $scope.validatedLangsFull.push(blade.languages.find(x => x.languageCode === blade.currentEntity.languageCode))
                }
            });

            $scope.openDictionarySettingManagement = function () {
                var newBlade = new DictionarySettingDetailBlade('Catalog.EditorialReviewTypes');
                newBlade.parentRefresh = function (data) {
                    $scope.types = data;
                };
                bladeNavigationService.showBlade(newBlade, blade);
            };

            function getLanguagesFullInfo(langs) {
                return blade.languages.filter(x =>
                    _.any(langs, lang => lang === x.languageCode)
                ).filter(onlyUnique);
            };

            var formScope;
            $scope.setForm = function (form) { formScope = form; }

            function isDirty() {
                return !angular.equals(blade.currentEntity, blade.origEntity);
            };

            initilize();
        }]);
