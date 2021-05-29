using Microsoft.Extensions.Logging;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Extensions;

namespace ConfigurationEditor.Extensions
{
    internal static class IFileSystemExtensions
    {
        /// <summary>
        /// Returns a FileSystem Tree from the virtul path
        /// </summary>
        /// <param name="virtualPath"></param>
        /// <returns></returns>
        internal static string ToTree(this string virtualPath)
        {
            virtualPath = virtualPath.TrimStart('~')
                                     .Replace('\\', '/');

            StringBuilder stringBuilder = new("-1");
            string[] segments = virtualPath.Split('/');
            for (int index = 0; index < segments.Length; index++)
            {
                stringBuilder.Append($",{HttpUtility.UrlEncode(string.Join("/", segments.Take(index + 1)))}");
            }
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Creates a CodeFileDisplay object from a virtual filesystem path.
        /// </summary>
        /// <param name="fileSystem"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        internal static CodeFileDisplay PathToCodeFile(this IFileSystem fileSystem, string path)
        {
            using (StreamReader streamReader = new(fileSystem.OpenFile(path)))
            {
                CodeFileDisplay codeFileDisplay = new()
                {
                    Content = streamReader.ReadToEnd(),
                    FileType = Path.GetExtension(path),
                    Id = HttpUtility.UrlEncode(path),
                    Name = Path.GetFileName(path),
                    Path = path.ToTree(),
                    VirtualPath = path
                };
                codeFileDisplay.FileType = Path.GetExtension(path).TrimStart('.');
                return codeFileDisplay;
            }
        }

        internal static bool IsConfigurationFile(this CodeFileDisplay file)
        {
            return Constants.Plugin.FileExtensions.InvariantContains(file.FileType);
        }

        /// <summary>
        /// Ensures the provided filename has the provided extension
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        internal static string EnsureCorrectFileExtension(this string filename, string extension)
        {
            if (!extension.InvariantStartsWith("."))
            {
                extension = $".{extension}";
            }

            if (!filename.EndsWith(extension))
            {
                return $"{filename}{extension}";
            }

            return filename;
        }

        /// <summary>
        /// Gets a representation of the physical file system at root.
        /// </summary>
        /// <param name="hostingEnvironment"></param>
        /// <param name="ioHelper"></param>
        /// <param name="loggerFactory"></param>
        /// <returns></returns>
        internal static PhysicalFileSystem GetRootFileSystem(this IHostingEnvironment hostingEnvironment,
            IIOHelper ioHelper,
            ILoggerFactory loggerFactory)
        {
            var rootPath = hostingEnvironment.MapPathContentRoot(Constants.Plugin.FileSystemRoot);
            var rootUrl = hostingEnvironment.ToAbsolute(Constants.Plugin.FileSystemRoot);
            return new PhysicalFileSystem(ioHelper, hostingEnvironment, loggerFactory.CreateLogger<PhysicalFileSystem>(), rootPath, rootUrl);

        }
    }
}
