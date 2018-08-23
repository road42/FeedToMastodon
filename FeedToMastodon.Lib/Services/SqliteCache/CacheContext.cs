/*
    Copyright (c) 2018 Christoph Jahn.

    This file is licensed to you under the MIT license.
    See the LICENSE file in the project root for more information.
*/

using System;
using Microsoft.EntityFrameworkCore;

namespace FeedToMastodon.Lib.Services.SqliteCache
{
    /*
        A Sqlite-implementation of the ICacheService
    */
    public class CacheContext : DbContext
    {
        public DbSet<CachedItem> CachedItems { get; set; }

        public CacheContext(DbContextOptions<CacheContext> options) : base(options) { }

        public CacheContext(DbContextOptions options) : base(options) { }

        // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        // {
        //     optionsBuilder.UseSqlite("Data Source=cache.db");
        // }
    }
}