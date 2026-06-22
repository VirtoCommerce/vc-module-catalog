angular.module('virtoCommerce.catalogModule')
.controller('virtoCommerce.catalogModule.sortOrderingDetailController',
    ['$scope', 'platformWebApp.bladeNavigationService',
    function ($scope, bladeNavigationService) {
        var blade = $scope.blade;
        blade.isLoading = false;
        blade.headIcon = 'fas fa-sort-amount-down';
        blade.updatePermission = 'store:update';

        blade.entity = angular.copy(blade.ordering);
        blade.entity.localizedNames = blade.entity.localizedNames || {};
        blade.entity.clauses = blade.entity.clauses || [];

        var original = angular.copy(blade.entity);

        blade.languages = (blade.store && blade.store.languages) || [];

        // Gate flags coming from the backend (resolver-provided). New custom orderings are fully editable.
        blade.canEditDisplay = blade.entity.allowOverride !== false;
        blade.canEditExpression = blade.entity.isExpressionEditable !== false;
        blade.canEditCode = blade.isNew === true;

        // Auto-propose a code from the name for new orderings until the user edits the code manually.
        var codeTouched = !blade.isNew;

        buildFieldOptions();
        updateExpression();

        function buildFieldOptions() {
            var names = _.map(blade.sortableFields || [], function (f) { return f.name; });
            // Preserve any clause field that is not in the discovered list (e.g. a built-in's "id" tie-breaker).
            _.each(blade.entity.clauses, function (c) {
                if (c.field && names.indexOf(c.field) < 0) {
                    names.push(c.field);
                }
            });
            blade.fieldOptions = names;
        }

        function slugify(text) {
            return (text || '').toLowerCase().trim()
                .replace(/[^a-z0-9]+/g, '-')
                .replace(/^-+|-+$/g, '')
                .replace(/-{2,}/g, '-');
        }

        blade.onNameChanged = function () {
            if (blade.isNew && !codeTouched) {
                blade.entity.code = slugify(blade.entity.name);
            }
        };

        blade.onCodeChanged = function () {
            codeTouched = true;
        };

        blade.addClause = function () {
            blade.entity.clauses.push({ field: '', isDescending: false });
            buildFieldOptions();
            updateExpression();
        };

        blade.removeClause = function (index) {
            blade.entity.clauses.splice(index, 1);
            updateExpression();
        };

        blade.setDirection = function (clause, descending) {
            clause.isDescending = descending;
            updateExpression();
        };

        blade.onClauseFieldChanged = function () {
            updateExpression();
        };

        function updateExpression() {
            var parts = (blade.entity.clauses || [])
                .filter(function (c) { return c.field; })
                .map(function (c) { return c.field + ':' + (c.isDescending ? 'desc' : 'asc'); });
            blade.entity.sortExpression = parts.join(';');
        }

        function isDirty() {
            return !angular.equals(blade.entity, original);
        }

        function isValid() {
            if (!blade.entity.code || !blade.entity.name) {
                return false;
            }
            var codeLower = blade.entity.code.toLowerCase();
            var duplicate = _.some(blade.existingCodes || [], function (c) {
                return c && c.toLowerCase() === codeLower && c.toLowerCase() !== (blade.ordering.code || '').toLowerCase();
            });
            if (duplicate) {
                return false;
            }
            if (blade.entity.isCustom && !_.some(blade.entity.clauses, function (c) { return c.field; })) {
                return false;
            }
            return true;
        }

        $scope.saveChanges = function () {
            updateExpression();
            if (blade.onSaved) {
                blade.onSaved(angular.copy(blade.entity));
            }
            // Mark clean before closing so onClose doesn't re-prompt and re-run save (which created duplicate items).
            original = angular.copy(blade.entity);
            bladeNavigationService.closeBlade(blade);
        };

        blade.onClose = function (closeCallback) {
            bladeNavigationService.showConfirmationIfNeeded(isDirty() && isValid(), true, blade, $scope.saveChanges, closeCallback,
                "Save changes", "The ordering has been modified. Apply the changes?");
        };

        blade.toolbarCommands = [
            {
                name: 'platform.commands.ok', icon: 'fas fa-check',
                executeMethod: $scope.saveChanges,
                canExecuteMethod: function () { return isValid() && isDirty(); }
            },
            {
                name: 'platform.commands.reset', icon: 'fa fa-undo',
                executeMethod: function () { blade.entity = angular.copy(original); buildFieldOptions(); updateExpression(); },
                canExecuteMethod: isDirty
            }
        ];

        $scope.sortableOptions = {
            axis: 'y',
            cursor: 'move',
            stop: function () { updateExpression(); }
        };
    }]);
