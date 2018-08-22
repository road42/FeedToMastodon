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

        public FeedService(IAppConfiguration configuration, IInstanceService instanceService)
        {
            this.cfg = configuration;
            this.instance = instanceService;
        }

        public async Task<bool> Run()
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
                        if (!await HandleRssFeed(feed))
                            return false;
                        break;

                    // Atom Feed
                    case FeedType.Atom:
                        if (!await HandleAtomFeed(feed))
                            return false;
                        break;

                    // Anything else is ignored
                    default:
                        break;
                }
            }

            return true;
        }

        private async Task<bool> HandleRssFeed(Feed feed)
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

                            var feedEntry = new FeedEntry() {
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

                            var tootResult = await instance.TootFeedEntry(feedEntry, feedTemplate, feedVisbility);

                            if (!tootResult)
                                return false;
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

        private async Task<bool> HandleAtomFeed(Feed feed)
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

                            var feedEntry = new FeedEntry() {
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

                            var tootResult = await instance.TootFeedEntry(feedEntry, feedTemplate, feedVisbility);

                            if (!tootResult)
                                return false;
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