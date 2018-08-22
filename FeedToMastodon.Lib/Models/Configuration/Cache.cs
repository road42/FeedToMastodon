/*
    Copyright (c) 2018 Christoph Jahn.

    This file is licensed to you under the MIT license.
    See the LICENSE file in the project root for more information.
*/


using System;

namespace FeedToMastodon.Lib.Models.Configuration
{
    public class Cache
    {
        public string ConnectionString { get; set; } = string.Empty;

        public int MaxEntries { get; set; } = 1000;

        public TimeSpan MaxAge { get; set; } = new TimeSpan(30, 0, 0, 0);
    }
}