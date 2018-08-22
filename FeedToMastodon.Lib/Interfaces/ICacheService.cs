/*
    Copyright (c) 2018 Christoph Jahn.

    This file is licensed to you under the MIT license.
    See the LICENSE file in the project root for more information.
*/

using System;
using System.Threading.Tasks;

namespace FeedToMastodon.Lib.Interfaces
{
    /*
        Provides methods to prevent duplicate posts
     */
    public interface ICacheService
    {
        Task<bool> IsCached(string id);

        Task<bool> Cache(string source, string id, DateTime posted);
    }
}