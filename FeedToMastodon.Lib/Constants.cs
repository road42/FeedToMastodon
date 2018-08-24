/*
    Copyright (c) 2018 Christoph Jahn.

    This file is licensed to you under the MIT license.
    See the LICENSE file in the project root for more information.
*/

using System;

namespace FeedToMastodon.Lib
{
    public static class Constants
    {
        public const string PROGRAMNAME = "FeedToMastodon";
        public const string PROGRAMFULLNAME = "FeedToMastodon.Cli";
        public const string EXTENDEDHELPTEXT = "\nSee more information about the software at: https://github.com/road42/FeedToMastodon \n(c) 2018 Christoph Jahn - Licensed under MIT license (See LICENSE)\n";

        public const string APPNAME = "FeedToMastodon";
        public const string APPSITE = "https://github.com/road42/FeedToMastodon";

        public const string DEFAULT_CONFIGCONNECTIONSTRING = "feedtomastodon.cfg.json";
        public const string ENVIRONMENT_CONFIGCONNECTIONSTRING_NAME = "FEEDTOMASTODON_CFG";

        public const int DEFAULT_CACHE_MAXENTRIES = 1000;

        // 30 days
        public static TimeSpan DEFAULT_CACHE_MAXAGE = new TimeSpan(30, 0, 0, 0);
        public const string DEFAULT_CACHE_CONNECTIONSTRING = "cache.db";

        public const string VERSION = "0.2.1";
    }
}
