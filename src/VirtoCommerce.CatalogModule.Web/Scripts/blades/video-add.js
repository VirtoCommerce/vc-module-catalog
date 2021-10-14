angular.module('virtoCommerce.catalogModule')
    .controller('virtoCommerce.catalogModule.videoAddController',
        ['$scope', '$resource', 'platformWebApp.bladeNavigationService', 'virtoCommerce.catalogModule.videos',
            function ($scope, $resource, bladeNavigationService, videos) {
                var blade = $scope.blade;
                var parentBlade = blade.parentBlade;

                blade.headIcon = 'fab fa-youtube';
                blade.title = 'catalog.blades.video-add.title';
                blade.subtitle = 'catalog.blades.video-add.subtitle';

                blade.refresh = function () {
                    blade.isLoading = false;
                };

                var currentForm;
                $scope.setForm = function (form) {
                    currentForm = form;
                };

                function canCreate() {
                    return currentForm && currentForm.$valid;
                }

                function createVideo() {
                    blade.isLoading = true;
                    videos.create({
                            contentUrl: blade.currentEntity.contentUrl,
                            languageCode: blade.defaultLanguage.languageCode,
                            ownerId: blade.owner.id,
                            ownerType: blade.ownerType
                        },
                        function (data) {
                            blade.isLoading = false;
                            $scope.bladeClose(function () {
                                var newBlade = {
                                    id: 'videoDetail',
                                    isNew: true,
                                    currentEntity: data,
                                    controller: 'virtoCommerce.catalogModule.videoDetailController',
                                    template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/video-detail.tpl.html'
                                };
                                bladeNavigationService.showBlade(newBlade, parentBlade);
                            });
                        },
                        function (error) {
                            blade.isLoading = false;
                            if (!error.statusText && error.data)
                                error.statusText = error.data.message;
                            bladeNavigationService.setError(error, blade);
                        });
                }

                blade.toolbarCommands = [
                    {
                        name: 'catalog.blades.video-add.create',
                        icon: 'fas fa-check',
                        executeMethod: function () {
                            createVideo();
                        },
                        canExecuteMethod: function () {
                            return canCreate();
                        }
                    }
                ];

                blade.refresh();
    }]);
