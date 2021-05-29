using ConfigurationEditor.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;
using UmbConstants = Umbraco.Cms.Core.Constants;

namespace ConfigurationEditor.Controllers
{
    [Authorize(Policy = AuthorizationPolicies.SectionAccessSettings, Roles = UmbConstants.Security.AdminGroupAlias)]
    [PluginController(Constants.Plugin.PluginName)]
    public class EditorController : BackOfficeNotificationsController
    {
        private readonly ILogger<EditorController> _logger;
        private readonly PhysicalFileSystem _fileSystem;
        private readonly ILocalizedTextService _localizedTextService;

        public EditorController(
            IHostingEnvironment hostingEnvironment,
            ILoggerFactory loggerFactory,
            IIOHelper ioHelper,
            ILocalizedTextService localizedTextService
            )
        {
            _logger = loggerFactory.CreateLogger<EditorController>();

            _fileSystem = hostingEnvironment.GetRootFileSystem(ioHelper, loggerFactory);
            _localizedTextService = localizedTextService;
        }

        [HttpGet]
        public ActionResult<CodeFileDisplay> GetByPath(string virtualPath)
        {
            if (string.IsNullOrWhiteSpace(virtualPath)) throw new ArgumentException("Value cannot be null or whitespace.", "virtualPath");

            virtualPath = System.Web.HttpUtility.UrlDecode(virtualPath);

            return _fileSystem.FileExists(virtualPath) ? _fileSystem.PathToCodeFile(virtualPath) : NotFound();
        }


        /// <summary>
        /// Used to create or update a 'partialview', 'partialviewmacro', 'script' or 'stylesheets' file
        /// </summary>
        /// <param name="display"></param>
        /// <returns>The updated CodeFileDisplay model</returns>
        public ActionResult<CodeFileDisplay> PostSave(CodeFileDisplay display)
        {
            if (display == null) throw new ArgumentNullException(nameof(display));

            TryValidateModel(display);
            if (ModelState.IsValid == false)
            {
                return ValidationProblem(ModelState);
            }

            if (!display.IsConfigurationFile())
            {
                return NotFound();
            }

            // if the file is the root web.config file, save a backup.            
            var result = UpdateFile(display);

            if (result.Success)
            {
                return result.Result;
            }

            display.AddErrorNotification(
                _localizedTextService.Localize("speechBubbles/configFileErrorHeader"),
                _localizedTextService.Localize("speechBubbles/configFileErrorText"));

            return display;
        }

        private Attempt<CodeFileDisplay> UpdateFile(CodeFileDisplay file)
        {
            // if the file is an appsettings.json file, save a backup of the original.
            if (file.VirtualPath.InvariantEndsWith("appsettings.json"))
            {
                _fileSystem.CopyFile(file.VirtualPath, $"{file.VirtualPath}.{DateTime.Now:yyyyMMdd-HHmmss}");
            }

            //            file.VirtualPath = file.VirtualPath.EnsureCorrectFileExtension(configFileExtension);
            //            file.Name = file.Name.EnsureCorrectFileExtension(configFileExtension);

            //if (!Path.GetFileNameWithoutExtension(file.VirtualPath).InvariantEquals(Path.GetFileNameWithoutExtension(file.Name)))
            //{
            //    _themesFileSystem.DeleteFile(file.VirtualPath);
            //    string[] strArray = file.VirtualPath.Split('/');
            //    file.VirtualPath = string.Join("/", ((IEnumerable<string>)strArray).Take(strArray.Length - 1)).EnsureEndsWith('/') + file.Name;
            //}

            Attempt<CodeFileDisplay> attempt;
            try
            {
                using (MemoryStream memoryStream = new())
                {
                    using StreamWriter streamWriter = new(memoryStream);
                    streamWriter.Write(file.Content);
                    streamWriter.Flush();
                    _fileSystem.AddFile(file.VirtualPath.TrimStart('/'), memoryStream, true);
                }
                attempt = Attempt<CodeFileDisplay>.Succeed(file);
            } catch (IOException ioe)
            {
                _logger.LogError(ioe, "Could not save file {file}", file.Path);
                attempt = Attempt<CodeFileDisplay>.Fail();
            }

            return attempt;
        }

    }
}
