angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.productConfigurationDetailController', ['$scope', 'platformWebApp.bladeNavigationService', 'virtoCommerce.catalogModule.configurationsApi', 'platformWebApp.uiGridHelper',
        function ($scope, bladeNavigationService, configurationsApi, uiGridHelper) {
            var blade = $scope.blade;

            blade.headIcon = 'fas fa-sliders';
            blade.title = 'catalog.blades.configuration-details.title';
            blade.formScope = null;
            blade.toolbarCommands = [
                {
                    name: "platform.commands.save",
                    icon: 'fas fa-save',
                    executeMethod: saveChanges,
                    canExecuteMethod: canSave,
                    permission: 'configurations:update'
                },
                {
                    name: "platform.commands.reset",
                    icon: 'fa fa-undo',
                    executeMethod: function () { angular.copy(blade.origEntity, blade.currentEntity); },
                    canExecuteMethod: isDirty,
                    permission: 'configurations:update'
                },
                {
                    name: "catalog.blades.configuration-details.commands.add",
                    icon: 'fas fa-plus',
                    executeMethod: function() { openSectionBlade({}); },
                    canExecuteMethod: function () { return true; },
                    permission: 'configurations:update'
                },
                {
                    name: "platform.commands.delete",
                    icon: 'fas fa-trash-alt',
                    executeMethod: function () { deleteList($scope.gridApi.selection.getSelectedRows()); },
                    canExecuteMethod: isItemsChecked,
                    permission: 'configurations:delete'
                }
            ];

            blade.onClose = function (closeCallback) {
                bladeNavigationService.showConfirmationIfNeeded(isDirty(), canSave(), blade, saveChanges, closeCallback, "catalog.dialogs.configuration-save.title", "catalog.dialogs.configuration-save.message");
            };

            $scope.setForm = function (form) { blade.formScope = form; }

            $scope.setGridOptions = function (gridOptions) {
                uiGridHelper.initialize($scope, gridOptions, function (gridApi) {
                    $scope.gridApi = gridApi;

                    gridApi.draggableRows.on.rowFinishDrag($scope, function () {
                        for (var i = 0; i < blade.currentEntity.sections.length; i++) {
                            blade.currentEntity.sections[i].displayOrder = i;
                        }
                    });
                });
            };

            $scope.edit = function(item)
            {
                $scope.selectedNodeId = item.displayOrder;
                openSectionBlade(item);
            }

            $scope.delete = function (data) {
                deleteList([data]);
            };

            $scope.$watch("blade.currentEntity", function () {
                if (blade.currentEntity) {
                    var canBeEnabled = _.some(blade.currentEntity.sections) &&
                        _.every(blade.currentEntity.sections, section => _.some(section.options));

                    if (!canBeEnabled) {
                        blade.currentEntity.isActive = false;
                    }

                    $scope.canBeEnabled = canBeEnabled;
                }
            }, true);

            function deleteList(list) {
                bladeNavigationService.closeChildrenBlades(blade,
                    function () {
                        var undeletedEntries = _.difference(blade.currentEntity.sections, list);
                        blade.currentEntity.sections = undeletedEntries;
                    });
            };

            function isItemsChecked() {
                return $scope.gridApi && _.any($scope.gridApi.selection.getSelectedRows());
            }

            function isDirty() {
                return !angular.equals(blade.currentEntity, blade.origEntity);
            }

            function canSave() {
                return isDirty() && blade.formScope && blade.formScope.$valid && blade.currentEntity.sections.length > 0;
            }

            function saveChanges() {
                blade.isLoading = true;

                configurationsApi.update({}, blade.currentEntity, function () {
                    refresh();
                });
            }

            function openSectionBlade(section) {
                var newBlade = {
                    id: "sectionDetail",
                    origEntity: section ,
                    title: section.name ? section.name : 'catalog.blades.section-details.title',
                    onSaveNew: function (section) {
                        section.displayOrder = blade.currentEntity.sections.length;
                        blade.currentEntity.sections.push(section);
                    },
                    controller: 'virtoCommerce.catalogModule.configurationSectionDetailController',
                    template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/configurations/configuration-section-detail.tpl.html'
                };

                bladeNavigationService.showBlade(newBlade, blade);
            }

            function refresh() {
                blade.isLoading = true;
                configurationsApi.getConfigurationByProduct({ productId: blade.productId }, function (data) {
                    blade.isLoading = false;
                    if(data.sections == null)
                    {
                        data.sections = [];
                    }
                    blade.currentEntity = angular.copy(data);
                    blade.origEntity = data;
                });
            }

            refresh();
        }]);
