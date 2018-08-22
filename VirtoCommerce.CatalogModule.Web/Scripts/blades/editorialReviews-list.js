angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.editorialReviewsListController', ['$timeout', '$scope', 'platformWebApp.bladeNavigationService', 'platformWebApp.uiGridHelper', 'platformWebApp.dialogService', 'platformWebApp.settings', function ($timeout, $scope, bladeNavigationService, uiGridHelper, dialogService, settings) {
        var blade = $scope.blade;

        $scope.selectedNodeId = null; // need to initialize to null
        blade.isLoading = false;
        blade.refresh = function (item) {
            initialize(item);
        };

        function initialize(item) {
            blade.headIcon = 'fa-comments';
            blade.item = item;
            blade.title = blade.item.name;
            blade.subtitle = 'catalog.blades.editorialReviews-list.subtitle';
            blade.selectNode = $scope.openBlade;
        };

        $scope.openBlade = function (node) {
            if (node) {
                $scope.selectedNodeId = node.id;
            }
            var newBlade = {
                id: 'editorialReview',
                currentEntity: node,
                item: blade.item,
                catalog: blade.catalog,
                languages: blade.catalog.languages,
                controller: 'virtoCommerce.catalogModule.editorialReviewDetailController',
                template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/editorialReview-detail.tpl.html'
            };
            bladeNavigationService.showBlade(newBlade, $scope.blade);
        }

        $scope.delete = function (data) {
            deleteList([data]);
        };

        function deleteList(selection) {
            var dialog = {
                id: "confirmDelete",
                title: "catalog.dialogs.review-delete.title",
                message: "catalog.dialogs.review-delete.message",
                callback: function (remove) {
                    if (remove) {
                        bladeNavigationService.closeChildrenBlades(blade, function () {
                            _.each(selection, function (x) {
                                blade.item.reviews.splice(blade.item.reviews.indexOf(x), 1);
                            });
                        });
                    }
                }
            };
            dialogService.showConfirmationDialog(dialog);
        }

        settings.getValues({ id: 'Catalog.EditorialReviewTypes' }, function (data) {
            $scope.types = data;
        });

        blade.toolbarCommands = [
            {
                name: "platform.commands.add", icon: 'fa fa-plus',
                executeMethod: function () {
                    $scope.openBlade({});
                },
                canExecuteMethod: function () {
                    var langs = blade.catalog.languages.map(x => x.languageCode);
                    var types = $scope.types;
                    if (!langs || !types)
                        return true; //it means it's not yet loaded for the blade

                    var langTypeCombinations = cartesian(langs, types);
                    var reviewContainsCombinationCounter = 0;
                    _.any(langTypeCombinations, function (combination) {
                        var reviewContains = _.any(blade.item.reviews, function (review) {
                            return review.languageCode === combination[0] && review.reviewType === combination[1];
                        });
                        if (reviewContains) {
                            reviewContainsCombinationCounter++;
                        }
                    });

                    return reviewContainsCombinationCounter < langTypeCombinations.length;
                }
            },
            {
                name: "platform.commands.delete", icon: 'fa fa-trash-o',
                executeMethod: function () { deleteList($scope.gridApi.selection.getSelectedRows()); },
                canExecuteMethod: function () {
                    return $scope.gridApi && _.any($scope.gridApi.selection.getSelectedRows());
                }
            }

        ];

        function cartesian() {
            var r = [], arg = arguments, max = arg.length - 1;
            function helper(arr, i) {
                for (var j = 0, l = arg[i].length; j < l; j++) {
                    var a = arr.slice(0); // clone arr
                    a.push(arg[i][j]);
                    if (i == max)
                        r.push(a);
                    else
                        helper(a, i + 1);
                }
            }
            helper([], 0);
            return r;
        }

        // ui-grid
        $scope.setGridOptions = function (gridOptions) {
            uiGridHelper.initialize($scope, gridOptions);
        };

        // open blade for new review 
        if (!_.some(blade.item.reviews)) {
            $timeout($scope.openBlade, 60, false);
        }

        initialize(blade.item);
    }]);
