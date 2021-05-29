(function () {
    "use strict"

    function configurationFilesResource(umbRequestHelper, $http) {
        return {

            saveFile: function (configFile) {
                return umbRequestHelper.resourcePromise(
                    $http.post(
                        umbRequestHelper.getApiUrl("configurationFilesEditorsBaseUrl", "SaveFile"),
                        configFile),
                    "Failed to save file")
            },

            getFile: function (virtualpath) {
                console.log(umbRequestHelper.getApiUrl(
                    "configurationFilesEditorsBaseUrl", "GetFile"))

                return umbRequestHelper.resourcePromise(
                    $http.get(
                        umbRequestHelper.getApiUrl("configurationFilesEditorsBaseUrl", "GetFile", { path: virtualpath })
                    ),
                    "Failed to retrieve file from path " + virtualpath)
            }
        };
    }

    angular.module("umbraco").factory("configurationFilesResource", configurationFilesResource)


})()