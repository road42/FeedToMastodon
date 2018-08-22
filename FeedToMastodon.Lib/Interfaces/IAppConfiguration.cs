/*
    Copyright (c) 2018 Christoph Jahn.

    This file is licensed to you under the MIT license.
    See the LICENSE file in the project root for more information.
*/

namespace FeedToMastodon.Lib.Interfaces
{
    /*
        Provides the application configration and some methods to write it
    */
    public interface IAppConfiguration
    {
        // Return the configuration
        Models.Configuration.Application GetConfiguration();

        // Saves the config to file
        bool Save();
    }
}