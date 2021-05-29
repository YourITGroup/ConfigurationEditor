using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;

namespace ConfigurationEditor.Composing
{
    public class ConfigTreeComposer : IUserComposer, IComposer, IDiscoverable
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.AddNotificationHandler<ServerVariablesParsingNotification, ConfigTreeNotificatonHandler>();

        }
    }
}
