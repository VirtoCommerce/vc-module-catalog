angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.outputTuningController', ['$scope', 'platformWebApp.bladeNavigationService', 'virtoCommerce.catalogModule.categories' , function ($scope, bladeNavigationService, categories) {
        var blade = $scope.blade;
        blade.isLoading = true;
        blade.headIcon = 'fa-area-chart';
        blade.properties = [];
        blade.outputTuningBlock = {};

        $scope.BlockResultingRules = 'BlockResultingRules';
        $scope.ConditionPropertyValues = 'ConditionPropertyValues';
        $scope.BlockOutputTuning = 'BlockOutputTuning';

        blade.sortFields = ['',''];
        blade.sortDirections = ['asc', 'asc'];

        var formScope;
        $scope.setForm = (form) => { formScope = form; };

        $scope.isValid = function () {
            return formScope && formScope.$valid;
        };

        blade.currentEntity = {};

        function initializeBlade() {
            blade.currentEntity = angular.copy(blade.originalEntity);
            categories.getByIds({ ids: blade.categoryIds },
                data => {
                    blade.properties = _.unique(_.first(data.map(x => x.properties))).map(x=>x.name);
                });
           
            blade.outputTuningBlock = _.find(blade.currentEntity.expressionTree.children, x => x.id === $scope.BlockOutputTuning);

            if (blade.outputTuningBlock.sort) {
                let sortPairs = blade.outputTuningBlock.sort.split(';');
                _.each(sortPairs,
                    (item, index) => {
                        let pair = item.split(':');
                        blade.sortFields[index] = _.first(pair);
                        blade.sortDirections[index] = _.last(pair);
                    });
            }
            blade.isLoading = false;
        }

        $scope.onStoreSelected = ($item) => blade.currentEntity.catalogId = $item.catalog;

        $scope.cancelChanges = function () {
            bladeNavigationService.closeBlade(blade);
        };

        $scope.saveChanges = function () {
            if (blade.onSelected) {
                blade.outputTuningBlock.sort = '';
                for (let i = 0; i < blade.sortFields.length; i++) {
                    if (blade.sortFields[i].length > 0) {
                        blade.outputTuningBlock.sort += blade.sortFields[i] + ':' + blade.sortDirections[i] + ';';
                    }
                }

                blade.onSelected(blade.outputTuningBlock);
                bladeNavigationService.closeBlade(blade);
            }

            bladeNavigationService.closeBlade(blade);
        };

        initializeBlade();
    }]);
