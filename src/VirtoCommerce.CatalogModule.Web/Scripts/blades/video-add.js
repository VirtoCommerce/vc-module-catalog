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
                    if (blade.videoOptions.googleApiKey) {
                        var videoId = getVideoId(blade.currentEntity.contentUrl);
                        videos.youtube_api({
                                part: 'snippet,contentDetails,player',
                                id: videoId.id,
                                key: blade.videoOptions.googleApiKey
                            },
                            function (data) {
                                var video = data.items[0];
                                var snippet = video.snippet;
                                angular.extend(blade.currentEntity, {
                                    ownerId: blade.owner.id,
                                    ownerType: blade.ownerType,
                                    name: snippet.title,
                                    description: snippet.description,
                                    uploadDate: snippet.publishedAt,
                                    thumbnailUrl: snippet.thumbnails.high.url,
                                    duration: video.contentDetails.duration,
                                    languageCode: blade.defaultLanguage.languageCode
                                });
                                try {
                                    angular.extend(blade.currentEntity, {
                                        embedUrl: getEmbedUrl(video.player.embedHtml)
                                    });
                                }
                                catch (e) {
                                    console.error('Error getting embedUrl.', e);
                                }
                                blade.isLoading = false;
                                addVideo(blade.currentEntity);
                            },
                            function (error) {
                                blade.isLoading = false;
                                bladeNavigationService.setError('Error ' + error.status, $scope.blade);
                            });

                    }
                    else {
                        videos.youtube_embed({ url: blade.currentEntity.contentUrl, format: 'json' },
                            function (data) {
                                angular.extend(blade.currentEntity, {
                                    ownerId: blade.owner.id,
                                    ownerType: blade.ownerType,
                                    name: data.title,
                                    description: data.title,
                                    uploadDate: new Date(),
                                    thumbnailUrl: data.thumbnail_url,
                                    duration: '00:00:00',
                                    languageCode: blade.defaultLanguage.languageCode
                                });
                                try {
                                    angular.extend(blade.currentEntity, {
                                        embedUrl: getEmbedUrl(data.html)
                                    });
                                }
                                catch (e) {
                                    console.error('Error getting embedUrl.', e);
                                }
                                blade.isLoading = false;
                                addVideo(blade.currentEntity);
                            },
                            function (error) {
                                blade.isLoading = false;
                                bladeNavigationService.setError('Error ' + error.status, $scope.blade);
                            });
                    }
                }

                function getEmbedUrl(html)
                {
                    var frame = angular.element(html)[0];
                    return frame.src;
                }

                function addVideo(item)
                {
                    $scope.bladeClose(function () {
                        var newBlade = {
                            id: 'videoDetail',
                            isNew: true,
                            currentEntity: item,
                            controller: 'virtoCommerce.catalogModule.videoDetailController',
                            template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/video-detail.tpl.html'
                        };
                        bladeNavigationService.showBlade(newBlade, parentBlade);
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
