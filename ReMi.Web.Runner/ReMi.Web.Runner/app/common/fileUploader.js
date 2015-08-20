(function () {
    'use strict';

    var fileUploaderModule = angular.module('fileUploaderModule', []);

    fileUploaderModule.directive('fileUploader', [
        function fileUploader() {
            return {
                restrict: 'A',
                require: '?ngModel',
                scope: {
                    uploadUrl: '@'
                },
                link: function (scope, elem, attrs, ngModel) {
                    if (!ngModel) {
                        console.log('Couldn\'t initialized fileUploader. Can\'t find ngModel attribute.');
                        return;
                    }

                    var $elem,
                        $status = elem.next('.progress'),
                        $progressBar = $status.find('.bar'),
                        config = {
                            //forceIframeTransport: true,
                            type: 'POST',
                            dataType: 'json',
                            url: scope.uploadUrl,

                            add: function (e, data) {

                                scope.$apply(function () {
                                    var fileArr = ngModel.$modelValue || [];
                                    data.files.forEach(function (file) {
                                        fileArr.push(file);
                                    });

                                    ngModel.$setViewValue(fileArr);
                                });
                            },

                            start: function (e) {
                                $elem = $(e.target);
                                $elem.hide();
                                $status.removeClass('hide');
                                $progressBar.text('Uploading...');
                            },

                            done: function (e, data) {

                                scope.$apply(function () {
                                    var fileArr = ngModel.$modelValue || [];

                                    if (fileArr.length == data.result.length)
                                        for (var i = 0; i < data.result.length; i++) {
                                            fileArr[i].serverName = data.result[i].name;
                                        }

                                    ngModel.$setViewValue(fileArr);
                                });

                                $status.removeClass('progress-striped progress-bar-warning active').addClass('progress-bar-success');
                                $progressBar.text('Done');
                            },

                            progress: function (e, data) {
                                var progress = parseInt(data.loaded / data.total * 100, 10);

                                $progressBar.css('width', progress + '%');
                                if (progress === 100) {
                                    $status.addClass('progress-bar-warning');
                                    $progressBar.text('Processing...');
                                }
                            },

                            error: function (resp, er, msg) {
                                console.log('Error');
                                console.log(msg);

                                $elem.show();
                                $status.removeClass('active progress-bar-warning progress-striped').addClass('progress-bar-danger');
                                $progressBar.css('width', '100%');

                                if (resp.status === 415) {
                                    $progressBar.text(msg);
                                } else {
                                    $progressBar.text('There was an error. Please try again.');
                                }
                            }
                        };

                    elem.fileupload(config);
                }
            };
        }
    ]);

})();
