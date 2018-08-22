/*
    Copyright (c) 2018 Christoph Jahn.

    This file is licensed to you under the MIT license.
    See the LICENSE file in the project root for more information.
*/

using mastodon.Enums;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FeedToMastodon.Lib.Models.Configuration
{
    public class Toot
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public StatusVisibilityEnum Visibiliy { get; set; } = StatusVisibilityEnum.Public;

        public string Template { get; set; } = string.Empty;
    }
}