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

                function getEmbedUrl(input)
                {
                    if (/<iframe/gi.test(input)) {
                        var matches = /src="(.*?)"/gm.exec(input);
                        if (matches && matches.length >= 2) {
                            return matches[1];
                        }
                    }
                    return undefined;
                }

                function getVideoId(url)
                {
                    var arr = url.split(/(vi\/|v%3D|v=|\/v\/|youtu\.be\/|\/embed\/)/);
                    return undefined !== arr[2] ? arr[2].split(/[^0-9a-z_\-]/i)[0] : arr[0];
                }

                function formatDuration(input)
                {
                    var re = /PT(?:(\d+)H)?(?:(\d+)M)?(?:(\d+)S)?$/;
                    if (re.test(input)) {
                        var [, hours, minutes, seconds] = input.match(re);
                        return [hours || '00', minutes || '00', seconds || '00']
                            .map(num => num.length < 2 ? `0${num}` : num)
                            .join(':');
                    }
                    return input;
                }

                function videoDetail(item)
                {
                    blade.isLoading = true;
                    // get last priority
                    videos.search({
                            ownerIds: [item.ownerId],
                            ownerType: item.ownerType,
                            sort: 'SortOrder:desc',
                            skip: 0,
                            take: 1
                        },
                        function (data) {
                            blade.isLoading = false;
                            var sortOrder = 1;
                            if (data.totalCount > 0)
                            {
                                sortOrder = data.results[0].sortOrder + 1;
                            }
                            angular.extend(item, {
                                SortOrder: sortOrder
                            });
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
                        },
                        function (error) {
                            bladeNavigationService.setError('Error ' + error.status, blade);
                        });
                }

                function createVideo() {
                    blade.isLoading = true;
                    if (blade.videoOptions.googleApiKey) {
                        var videoId = getVideoId(blade.currentEntity.contentUrl);
                        videos.youtube_api({
                                part: 'snippet,contentDetails,player',
                                id: videoId,
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
                                    embedUrl: getEmbedUrl(video.player.embedHtml),
                                    duration: formatDuration(video.contentDetails.duration),
                                    languageCode: blade.defaultLanguage.languageCode
                                });
                                blade.isLoading = false;
                                videoDetail(blade.currentEntity);
                            },
                            function (error) {
                                blade.isLoading = false;
                                bladeNavigationService.setError('Error ' + error.status, blade);
                            });

                    }
                    else {
                        videos.youtube_embed({
                                url: blade.currentEntity.contentUrl,
                                format: 'json'
                            },
                            function (data) {
                                angular.extend(blade.currentEntity, {
                                    ownerId: blade.owner.id,
                                    ownerType: blade.ownerType,
                                    name: data.title,
                                    description: data.title,
                                    thumbnailUrl: data.thumbnail_url,
                                    embedUrl: getEmbedUrl(data.html),
                                    duration: '00:00:00',
                                    languageCode: blade.defaultLanguage.languageCode
                                });
                                blade.isLoading = false;
                                videoDetail(blade.currentEntity);
                            },
                            function (error) {
                                blade.isLoading = false;
                                bladeNavigationService.setError('Error ' + error.status, blade);
                            });
                    }
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
