(function () {
    "use strict"

    function configurationEditorResource(umbRequestHelper, $http) {
        return {

            saveFile: function (configFile) {
                return umbRequestHelper.resourcePromise(
                    $http.post(
                        umbRequestHelper.getApiUrl("configurationEditorEditorsBaseUrl", "PostSave"),
                        configFile),
                    "Failed to save file")
            },

            getFile: function (virtualpath) {
                console.log(umbRequestHelper.getApiUrl(
                    "configurationEditorEditorsBaseUrl", "GetFile"))

                return umbRequestHelper.resourcePromise(
                    $http.get(
                        umbRequestHelper.getApiUrl("configurationEditorEditorsBaseUrl", "GetByPath", { virtualpath: virtualpath })
                    ),
                    "Failed to retrieve file from path " + virtualpath)
            }
        };
    }

    angular.module("umbraco").factory("configurationEditorResource", configurationEditorResource)


})()