(function () {
    "use strict"

    function configurationEditorEditController($scope, $routeParams, configurationEditorResource, assetsService, notificationsService, overlayService, editorState, navigationService, appState, angularHelper, $timeout, contentEditingHelper, localizationService) {

        var vm = this

        const treeName = "configurationEditor"

        vm.header = {}
        vm.header.editorfor = "settings_configuration";
        vm.header.setPageTitle = true;

        vm.page = {}
        vm.page.loading = true

        //menu
        vm.page.menu = {}
        vm.page.menu.currentSection = appState.getSectionState("currentSection")
        vm.page.menu.currentNode = null

        vm.file = {}
        vm.isWebConfig = false

        // bind functions to view model
        vm.save = save

        /* Functions bound to view model */

        function save() {
            vm.page.saveButtonState = "busy";
            if (vm.isWebConfig) {
                localizationService.localizeMany(["configurationEditor_save", "general_cancel", "configurationEditor_yesSave", "configurationEditor_saveFileConfirmation"])
                    .then(function (data) {

                        const overlay = {
                            title: data[0],
                            content: data[3],
                            closeButtonLabel: data[1],
                            submitButtonLabel: data[2],
                            confirmMessageStyle: "danger",
                            submitButtonStyle: "danger",
                            close: function () {
                                vm.page.saveButtonState = "error";
                                overlayService.close()
                            },
                            submit: function () {
                                performSave()
                                overlayService.close()
                            }
                        };
                        overlayService.confirm(overlay)

                    })
            } else {
                performSave()
            }
        }
        function performSave() {

            vm.file.content = vm.editor.getValue();

            contentEditingHelper.contentEditorPerformSave({
                saveMethod: configurationEditorResource.saveFile,
                scope: $scope,
                content: vm.file,
                rebindCallback: function (original, saved) { }
            }).then(function (saved) {

                localizationService.localizeMany(["speechBubbles_fileSavedHeader", "speechBubbles_fileSavedText"]).then(function (data) {
                    notificationsService.success(data[0], data[1])
                });

                //check if the name changed, if so we need to redirect
                if (vm.file.id !== saved.id) {
                    contentEditingHelper.redirectToRenamedContent(saved.id)
                }
                else {
                    vm.page.saveButtonState = "success";
                    vm.file = saved

                    //sync state
                    editorState.set(vm.file)

                    // sync tree
                    navigationService.syncTree({ tree: treeName, path: vm.file.path, forceReload: true }).then(function (syncArgs) {
                        vm.page.menu.currentNode = syncArgs.node;
                    })
                }

            }, function (err) {

                vm.page.saveButtonState = "error"

                localizationService.localizeMany(["speechBubbles_validationFailedHeader", "speechBubbles_validationFailedMessage"]).then(function (data) {
                    notificationsService.error(data[0], data[1])
                });

            });

        }

        /* Local functions */

        function init() {
            //we need to load this somewhere, for now its here.
            assetsService.loadCss("lib/ace-razor-mode/theme/razor_chrome.css");

            configurationEditorResource.getFile($routeParams.id).then(
                function (file) {
                    ready(file, true)
                })

        }

        function ready(file, syncTree) {

            vm.page.loading = false

            vm.file = file
            vm.isWebConfig = file.name.toLowerCase() === "appsettings.json"

            vm.setDirty = function () {
                setFormState("dirty")
            }

            //sync state
            editorState.set(vm.file)

            if (syncTree) {
                navigationService.syncTree({ tree: treeName, path: vm.file.path, forceReload: true }).then(function (syncArgs) {
                    vm.page.menu.currentNode = syncArgs.node
                });
            }

            // ace configuration
            const ext = file.name.split('.').pop().toLowerCase()

            // TODO: Add in support for XML.
            const mode = ext === "js" || ext === "json" ? "javascript" : "xml"

            vm.aceOption = createAceOptions(mode)
        }

        function createAceOptions(mode) {
            if (mode === 'xml') {
                ace.config.setModuleUrl("ace/snippets/xml", '/App_Plugins/ConfigurationEditor/lib/ace-builds/src-min-noconflict/snippets/xml.js');
                // this doesn't work:
                //ace.config.setModuleUrl("ace/worker-xml", '/App_Plugins/ConfigurationEditor/lib/ace-builds/src-min-noconflict/worker-xml.js');
            }
            return {
                mode: mode,
                theme: "chrome",
                showPrintMargin: false,
                showGutter: true,
                advanced: {
                    fontSize: '14px',
                    enableSnippets: true,
                    enableBasicAutocompletion: true,
                    enableLiveAutocompletion: false
                },
                onLoad: function (_editor) {
                    vm.editor = _editor;

                    //change on blur, focus
                    vm.editor.on("blur", persistCurrentLocation)
                    vm.editor.on("focus", persistCurrentLocation)
                    vm.editor.on("change", changeAceEditor)

                    //Update the auto-complete method to use ctrl+alt+space
                    _editor.commands.bindKey("ctrl-alt-space", "startAutocomplete")

                    //Unassigns the keybinding (That was previously auto-complete)
                    //As conflicts with our own tree search shortcut
                    _editor.commands.bindKey("ctrl-space", null)

                    // TODO: Move all these keybinding config out into some helper/service
                    _editor.commands.addCommands([
                        //Disable (alt+shift+K)
                        //Conflicts with our own show shortcuts dialog - this overrides it
                        {
                            name: 'unSelectOrFindPrevious',
                            bindKey: 'Alt-Shift-K',
                            exec: function () {
                                //Toggle the show keyboard shortcuts overlay
                                $scope.$apply(function () {
                                    vm.showKeyboardShortcut = !vm.showKeyboardShortcut
                                })
                            },
                            readOnly: true
                        }
                    ]);

                    $timeout(function () {
                        // initial cursor placement
                        vm.editor.focus()
                        persistCurrentLocation()
                    });

                    vm.editor.on("change", changeAceEditor)
                }
            }
        }

        function persistCurrentLocation() {
            vm.currentPosition = vm.editor.getCursorPosition()
        }

        function changeAceEditor() {
            setFormState("dirty")
        }

        function setFormState(state) {

            // get the current form
            var currentForm = angularHelper.getCurrentForm($scope)

            // set state
            if (state === "dirty") {
                currentForm.$setDirty()
            } else if (state === "pristine") {
                currentForm.$setPristine()
            }
        }

        function setFormState(state) {

            // get the current form
            var currentForm = angularHelper.getCurrentForm($scope)

            // set state
            if (state === "dirty") {
                currentForm.$setDirty()
            } else if (state === "pristine") {
                currentForm.$setPristine()
            }
        }

        // force loading the xml mode before initialising.
        assetsService.load([
            'lib/ace-builds/src-min-noconflict/ace.js',
            'lib/ace-builds/src-min-noconflict/ext-language_tools.js',
            '/App_Plugins/ConfigurationEditor/lib/ace-builds/src-min-noconflict/mode-xml.js',
        ], $scope).then(function () {
            if (angular.isUndefined(window.ace)) {
                throw new Error('ui-ace need ace to work... (o rly?)');
            } else {
                init()
            }
        })


    }

    angular.module("umbraco").controller("ConfigurationEditor.Editors.EditController", configurationEditorEditController)
})()