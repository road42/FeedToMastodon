/*
    Copyright (c) 2018 Christoph Jahn.

    This file is licensed to you under the MIT license.
    See the LICENSE file in the project root for more information.
*/

using System.Threading.Tasks;
using FeedToMastodon.Lib.Models;
using mastodon.Enums;

namespace FeedToMastodon.Lib.Interfaces
{
    /*
        Provides the connection to a mastodon-instance and
        methods to work with it.
     */
    public interface IInstanceService
    {
        Task<bool> RegisterApplication(string instance, string appName, string appSite);

        Task<bool> FetchAccessToken(string email, string password);

        Task<bool> TootFeedEntry(FeedEntry feedEntry, string template = null, StatusVisibilityEnum? visibility = null);

        Task<bool> Toot(string status, StatusVisibilityEnum visibility = StatusVisibilityEnum.Public, int inReplyToId = -1, int[] mediaIds = null, bool sensitive = false, string spoilerText = null);
    }
}