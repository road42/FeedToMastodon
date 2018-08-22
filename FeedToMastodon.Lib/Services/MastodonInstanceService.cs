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

                    // Save appData
                    cfg.Application.Instance.Name = instance;
                    cfg.Application.Instance.ClientId = appData.client_id;
                    cfg.Application.Instance.ClientSecret = appData.client_secret;

                    cfg.Save();

                    return true;
                }
            }
            catch
            {
            }

            return false;
        }

        public async Task<bool> FetchAccessToken(string email, string password)
        {
            // Skip when not registered
            if (!cfg.ClientCredentialsSaved)
                return false;

            var instance = cfg.Application.Instance;

            // Fetch accessToken
            try
            {
                using (var authHandler = new AuthHandler(instance.Name))
                {
                    var tokenInfo = await authHandler
                        .GetTokenInfoAsync(instance.ClientId, instance.ClientSecret, email, password, scopes);

                    // Is null if registration failed (e.g. if two-factor is enabled)
                    if (tokenInfo.access_token == null)
                        return false;

                    // Save token
                    cfg.Application.Instance.AccessToken = tokenInfo.access_token;
                    cfg.Save();

                    return true;
                }
            }
            catch
            {
            }

            return false;
        }

        public async Task<bool> Toot(string status, StatusVisibilityEnum visibility = StatusVisibilityEnum.Public, int inReplyToId = -1, int[] mediaIds = null, bool sensitive = false, string spoilerText = null)
        {
            // Only if configured
            if (!cfg.FullInstanceRegistrationCompleted)
                return false;

            var instance = cfg.Application.Instance;

            using (var mastodonClient = new MastodonClient(instance.Name))
            {
                var postResult = await mastodonClient
                    .PostNewStatusAsync(
                        instance.AccessToken,
                        status, visibility,
                        inReplyToId,
                        mediaIds,
                        sensitive,
                        spoilerText);

                if (postResult.created_at != null)
                    return true;
            }

            return false;
        }
    }
}