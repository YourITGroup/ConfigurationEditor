using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Web;
using Umbraco.Web.JavaScript;
using UmbracoConfigTree.Controllers;

namespace UmbracoConfigTree.Composing
{
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class ConfigTreeComposer : IUserComposer, IComposer, IDiscoverable
    {
        public void Compose(Composition composition)
        {
            composition.Components().Append<ConfigTreeComponent>();
        }
    }

    public class ConfigTreeComponent : IComponent
    {
        public ConfigTreeComponent()
        {
        }

        public void Initialize()
        {
            ServerVariablesParser.Parsing += new EventHandler<Dictionary<string, object>>(ServerVariablesParser_Parsing);
        }

        private void ServerVariablesParser_Parsing(object sender, Dictionary<string, object> e)
        {
            if (HttpContext.Current == null)
                throw new InvalidOperationException("HttpContext is null");
            
            UrlHelper urlHelper = new UrlHelper(new RequestContext(new HttpContextWrapper(HttpContext.Current), new RouteData()));

            string configurationFilesEditorsBaseUrl = urlHelper.GetUmbracoApiServiceBaseUrl<EditorController>(controller => controller.GetFile(null));

            Dictionary<string, object> umbracoUrls;
            if (e.TryGetValue("umbracoUrls", out object found))
            {
                umbracoUrls = found as Dictionary<string, object>;
            }
            else
            {
                umbracoUrls = new Dictionary<string, object>();
                e["umbracoUrls"] = umbracoUrls;
            }

            if (!umbracoUrls.ContainsKey(nameof(configurationFilesEditorsBaseUrl)))
            {
                umbracoUrls[nameof(configurationFilesEditorsBaseUrl)] = configurationFilesEditorsBaseUrl;
            }
        }

        public void Terminate()
        {
        }
    }
}
