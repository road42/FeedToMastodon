/*
    Copyright (c) 2018 Christoph Jahn.

    This file is licensed to you under the MIT license.
    See the LICENSE file in the project root for more information.
*/

using mastodon.Enums;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using System.ComponentModel.DataAnnotations;

namespace FeedToMastodon.Lib.Models.Configuration
{
    public class Feed
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [JsonConverter(typeof(StringEnumConverter))]
        public FeedType Type { get; set; } = FeedType.RSS;

        [Required]
        public string ConnectionString { get; set; } = string.Empty;

        public Toot Toot { get; set; } = null;
    }
}