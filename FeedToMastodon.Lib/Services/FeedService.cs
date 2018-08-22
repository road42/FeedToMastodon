/*
    Copyright (c) 2018 Christoph Jahn.

    This file is licensed to you under the MIT license.
    See the LICENSE.txt file in the project root for more information.
*/

using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using FeedToMastodon.Lib.Interfaces;
using FeedToMastodon.Lib.Models;
using FeedToMastodon.Lib.Models.Configuration;
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
        private readonly IAppConfiguration cfg;
        private readonly IInstanceService instance;
        private readonly ICacheService cache;

        public FeedService(IAppConfiguration configuration, IInstanceService instanceService, ICacheService cacheService)
        {
            this.cfg = configuration;
            this.instance = instanceService;
            this.cache = cacheService;
        }

        public async Task<bool> Run(bool populateCacheOnly = false)
        {
            if (!cfg.FullInstanceRegistrationCompleted)
                return false;

            // Work with every feed in source
            foreach (var feed in cfg.Application?.Feeds)
            {
                // Other types prepared :-)
                switch (feed.Type)
                {
                    // RSS Feed
                    case FeedType.RSS:
                        await HandleRssFeed(feed, populateCacheOnly);
                        break;

                    // Atom Feed
                    case FeedType.Atom:
                        await HandleAtomFeed(feed, populateCacheOnly);
                        break;

                    // Anything else is ignored
                    default:
                        break;
                }
            }

            return true;
        }

        private async Task<bool> HandleRssFeed(Feed feed, bool populateCacheOnly = false)
        {
            var feedTemplate = feed?.Toot?.Template;
            var feedVisbility = feed?.Toot?.Visibiliy;

            try
            {
                using (XmlReader xmlReader = XmlReader.Create(feed.ConnectionString, new XmlReaderSettings() { Async = true }))
                {
                    var reader = new RssFeedReader(xmlReader);

                    while (await reader.Read())
                    {
                        if (reader.ElementType == SyndicationElementType.Item)
                        {
                            ISyndicationItem entry = await reader.ReadItem();

                            if (!await cache.IsCached(entry.Id))
                            {
                                var feedEntry = new FeedEntry()
                                {
                                    FeedTitle = feed.Name,
                                    Id = entry.Id,
                                    Title = entry.Title,
                                    Summary = string.Empty,
                                    Description = entry.Description,
                                    LastUpdated = entry.LastUpdated,
                                    Published = entry.Published,
                                    Link = entry?.Links?.First(r => r.RelationshipType == "alternate")?.Uri.ToString()
                                };

                                if (entry.Categories != null)
                                {
                                    foreach (var category in entry.Categories)
                                    {
                                        feedEntry.Tags.Add(category.Name);
                                    }
                                }

                                if (!populateCacheOnly)
                                {
                                    var tootResult = await instance.TootFeedEntry(feedEntry, feedTemplate, feedVisbility);

                                    if (!tootResult)
                                        return false;
                                }

                                // Cache the entry
                                await cache.Cache(feed.Name, entry.Id, entry.Published.DateTime);
                            }
                        }
                    }
                }

                return true;
            }
            catch
            {

            }

            return false;
        }

        private async Task<bool> HandleAtomFeed(Feed feed, bool populateCacheOnly = false)
        {
            var feedTemplate = feed?.Toot?.Template;
            var feedVisbility = feed?.Toot?.Visibiliy;

            try
            {
                using (XmlReader xmlReader = XmlReader.Create(feed.ConnectionString, new XmlReaderSettings() { Async = true }))
                {
                    var reader = new AtomFeedReader(xmlReader);

                    while (await reader.Read())
                    {
                        if (reader.ElementType == SyndicationElementType.Item)
                        {
                            IAtomEntry entry = await reader.ReadEntry();

                            if (!await cache.IsCached(entry.Id))
                            {

                                var feedEntry = new FeedEntry()
                                {
                                    FeedTitle = feed.Name,
                                    Id = entry.Id,
                                    Title = entry.Title,
                                    Summary = entry.Summary,
                                    Description = entry.Description,
                                    LastUpdated = entry.LastUpdated,
                                    Published = entry.Published,
                                    Link = entry?.Links?.First()?.Uri.ToString()
                                };

                                if (entry.Categories != null)
                                {
                                    foreach (var category in entry.Categories)
                                    {
                                        feedEntry.Tags.Add(category.Name);
                                    }
                                }


                                if (!populateCacheOnly)
                                {
                                    var tootResult = await instance.TootFeedEntry(feedEntry, feedTemplate, feedVisbility);

                                    if (!tootResult)
                                        return false;
                                }

                                // Cache the entry
                                await cache.Cache(feed.Name, entry.Id, entry.Published.DateTime);
                            }
                        }
                    }
                }

                return true;
            }
            catch
            {

            }

            return false;
        }
    }
}