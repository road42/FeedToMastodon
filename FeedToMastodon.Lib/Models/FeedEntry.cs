/*
    Copyright (c) 2018 Christoph Jahn.

    This file is licensed to you under the MIT license.
    See the LICENSE file in the project root for more information.
*/

using System;
using System.Collections.Generic;

namespace FeedToMastodon.Lib.Models
{
    public class FeedEntry
    {
        public string FeedTitle { get; set; }
        public string Id { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public DateTimeOffset LastUpdated { get; set; }
        public DateTimeOffset Published { get; set; }
        public string Description { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public string Link { get; set; }
    }
}