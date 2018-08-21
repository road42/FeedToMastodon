/*
    Copyright (c) 2018 Christoph Jahn.

    This file is licensed to you under the MIT license.
    See the LICENSE.txt file in the project root for more information.
*/

namespace FeedToMastodon.Lib.Interfaces
{
    /*
        Provides the application configration and some methods to write it
    */
    public interface IAppConfiguration
    {
        // Check if connectionString is valid
        bool IsValidConnectionString(string connectionString);

        // Create config store or read existing one (may be filename, may be sqllite or environment ...)
        bool InitializeFromConnectionString(string connectionString);

        // Return the configuration
        Models.Configuration.Application GetConfiguration();
    }
}