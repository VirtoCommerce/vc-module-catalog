angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.bulkActionProgressController', ['$scope', 'platformWebApp.bladeNavigationService', 'virtoCommerce.catalogModule.bulkActions'
        , function ($scope, bladeNavigationService, bulkActions) {
        var blade = $scope.blade;
        blade.isLoading = true;
        $scope.blade.headIcon = 'fa-upload';

        function initializeBlade() {
            blade.isLoading = false;
            bulkActions.runBulkAction(blade.actionDataContext,
                function (data) {
                    blade.notification = data;
                });
        }

        $scope.$on("new-notification-event", function (event, notification) {
            if (blade.notification && notification.id === blade.notification.id) {
                angular.copy(notification, blade.notification);
                if (notification.errorCount > 0) {
                    bladeNavigationService.setError('Action error', blade);
                }

                if (blade.notification.finished) {
                    if (blade.onCompleted) {
                        blade.onCompleted();
                    }
                }
            }
        });

        var commandCancel = {
            name: 'platform.commands.cancel',
            icon: 'fa fa-times',
            canExecuteMethod: function () {
                return blade.notification && !blade.notification.finished;
            },
            executeMethod: function () {
                bulkActions.cancel({ jobId: blade.notification.jobId }, function (data) {
                });
            }
        };

        blade.toolbarCommands = [commandCancel];
        initializeBlade();
    }]);
