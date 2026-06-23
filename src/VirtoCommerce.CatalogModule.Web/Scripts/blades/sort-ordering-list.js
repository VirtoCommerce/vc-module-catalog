angular.module('virtoCommerce.catalogModule')
.controller('virtoCommerce.catalogModule.sortOrderingListController',
    ['$scope', 'platformWebApp.bladeNavigationService', 'virtoCommerce.catalogModule.sortOrderings',
    function ($scope, bladeNavigationService, sortOrderings) {
        var blade = $scope.blade;
        blade.updatePermission = 'store:update';
        blade.headIcon = 'fas fa-sort-amount-down';

        function initializeBlade() {
            blade.isLoading = true;

            sortOrderings.getOrderings({ storeId: blade.storeId }, function (data) {
                blade.currentEntities = angular.copy(data);
                blade.origEntities = angular.copy(data);
                recalculate();
                blade.isLoading = false;
            }, function (error) {
                bladeNavigationService.setError('Error ' + error.status, blade);
            });

            // Sortable fields for the clause editor (loaded once, passed down to detail blades).
            sortOrderings.getFields({ storeId: blade.storeId }, function (fields) {
                blade.sortableFields = fields;
            });
        }

        // Order reflects list position; default = the first visible ordering.
        function recalculate() {
            _.each(blade.currentEntities, function (item, index) {
                item.order = index;
            });
            var firstVisible = _.find(blade.currentEntities, function (x) {
                return x.isVisible;
            });
            _.each(blade.currentEntities, function (x) {
                x.isDefault = (x === firstVisible);
            });
        }

        blade.edit = function (node) {
            openDetail(node, false);
        };

        blade.addNew = function () {
            var newOrdering = {
                code: '',
                name: '',
                localizedNames: {},
                order: blade.currentEntities.length,
                isVisible: true,
                isDefault: false,
                isCustom: true,
                isExpressionEditable: true,
                allowOverride: true,
                clauses: [],
                sortExpression: ''
            };
            openDetail(newOrdering, true);
        };

        function openDetail(node, isNew) {
            bladeNavigationService.showBlade({
                id: 'sortOrderingDetail',
                storeId: blade.storeId,
                store: blade.store,
                ordering: node,
                isNew: isNew,
                sortableFields: blade.sortableFields,
                existingCodes: _.pluck(blade.currentEntities, 'code'),
                title: isNew ? 'catalog.blades.sort-ordering-detail.title-new' : node.name,
                controller: 'virtoCommerce.catalogModule.sortOrderingDetailController',
                template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/sort-ordering-detail.tpl.html',
                onSaved: function (updated) {
                    if (isNew) {
                        blade.currentEntities.push(updated);
                    } else {
                        var idx = blade.currentEntities.indexOf(node);
                        if (idx >= 0) {
                            blade.currentEntities.splice(idx, 1, updated);
                        }
                    }
                    recalculate();
                }
            }, blade);
        }

        blade.deleteEntity = function (node, $event) {
            if ($event) {
                $event.stopPropagation();
            }
            // Only admin-authored (custom) orderings can be deleted; built-ins can be hidden via the Visible toggle.
            if (!node.isCustom) {
                return;
            }
            var idx = blade.currentEntities.indexOf(node);
            if (idx >= 0) {
                blade.currentEntities.splice(idx, 1);
                recalculate();
                closeChildren();
            }
        };

        function isDirty() {
            return !angular.equals(blade.currentEntities, blade.origEntities) && blade.hasUpdatePermission();
        }

        $scope.saveChanges = function () {
            blade.isLoading = true;
            recalculate();
            sortOrderings.saveOrderings({ storeId: blade.storeId }, blade.currentEntities, function () {
                closeChildren();
                initializeBlade();
            }, function (error) {
                blade.isLoading = false;
                bladeNavigationService.setError('Error ' + error.status, blade);
            });
        };

        function closeChildren() {
            angular.forEach((blade.childrenBlades || []).slice(), function (child) {
                bladeNavigationService.closeBlade(child);
            });
        }

        blade.onClose = function (closeCallback) {
            bladeNavigationService.showConfirmationIfNeeded(isDirty(), true, blade, $scope.saveChanges, closeCallback,
                "Save changes", "The sort orderings have been modified. Do you want to save the changes?");
        };

        blade.toolbarCommands = [
            {
                name: 'platform.commands.save', icon: 'fas fa-save',
                executeMethod: $scope.saveChanges,
                canExecuteMethod: isDirty,
                permission: blade.updatePermission
            },
            {
                name: 'platform.commands.add', icon: 'fas fa-plus',
                executeMethod: blade.addNew,
                canExecuteMethod: function () { return blade.hasUpdatePermission(); }
            },
            {
                name: 'platform.commands.reset', icon: 'fa fa-undo',
                executeMethod: function () {
                    blade.currentEntities = angular.copy(blade.origEntities);
                    recalculate();
                    closeChildren();
                },
                canExecuteMethod: isDirty
            },
            {
                name: 'platform.commands.refresh', icon: 'fa fa-refresh',
                executeMethod: function () {
                    closeChildren();
                    initializeBlade();
                },
                canExecuteMethod: function () { return true; }
            }
        ];

        $scope.sortableOptions = {
            axis: 'y',
            cursor: 'move',
            stop: function () { recalculate(); }
        };

        initializeBlade();
    }]);
