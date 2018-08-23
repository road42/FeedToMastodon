/*
    Copyright (c) 2018 Christoph Jahn.

    This file is licensed to you under the MIT license.
    See the LICENSE file in the project root for more information.
*/

using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FeedToMastodon.Lib.Interfaces;
using FeedToMastodon.Lib.Models;
using mastodon;
using mastodon.Enums;
using Microsoft.Extensions.Logging;

namespace FeedToMastodon.Lib.Services
{
    /*
        The real implementation of the InstanceService.
        Should connect and work with servers.
    */
    public class MastodonInstanceService : IInstanceService
    {
        private readonly ILogger<MastodonInstanceService> log;
        private readonly IAppConfiguration cfg;

        // We only write 'toot's
        private readonly AppScopeEnum scopes = AppScopeEnum.Write;

        public MastodonInstanceService(ILogger<MastodonInstanceService> logger, IAppConfiguration configuration)
        {
            this.log = logger;
            this.cfg = configuration;
        }

        // Register the application with a mastodon instance,
        // retreive client-credentials and
        // save them to configuration.
        public async Task<bool> RegisterApplication(string instance, string appName, string appSite)
        {
            using (log.BeginScope($"{ nameof(MastodonInstanceService) }->{ nameof(RegisterApplication) } with instance: {instance}, appName: {appName}, appSite: {appSite}"))
            {
                try
                {
                    log.LogDebug("Create mastodon-AppHandler");

                    using (var appHandler = new AppHandler(instance))
                    {
                        var appData = await appHandler.CreateAppAsync(appName, scopes, appSite);

                        log.LogDebug("appData returned");

                        // Save appData
                        cfg.Application.Instance.Name = instance;
                        cfg.Application.Instance.ClientId = appData.client_id;
                        cfg.Application.Instance.ClientSecret = appData.client_secret;

                        cfg.Save();
                    }
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "RegisterApplication - Exception");
                    return false;
                }
            }

            return true;
        }

        public async Task<bool> FetchAccessToken(string email, string password)
        {
            using (log.BeginScope($"{ nameof(MastodonInstanceService) }->{ nameof(FetchAccessToken) }"))
            {
                // Skip when not registered
                if (!cfg.ClientCredentialsSaved)
                {
                    log.LogError("No clientCredentials given.");
                    return false;
                }

                var instance = cfg.Application.Instance;

                // Fetch accessToken
                try
                {
                    log.LogDebug("Create AuthHandler");

                    using (var authHandler = new AuthHandler(instance.Name))
                    {
                        var tokenInfo = await authHandler
                            .GetTokenInfoAsync(instance.ClientId, instance.ClientSecret, email, password, scopes);

                        log.LogDebug("AuthHandler returned");

                        // Is null if registration failed (e.g. if two-factor is enabled)
                        if (tokenInfo.access_token == null)
                        {
                            log.LogError("AccessToken is null");
                            return false;
                        }

                        // Save token
                        log.LogDebug("Save new config");
                        cfg.Application.Instance.AccessToken = tokenInfo.access_token;
                        cfg.Save();
                    }
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "FetchAccessToken - Exception");
                    return false;
                }

                return true;
            }
        }

        public async Task<bool> Toot(string status, StatusVisibilityEnum visibility = StatusVisibilityEnum.Public, int inReplyToId = -1, int[] mediaIds = null, bool sensitive = false, string spoilerText = null)
        {
            using (log.BeginScope($"{ nameof(MastodonInstanceService) }->{ nameof(Toot) } "))
            {
                log.LogDebug(" Parameters - status: {status}, visibility: {visibility}, inReplyToId: {inReplyToId}, mediaIds: {mediaIds}, sensitive: {sensitive}, spoilerText: {spoilerText}",
                        status,
                        visibility,
                        inReplyToId,
                        mediaIds,
                        sensitive,
                        spoilerText
                        );

                // Only if configured
                if (!cfg.FullInstanceRegistrationCompleted)
                {
                    log.LogError("No instanceRegistration given.");
                    return false;
                }

                var instance = cfg.Application.Instance;

                log.LogDebug("Create MastodonClient");

                try
                {
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

                        log.LogDebug("PostNewStatusAsync returned");

                        if (postResult.created_at == null)
                        {
                            log.LogError("postResult.created_at is null");
                            return false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "Toot - Exception");
                    return false;
                }

                return true;
            }
        }

        public async Task<bool> TootFeedEntry(FeedEntry feedEntry, string template = null, StatusVisibilityEnum? visibility = null)
        {
            using (log.BeginScope($"{ nameof(MastodonInstanceService) }->{ nameof(Toot) } " + " with feedEntry: {feedEntry}, template: {template}, visibility: {visibility}",
                        feedEntry,
                        template,
                        visibility
                        ))
            {
                var tootTemplate = template ?? cfg.Application.Toot.Template;
                var tootVisibility = visibility ?? cfg.Application.Toot.Visibiliy;

                log.LogDebug("Used template: {tootTemplate} and {tootVisibility}", tootTemplate, tootVisibility);

                var sb = new StringBuilder(tootTemplate);

                sb.Replace("{feedname}", feedEntry.FeedTitle);
                sb.Replace("{id}", feedEntry.Id);
                sb.Replace("{title}", feedEntry.Title);
                sb.Replace("{summary}", feedEntry.Summary);
                sb.Replace("{description}", feedEntry.Description);
                sb.Replace("{link}", feedEntry.Link);
                sb.Replace("{published}", feedEntry.Published.ToString(cfg.Application.Toot.DateFormatString));
                sb.Replace("{lastUpdated}", feedEntry.LastUpdated.ToString(cfg.Application.Toot.DateFormatString));

                foreach (var tag in feedEntry.Tags)
                {
                    sb.Append($" #{Regex.Replace(tag, "[^\\w]", "").ToLower()}");
                }

                log.LogDebug("Final message: {status}", sb);
                return await Toot(sb.ToString(), tootVisibility);
            }
        }
    }
}