/*
    Copyright (c) 2018 Christoph Jahn.

    This file is licensed to you under the MIT license.
    See the LICENSE file in the project root for more information.
*/

using System;
using System.Threading.Tasks;
using FeedToMastodon.Lib.Interfaces;

namespace FeedToMastodon.Lib.Services.Dummy
{
    /*
        A dummy-implementation of the InstanceService just does everything
        InMemory for tests and development.
    */
    public class InstanceService : Interfaces.IInstanceService
    {
        private readonly IAppConfiguration cfg;

        private string appName;
        private string appSite;

        public InstanceService(IAppConfiguration configuration)
        {
            this.cfg = configuration;
        }

        public Task<bool> RegisterApplication(string instance, string appName, string appSite)
        {
            /*
                Only set the data here and save it.
            */
            this.appName = appName;
            this.appSite = appSite;

            cfg.GetConfiguration().Instance.Name = $"https://{instance}";

            // Generate DummyId
            cfg.GetConfiguration().Instance.ClientId = $"DummyId-{Guid.NewGuid()}";
            cfg.GetConfiguration().Instance.ClientSecret = $"DummySecret-{Guid.NewGuid()}";

            if (cfg.Save())
                return Task.FromResult(true);

            return Task.FromResult(false);
        }

        public Task<bool> FetchAccessToken(string email, string password)
        {
            // Skip when not registered
            if (
                string.IsNullOrWhiteSpace(cfg.GetConfiguration().Instance.Name) ||
                string.IsNullOrWhiteSpace(cfg.GetConfiguration().Instance.ClientId) ||
                string.IsNullOrWhiteSpace(cfg.GetConfiguration().Instance.ClientSecret)
            )
            {
                return Task.FromResult(false);
            }

            cfg.GetConfiguration().Instance.AccessToken = $"refresh-{Guid.NewGuid()}";
            cfg.Save();

            return Task.FromResult(true);
        }
    }
}