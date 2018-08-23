/*
    Copyright (c) 2018 Christoph Jahn.

    This file is licensed to you under the MIT license.
    See the LICENSE.txt file in the project root for more information.
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
                    // RSS2.0 or Atom Feed
                    case FeedType.RSS:
                    case FeedType.Atom:
                        return await HandleXmlFeed(feed, populateCacheOnly);

                    // Anything else is ignored
                    default:
                        break;
                }
            }

            return true;
        }

        //TODO: Combine Rss and Atmom feed handling. (Too much copies)
        private async Task<bool> HandleXmlFeed(Feed feed, bool populateCacheOnly = false)
        {
            try
            {
                using (XmlReader xmlReader = XmlReader.Create(feed.ConnectionString, new XmlReaderSettings() { Async = true }))
                {
                    ISyndicationFeedReader reader = null;

                    switch (feed.Type)
                    {
                        case FeedType.RSS:
                            reader = new RssFeedReader(xmlReader);
                            break;
                        case FeedType.Atom:
                            reader = new AtomFeedReader(xmlReader);
                            break;
                        default:
                            return false;
                    }

                    while (await reader.Read())
                    {
                        if (reader.ElementType == SyndicationElementType.Item)
                        {
                            FeedEntry feedEntry = null;

                            switch (feed.Type)
                            {
                                case FeedType.RSS:
                                    ISyndicationItem rssEntry = await (reader as RssFeedReader).ReadItem();
                                    feedEntry = CreateFeedEntry(rssEntry, feed);
                                    break;
                                case FeedType.Atom:
                                    IAtomEntry atomEntry = await (reader as AtomFeedReader).ReadEntry();
                                    feedEntry = CreateFeedEntry(atomEntry, feed);
                                    break;
                                default:
                                    continue;
                            }

                            if (!(await cache.IsCached(feedEntry.Id)))
                                return await TootTheFeedEntry(feedEntry, populateCacheOnly, feed);
                        }
                    }
                }
            }
            catch
            {

            }

            return false;
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

        private string GetLinkFromFeedEntry(IEnumerable<ISyndicationLink> links)
        {
            var link = links?.First(r => r.RelationshipType == "alternate")?.Uri.ToString();

            if (string.IsNullOrWhiteSpace(link))
                link = links?.First()?.Uri.ToString();

            if (string.IsNullOrWhiteSpace(link))
                return null;

            return link;
        }

        private string CreateIdFromFeedEntry(FeedEntry entry)
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

            return id.ToString();
        }
    }
}