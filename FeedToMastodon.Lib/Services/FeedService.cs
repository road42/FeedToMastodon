/*
    Copyright (c) 2018 Christoph Jahn.

    This file is licensed to you under the MIT license.
    See the LICENSE file in the project root for more information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using FeedToMastodon.Lib.Interfaces;
using FeedToMastodon.Lib.Models;
using FeedToMastodon.Lib.Models.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SyndicationFeed;
using Microsoft.SyndicationFeed.Atom;
using Microsoft.SyndicationFeed.Rss;

namespace FeedToMastodon.Lib.Services
{
    /*
        A implementation of the FeedService.
    */
    public class FeedService : Interfaces.IFeedService
    {
        private readonly ILogger<FeedService> log;
        private readonly IAppConfiguration cfg;
        private readonly IInstanceService instance;
        private readonly ICacheService cache;

        public FeedService(ILogger<FeedService> logger, IAppConfiguration configuration, IInstanceService instanceService, ICacheService cacheService)
        {
            this.log = logger;
            this.cfg = configuration;
            this.instance = instanceService;
            this.cache = cacheService;
        }

        // Fetches every feed and tootes the entries
        public async Task<bool> Run(bool populateCacheOnly = false)
        {
            using (log.BeginScope($"{ nameof(FeedService) }->{ nameof(Run) } with populateCacheOnly: {populateCacheOnly}"))
            {
                if (!cfg.FullInstanceRegistrationCompleted)
                {
                    log.LogError("No instance-configuration found");
                    return false;
                }

                // Exit if no feeds found
                if (cfg.Application?.Feeds == null || cfg.Application?.Feeds.Count == 0)
                {
                    log.LogDebug("No feeds found");
                    return false;
                }

                // Work with every feed in source
                foreach (var feed in cfg.Application?.Feeds)
                {
                    // Other types prepared :-)
                    switch (feed.Type)
                    {
                        // RSS2.0 or Atom Feed
                        case FeedType.RSS:
                        case FeedType.Atom:
                            return await HandleXmlFeed(feed, populateCacheOnly);

                        // Anything else is ignored
                        default:
                            break;
                    }
                }
            }

            return true;
        }

        // Handles RSS2.0 and Atom feeds
        private async Task<bool> HandleXmlFeed(Feed feed, bool populateCacheOnly = false)
        {
            using (log.BeginScope($"{ nameof(FeedService) }->{ nameof(HandleXmlFeed) } with feed: {feed}"))
            {
                try
                {
                    // Create async reader
                    using (XmlReader xmlReader = XmlReader.Create(feed.ConnectionString, new XmlReaderSettings() { Async = true }))
                    {
                        ISyndicationFeedReader reader = null;

                        // Special reader for every type
                        switch (feed.Type)
                        {
                            case FeedType.RSS:
                                log.LogDebug("Creating RssFeedReader");
                                reader = new RssFeedReader(xmlReader);
                                break;
                            case FeedType.Atom:
                                log.LogDebug("Creating AtomFeedReader");
                                reader = new AtomFeedReader(xmlReader);
                                break;
                            default:
                                log.LogError("Invalid FeedType found.");
                                return false;
                        }

                        log.LogDebug("Starting feed reading");

                        // Read the entries async
                        while (await reader.Read())
                        {
                            log.LogDebug("Found elementType '{ElementType}'", reader.ElementType);

                            // We only need the entrytypes (yet)
                            if (reader.ElementType == SyndicationElementType.Item)
                            {
                                FeedEntry feedEntry = null;

                                // Create my feedEntry
                                switch (feed.Type)
                                {
                                    case FeedType.RSS:
                                        ISyndicationItem rssEntry = await (reader as RssFeedReader).ReadItem();
                                        log.LogDebug("ISyndicationItem: {Item}", rssEntry);
                                        feedEntry = CreateFeedEntry(rssEntry, feed);
                                        break;
                                    case FeedType.Atom:
                                        IAtomEntry atomEntry = await (reader as AtomFeedReader).ReadEntry();
                                        log.LogDebug("IAtomEntry: {Entry}", atomEntry);
                                        feedEntry = CreateFeedEntry(atomEntry, feed);
                                        break;
                                    default:
                                        continue;
                                }

                                // Check if we already had this one
                                if (await cache.IsCached(feedEntry.Id))
                                {
                                    log.LogDebug("ID {Id} is in cache", feedEntry.Id);
                                }
                                else
                                {
                                    // Hand over to instance service
                                    log.LogDebug("Tooting id: {Id} ", feedEntry.Id);
                                    await TootTheFeedEntry(feedEntry, populateCacheOnly, feed);
                                }
                            }
                        }

                        return true;
                    }
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "HandleXmlFeed Exception - {feed}", feed);
                    return false;
                }
            }
        }

        private async Task<bool> TootTheFeedEntry(FeedEntry entry, bool populateCacheOnly, Feed feed)
        {
            // Toot if wanted
            if (!populateCacheOnly && !(await instance.TootFeedEntry(entry, feed?.Toot?.Template, feed?.Toot?.Visibiliy)))
                return false;

            // Cache the entry
            await cache.Cache(feed.Name, entry.Id, entry.Published.DateTime);

            return true;
        }

        // AtomEntry conversion
        private FeedEntry CreateFeedEntry(IAtomEntry entry, Feed feed)
        {
            var link = GetLinkFromFeedEntry(entry.Links);

            var feedEntry = new FeedEntry()
            {
                FeedTitle = feed.Name,
                Id = entry.Id,
                Title = entry.Title,
                Summary = entry.Summary,
                Description = entry.Description,
                LastUpdated = entry.LastUpdated,
                Published = entry.Published,
                Link = link
            };

            if (feedEntry.Id == null)
                feedEntry.Id = CreateIdFromFeedEntry(feedEntry);

            //TODO: This could be better
            if (entry.Categories != null)
            {
                foreach (var category in entry.Categories)
                {
                    feedEntry.Tags.Add(category.Name);
                }
            }

            return feedEntry;
        }

        // RSS Feed conversion. RSS has no summary
        private FeedEntry CreateFeedEntry(ISyndicationItem entry, Feed feed)
        {
            var link = GetLinkFromFeedEntry(entry.Links);

            var feedEntry = new FeedEntry()
            {
                FeedTitle = feed.Name,
                Id = entry.Id,
                Title = entry.Title,
                Summary = null,
                Description = entry.Description,
                LastUpdated = entry.LastUpdated,
                Published = entry.Published,
                Link = link
            };

            if (feedEntry.Id == null)
                feedEntry.Id = CreateIdFromFeedEntry(feedEntry);

            //TODO: This could be better
            if (entry.Categories != null)
            {
                foreach (var category in entry.Categories)
                {
                    feedEntry.Tags.Add(category.Name);
                }
            }

            return feedEntry;
        }

        // See, if we get a nice link from entry
        private string GetLinkFromFeedEntry(IEnumerable<ISyndicationLink> links)
        {
            var link = links?.First(r => r.RelationshipType == "alternate")?.Uri.ToString();

            if (string.IsNullOrWhiteSpace(link))
                link = links?.First()?.Uri.ToString();

            if (string.IsNullOrWhiteSpace(link))
                return null;

            return link;
        }

        // Sometimes no id is given.
        private string CreateIdFromFeedEntry(FeedEntry entry)
        {

            using (log.BeginScope($"{ nameof(FeedService) }->{ nameof(CreateIdFromFeedEntry) }" + " with entry: {Entry}", entry))
            {
                // Check if there is an id
                if (entry.Id != null)
                    return entry.Id;

                // Create a new Id
                var key = $"{entry.FeedTitle}-{entry.Title}-{entry.Link}";

                var mySHA256 = SHA256Managed.Create();
                var hash = mySHA256.ComputeHash(Encoding.UTF8.GetBytes(key));

                var id = new System.Text.StringBuilder();

                foreach (byte theByte in hash)
                {
                    id.Append(theByte.ToString("x2"));
                }

                log.LogDebug("Created id: {Id} from key: {Key}", id, key);

                return id.ToString();
            }
        }
    }
}