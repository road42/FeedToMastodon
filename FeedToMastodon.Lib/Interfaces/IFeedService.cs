/*
    Copyright (c) 2018 Christoph Jahn.

    This file is licensed to you under the MIT license.
    See the LICENSE.txt file in the project root for more information.
*/

using System.Threading.Tasks;

namespace FeedToMastodon.Lib.Interfaces
{
    /*
        Provides the connection to all feeds and
        methods to work with them.
     */
    public interface IFeedService
    {
        Task<bool> Run(bool populateCacheOnly = false);
    }
}