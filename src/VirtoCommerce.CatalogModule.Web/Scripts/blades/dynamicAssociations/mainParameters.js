angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.dynamicAssociationParametersController', ['$scope', 'platformWebApp.bladeNavigationService', 'virtoCommerce.storeModule.stores', 'virtoCommerce.catalogModule.items', 'platformWebApp.settings', function ($scope, bladeNavigationService, stores, items, settings ) {
        var blade = $scope.blade;
        blade.isLoading = true;
        blade.headIcon = 'fa-area-chart';
        var formScope;
        $scope.setForm = (form) => { formScope = form; };

        blade.isValid = function() {
            return formScope && formScope.$valid;
        };

        blade.refresh = (parentRefresh) => {
            $scope.associationGroups = settings.getValues({ id: 'Catalog.AssociationGroups' }, data => {
                if (data && data.length > 0) {
                    blade.associationType = data[0];
                }
            });

            stores.query({}, response => {
                $scope.stores = response;
                if (parentRefresh) {
                    blade.parentBlade.refresh();
                }
                blade.isLoading = false;
            });
        };

        // datepicker 
        $scope.datepickers = {
            str: false,
            end: false
        };

        $scope.open = function ($event, which) {
            $event.preventDefault();
            $event.stopPropagation();
            $scope.datepickers[which] = true;
        };

        $scope.openDictionarySettingManagement = function () {
            var newBlade = {
                id: 'settingDetailChild',
                isApiSave: true,
                currentEntityId: 'Catalog.AssociationGroups',
                parentRefresh: function (data) { $scope.associationGroups = data; },
                controller: 'platformWebApp.settingDictionaryController',
                template: '$(Platform)/Scripts/app/settings/blades/setting-dictionary.tpl.html'
            };
            bladeNavigationService.showBlade(newBlade, blade);
        };

        blade.refresh(false);
    }]);
