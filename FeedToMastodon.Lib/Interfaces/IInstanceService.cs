/*
    Copyright (c) 2018 Christoph Jahn.

    This file is licensed to you under the MIT license.
    See the LICENSE file in the project root for more information.
*/

namespace FeedToMastodon.Lib.Interfaces
{
    /*
        Provides the connection to a mastodon-instance and
        methods to work with it.
     */
    public interface IInstanceService
    {
        bool RegisterApplication(string instance, string appName, string appSite);
    }
}