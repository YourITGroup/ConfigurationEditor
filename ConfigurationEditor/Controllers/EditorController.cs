using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.Editors;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using UmbracoConfigTree.Extensions;
using UmbConstants = Umbraco.Core.Constants;

namespace UmbracoConfigTree.Controllers
{
    [UmbracoTreeAuthorize(
        Constants.Trees.ConfigurationFiles,
        Roles = UmbConstants.Security.AdminGroupAlias
        )]
    [PluginController(Constants.PluginName)]
    public class EditorController : BackOfficeNotificationsController
    {
        private readonly IFileSystem _fileSystem = new PhysicalFileSystem("~/");

        private readonly string[] FileExtensions = new[] { "config", "json", "js", "xml" };

        public EditorController(IGlobalSettings globalSettings, IUmbracoContextAccessor umbracoContextAccessor,
                                ISqlContext sqlContext, ServiceContext services, AppCaches appCaches,
                                IProfilingLogger logger, IRuntimeState runtimeState, UmbracoHelper umbracoHelper) : 
            base(globalSettings, umbracoContextAccessor, sqlContext, services, appCaches, logger, runtimeState, umbracoHelper)
        {
        }

        [HttpGet]
        public CodeFileDisplay GetFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Path cannot be null or whitespace.", nameof(path));

            path = HttpUtility.UrlDecode(path);

            return _fileSystem.FileExists(path) ? _fileSystem.PathToCodeFile(path) : throw new HttpResponseException(HttpStatusCode.NotFound);
        }


        [HttpPost]
        public CodeFileDisplay SaveFile(CodeFileDisplay file)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));

            if (!ModelState.IsValid)
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));

            if (!FileExtensions.InvariantContains(file.FileType))
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            // if the file is the root web.config file, save a backup.            
            CreateOrUpdateFile(file);

            return _fileSystem.PathToCodeFile(file.VirtualPath);
        }

        private void CreateOrUpdateFile(CodeFileDisplay file)
        {
            // if the file is the root web.config file, save a backup of the original.
            if (file.VirtualPath.InvariantEndsWith("web.config"))
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

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (StreamWriter streamWriter = new StreamWriter(memoryStream))
                {
                    streamWriter.Write(file.Content);
                    streamWriter.Flush();
                    _fileSystem.AddFile(file.VirtualPath.TrimStart('/'), memoryStream, true);
                }
            }
        }

    }
}
