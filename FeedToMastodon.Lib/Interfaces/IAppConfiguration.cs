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
        // True if all instanceData is set
        bool FullInstanceRegistrationCompleted { get; }

        // True if only a name is given
        bool InstanceSaved { get; }

        // True if a name, clientId and clientSecret are given
        bool ClientCredentialsSaved { get; }

        // Return the configuration
        Models.Configuration.Application Application { get; }

        // Saves the config to file
        bool Save();
    }
}