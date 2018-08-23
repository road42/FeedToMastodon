/*
    Copyright (c) 2018 Christoph Jahn.

    This file is licensed to you under the MIT license.
    See the LICENSE file in the project root for more information.
*/

using System;

namespace FeedToMastodon.Lib.Services.SqliteCache
{
    /*
        A cached object
    */
    public class CachedItem
    {
        public string Id { get; set; }

        public DateTime Published { get; set; }
    }
}