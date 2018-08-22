/*
    Copyright (c) 2018 Christoph Jahn.

    This file is licensed to you under the MIT license.
    See the LICENSE.txt file in the project root for more information.
*/

namespace FeedToMastodon.Lib.Models.Configuration
{
    /*
        The registration of an instance creates this part of the
        configuration.
    */

    public class Instance
    {
        // the url of the mastodon instance. e.g. https://road42.social
        public string Name { get; set; } = string.Empty;

        // ClientId and ClientSecret are set after registration of the instance
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;

        // The refresh token is returned after login with Username, Password, ClientId and ClientSecret
        public string RefreshToken { get; set; } = string.Empty;
    }
}