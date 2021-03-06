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
using Microsoft.Extensions.Logging;

namespace FeedToMastodon.Lib.Services.SqliteCache
{
    /*
        A Sqlite-implementation of the ICacheService
    */
    public class SqliteCacheService : Interfaces.ICacheService
    {
        private readonly ILogger<SqliteCacheService> log;
        private readonly IAppConfiguration cfg;
        private readonly CacheContext db;
        private bool IsInitialized { get; set; } = false;

        public SqliteCacheService(ILogger<SqliteCacheService> logger, IAppConfiguration configuration)
        {
            this.log = logger;
            this.cfg = configuration;

            using (log.BeginScope($"{ nameof(SqliteCacheService) }->{ nameof(SqliteCacheService) }"))
            {
                var cacheConnectionString = cfg.Application.Cache.ConnectionString?.Trim() ??
                        Constants.DEFAULT_CACHE_CONNECTIONSTRING;

                log.LogDebug("ConnectionString: {ConnectionString}", cacheConnectionString);

                try
                {
                    var builder = new DbContextOptionsBuilder<CacheContext>();
                    builder.UseSqlite($"Data Source={cacheConnectionString}");
                    this.db = new CacheContext(builder.Options);
                    this.db.Database.EnsureCreated();

                    IsInitialized = true;
                }
                catch (Exception ex)
                {
                    log.LogError(ex, $"{ nameof(SqliteCacheService) } - Exception");
                }
            }
        }

        public async Task<bool> CleanupEntries()
        {
            using (log.BeginScope($"{ nameof(SqliteCacheService) }->{ nameof(CleanupEntries) }"))
            {
                try
                {
                    // Cleanup outdated entries
                    var agedDate = DateTime.Now - cfg.Application.Cache.MaxAge;
                    var agedEntries = db.CachedItems.Where(i => i.Published < agedDate);
                    db.CachedItems.RemoveRange(agedEntries);
                    log.LogInformation($"Cleanup AgedEntries: {agedEntries.Count()} items removed");
                    await db.SaveChangesAsync();

                    // Cleanup MaxEntries
                    var entriesExceededMaxEntries = db
                            .CachedItems
                            .OrderByDescending(i => i.Published)
                            .Skip(cfg.Application.Cache.MaxEntries);
                    db.RemoveRange(entriesExceededMaxEntries);
                    log.LogInformation($"Cleanup MaxEntries: {entriesExceededMaxEntries.Count()} items removed");
                    await db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "CleanupEntries");
                    return false;
                }

                return true;
            }
        }

        public async Task<bool> Cache(string source, string id, DateTime posted)
        {
            using (log.BeginScope($"{ nameof(SqliteCacheService) }->{ nameof(Cache) }"))
            {
                log.LogDebug("Caching id: {id}", id);

                if (!IsInitialized)
                {
                    log.LogDebug("Not IsInitialized");
                    return false;
                }

                try
                {
                    var ci = new CachedItem()
                    {
                        Id = id,
                        Published = DateTime.Now
                    };

                    db.CachedItems.Add(ci);
                    var result = await db.SaveChangesAsync();
                    log.LogDebug($"Cache savereult: {result}");
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "Cannot cache entry {id}.");
                    return false;
                }

                return true;
            }
        }

        public async Task<bool> IsCached(string id)
        {
            using (log.BeginScope($"{ nameof(SqliteCacheService) }->{ nameof(IsCached) }"))
            {
                log.LogDebug("Checking cache for id: {id}", id);

                if (!IsInitialized)
                    return false;

                var result = db.CachedItems.Where(c => c.Id == id);

                if ((await result.CountAsync()) > 0)
                {
                    log.LogDebug("Found");
                    return true;
                }

                log.LogDebug("Not found");
                return false;
            }
        }
    }
}