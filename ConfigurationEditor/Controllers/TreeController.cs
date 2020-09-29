using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.Trees;
using Umbraco.Web.WebApi.Filters;
using UmbConstants = Umbraco.Core.Constants;

namespace UmbracoConfigTree.Controllers
{
    [UmbracoTreeAuthorize(
        Constants.Trees.ConfigurationFiles,
        Roles = UmbConstants.Security.AdminGroupAlias
        )]
    [Tree(UmbConstants.Applications.Settings, Constants.Trees.ConfigurationFiles, SortOrder = 20, TreeGroup = UmbConstants.Trees.Groups.Settings)]
    [PluginController(Constants.PluginName)]
    public class TreeController : FileSystemTreeController
    {
        protected override IFileSystem FileSystem => new PhysicalFileSystem("~/");
        private static readonly string[] ExtensionsStatic = { "config", "json", "xml", "js" };

        protected override string[] Extensions => ExtensionsStatic;

        protected override string FileIcon => "icon-tools";

        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            var nodes = base.GetTreeNodes(id, queryStrings);
            var removalList = new List<TreeNode>();
            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                if (!(IsAllowedDirectory(node) || IsAllowedFile(node.Name)))
                {
                    removalList.Add(node);
                }
            }

            foreach (var node in removalList)
            {
                nodes.Remove(node);
            }

            return nodes;
        }

        private bool IsAllowedDirectory(TreeNode node)
        {
            bool allowed = false;

            if (node.Id.ToString().InvariantStartsWith("config") && node.HasChildren)
            {
                var path = HttpUtility.UrlDecode(node.Id.ToString());

                // check if the directory contains allowed files or other directories.
                allowed = FileSystem.GetFiles(path).Any(IsAllowedFile) || FileSystem.GetDirectories(path).Any();
            }
            return allowed;
        }

        private bool IsAllowedFile(string name)
        {
            return Extensions.Contains(name.Split(new[] { '.' }).Last());
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            //if root node no need to visit the filesystem so lets just create the menu and return it
            if (id == UmbConstants.System.RootString)
            {
                return GetMenuForRootNode(queryStrings);
            }

            var menu = new MenuItemCollection();

            var path = string.IsNullOrEmpty(id) == false && id != UmbConstants.System.RootString
                ? HttpUtility.UrlDecode(id).TrimStart("/")
                : "";

            var isFile = FileSystem.FileExists(path);
            var isDirectory = FileSystem.DirectoryExists(path);

            if (isDirectory)
            {
                return GetMenuForFolder(path, queryStrings);
            }

            if (isFile)
            {
                return null;
            }

            menu.Items.Add(new RefreshNode(Services.TextService, true));
            return menu;
        }
        protected override MenuItemCollection GetMenuForRootNode(FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            //refresh action
            menu.Items.Add(new RefreshNode(Services.TextService, true));

            return menu;
        }

        protected override MenuItemCollection GetMenuForFolder(string path, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            //refresh action
            menu.Items.Add(new RefreshNode(Services.TextService, true));

            return menu;
        }

    }
}
