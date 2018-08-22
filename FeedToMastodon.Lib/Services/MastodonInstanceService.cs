/*
    Copyright (c) 2018 Christoph Jahn.

    This file is licensed to you under the MIT license.
    See the LICENSE file in the project root for more information.
*/

using FeedToMastodon.Lib.Interfaces;

namespace FeedToMastodon.Lib.Services
{
    /*
        The real implementation of the InstanceService.
        Should connect and work with servers.
    */
    public class MastodonInstanceService : IInstanceService
    {
        private readonly IAppConfiguration configuration;

        public MastodonInstanceService(IAppConfiguration configuration)
        {
            this.configuration = configuration;
        }

        // Register the application with a mastodon instance,
        // retreive client-credentials and
        // save them to configuration.
        // QUESTION: What to do, when there ist already a registration in the configuration?
        public bool RegisterApplication(string instance, string appName, string appSite)
        {
            /*
                TODO: Add config file option to write data to

                Example source for registering:

                using (var appHandler = new AppHandler(InstanceName))
                {
                    var scopes = AppScopeEnum.Write;
                    var appData = appHandler.CreateAppAsync(ClientName, scopes, Website).Result;
                }
             */
            return true;
        }
    }
}