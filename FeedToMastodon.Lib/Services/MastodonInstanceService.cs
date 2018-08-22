/*
    Copyright (c) 2018 Christoph Jahn.

    This file is licensed to you under the MIT license.
    See the LICENSE file in the project root for more information.
*/

using System.Threading.Tasks;
using FeedToMastodon.Lib.Interfaces;
using mastodon;
using mastodon.Enums;

namespace FeedToMastodon.Lib.Services
{
    /*
        The real implementation of the InstanceService.
        Should connect and work with servers.
    */
    public class MastodonInstanceService : IInstanceService
    {
        private readonly IAppConfiguration cfg;

        // We only write 'toot's
        private readonly AppScopeEnum scopes = AppScopeEnum.Write;

        public MastodonInstanceService(IAppConfiguration configuration)
        {
            this.cfg = configuration;
        }

        // Register the application with a mastodon instance,
        // retreive client-credentials and
        // save them to configuration.
        public async Task<bool> RegisterApplication(string instance, string appName, string appSite)
        {
            try
            {
                using (var appHandler = new AppHandler(instance))
                {
                    var appData = await appHandler.CreateAppAsync(appName, scopes, appSite);

                    cfg.GetConfiguration().Instance.Name = instance;
                    cfg.GetConfiguration().Instance.ClientId = appData.client_id;
                    cfg.GetConfiguration().Instance.ClientSecret = appData.client_secret;

                    cfg.Save();
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        public async Task<bool> FetchAccessToken(string email, string password)
        {
            var instance = cfg.GetConfiguration().Instance;

            // Skip when not registered
            if (
                string.IsNullOrWhiteSpace(instance.Name) ||
                string.IsNullOrWhiteSpace(instance.ClientId) ||
                string.IsNullOrWhiteSpace(instance.ClientSecret)
            )
            {
                return false;
            }

            try
            {
                using (var authHandler = new AuthHandler(instance.Name))
                {
                    var tokenInfo = await authHandler
                        .GetTokenInfoAsync(instance.ClientId, instance.ClientSecret, email, password, scopes);

                    if (tokenInfo.access_token == null)
                        return false;

                    cfg.GetConfiguration().Instance.AccessToken = tokenInfo.access_token;
                    cfg.Save();
                }
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}