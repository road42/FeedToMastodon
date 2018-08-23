/*
    Copyright (c) 2018 Christoph Jahn.

    This file is licensed to you under the MIT license.
    See the LICENSE file in the project root for more information.
*/

using System.Collections.Generic;

namespace FeedToMastodon.Lib.Models.Configuration
{
    public class Application
    {
        public Instance Instance { get; set; } = new Instance();

        public Cache Cache { get; set; } = new Cache();

        public Toot Toot { get; set; } = new Toot();

        public List<Feed> Feeds { get; set; } = new List<Feed>();
    }
}