/*
    Copyright (c) 2018 Christoph Jahn.

    This file is licensed to you under the MIT license.
    See the LICENSE file in the project root for more information.
*/

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FeedToMastodon.Lib.Interfaces;

namespace FeedToMastodon.Lib.Services.SqliteCache
{
    /*
        A Sqlite-implementation of the ICacheService
    */
    public class SqliteCacheService : Interfaces.ICacheService
    {
        private readonly IAppConfiguration cfg;
        private readonly CacheContext db;

        public SqliteCacheService(IAppConfiguration configuration)
        {
            this.cfg = configuration;

            var cacheConnectionString = cfg.Application.Cache.ConnectionString?.Trim() ??
                    Constants.DEFAULT_CACHE_CONNECTIONSTRING;

            var builder = new DbContextOptionsBuilder<CacheContext>();
            builder.UseSqlite($"Data Source={cacheConnectionString}");
            this.db = new CacheContext(builder.Options);
            this.db.Database.EnsureCreated();

            // TODO: Cleanup TimeSpan
            // TODO: Cleanup MaxAge

        }

        public async Task<bool> Cache(string source, string id, DateTime posted)
        {
            if (posted == null)
                posted = DateTime.Now;

            try
            {
                var ci = new CachedItem()
                {
                    Id = id,
                    Published = posted
                };

                db.CachedItems.Add(ci);
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine ("Cache: " + ex.Message);
                return false;
            }

            return true;
        }

        public async Task<bool> IsCached(string id)
        {
            var result = db.CachedItems.Where(c => c.Id == id);

            if ((await result.CountAsync()) > 0)
            {
                return true;
            }

            return false;
        }
    }
}