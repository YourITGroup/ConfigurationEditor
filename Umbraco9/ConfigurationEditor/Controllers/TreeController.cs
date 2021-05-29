using ConfigurationEditor.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models.Trees;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Trees;
using Umbraco.Cms.Web.BackOffice.Trees;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;
using UmbConstants = Umbraco.Cms.Core.Constants;

namespace ConfigurationEditor.Controllers
{
    [Authorize(Policy = AuthorizationPolicies.SectionAccessSettings, Roles = UmbConstants.Security.AdminGroupAlias)]
    [Tree(UmbConstants.Applications.Settings, Constants.Trees.ConfigurationEditor, SortOrder = 20, TreeGroup = UmbConstants.Trees.Groups.Settings)]
    [PluginController(Constants.Plugin.PluginName)]
    public class TreeController : FileSystemTreeController
    {
        protected override IFileSystem FileSystem { get; }
        private static readonly string[] BaseExtensions = { "config", "json", "xml" };

        public TreeController(
                              IHostingEnvironment hostingEnvironment,
                              ILoggerFactory loggerFactory,
                              IIOHelper ioHelper, 
                              ILocalizedTextService localizedTextService,
                              Umbraco.Cms.Core.UmbracoApiControllerTypeCollection umbracoApiControllerTypeCollection,
                              IMenuItemCollectionFactory menuItemCollectionFactory, IEventAggregator eventAggregator) : 
            base(localizedTextService, umbracoApiControllerTypeCollection, menuItemCollectionFactory, eventAggregator)
        {
            FileSystem = hostingEnvironment.GetRootFileSystem(ioHelper, loggerFactory);
        }

        protected override string[] Extensions => Constants.Plugin.FileExtensions;

        protected override string FileIcon => "icon-tools";

        protected override ActionResult<TreeNodeCollection> GetTreeNodes(string id, FormCollection queryStrings)
        {
            //var path = string.IsNullOrEmpty(id) == false && id != UmbConstants.System.RootString
            //    ? WebUtility.UrlDecode(id).TrimStart("/")
            //    : "";

            var nodes = base.GetTreeNodes(id, queryStrings).Value;
            if (!nodes.Any())
            {
                return nodes;
            }

            var removalList = new List<TreeNode>();
            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                if (!(IsAllowedDirectory(node, queryStrings, id == UmbConstants.System.RootString) || IsAllowedFile(node.Name, id.InvariantStartsWith("config"))))
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

        private bool IsAllowedDirectory(TreeNode node, FormCollection queryStrings, bool isRootNode)
        {
            bool allowed = false;
            var nodeId = node.Id.ToString();
            var path = System.Web.HttpUtility.UrlDecode(nodeId);

            // Default config directory test.
            if (nodeId.InvariantStartsWith("config") && node.HasChildren)
            {
                // check if the directory contains allowed files or other directories.
                allowed = FileSystem.GetFiles(path).Any(n => IsAllowedFile(n, true)) || FileSystem.GetDirectories(path).Any();
            }

            if (!allowed && nodeId.InvariantStartsWith("views"))
            {
                allowed = FileSystem.GetFiles(path).Any(n => IsAllowedFile(n));
            }

            // Test for Umbraco Forms config file - found in /App_Plugins/UmbracoForms/
            if (!allowed)
            {
                if (path.InvariantEquals("app_plugins"))
                {
                    allowed = FileSystem.GetDirectories(path).Any();

                }
                else if (path.InvariantStartsWith("app_plugins") && node.HasChildren)
                {
                    allowed = FileSystem.GetFiles(path).Any(n => IsAllowedFile(n)) || FileSystem.GetDirectories(path).Any();
                }
            }

            if (!isRootNode)
            {
                allowed &= GetTreeNodes(path, queryStrings).Value.Any();
            }
            return allowed;
        }

        private bool IsAllowedFile(string name, bool isConfigDirectory = false)
        {
            var extensionList = isConfigDirectory ? Extensions : BaseExtensions;
            return extensionList.Contains(name.Split(new[] { '.' }).Last());
        }

        //protected override ActionResult<MenuItemCollection> GetMenuForNode(string id, FormCollection queryStrings)
        //{
        //    //if root node no need to visit the filesystem so lets just create the menu and return it
        //    if (id == UmbConstants.System.RootString)
        //    {
        //        return GetMenuForRootNode(queryStrings);
        //    }

        //    var menu = MenuItemCollectionFactory.Create();

        //    var path = string.IsNullOrEmpty(id) == false && id != UmbConstants.System.RootString
        //        ? System.Web.HttpUtility.UrlDecode(id).TrimStart("/")
        //        : "";

        //    var isFile = FileSystem.FileExists(path);
        //    var isDirectory = FileSystem.DirectoryExists(path);

        //    if (isDirectory)
        //    {
        //        return GetMenuForFolder(path, queryStrings);
        //    }

        //    if (isFile)
        //    {
        //        return null;
        //    }

        //    menu.Items.Add(new RefreshNode(LocalizedTextService, true));
        //    return menu;
        //}

        protected override MenuItemCollection GetMenuForRootNode(FormCollection queryStrings)
        {
            var menu = MenuItemCollectionFactory.Create();

            //refresh action
            menu.Items.Add(new RefreshNode(LocalizedTextService, true));

            return menu;
        }

        protected override MenuItemCollection GetMenuForFile(string path, FormCollection queryStrings)
        {
            var menu = MenuItemCollectionFactory.Create();

            return menu;
        }

        protected override MenuItemCollection GetMenuForFolder(string path, FormCollection queryStrings)
        {
            var menu = MenuItemCollectionFactory.Create();

            //refresh action
            menu.Items.Add(new RefreshNode(LocalizedTextService, true));

            return menu;
        }

    }
}
