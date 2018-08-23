/*
    Copyright (c) 2018 Christoph Jahn.

    This file is licensed to you under the MIT license.
    See the LICENSE file in the project root for more information.
*/

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using FeedToMastodon.Lib;
using FeedToMastodon.Lib.Interfaces;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace FeedToMastodon.Cli.Commands
{
    /*
        This command is needed first. You have to register the
        application with the instance.

        If you want to run multiple versions of the application you
        have to choose an individual name for each one.
    */
    [Command(
        "run",
        Name = Constants.PROGRAMNAME,
        FullName = Constants.PROGRAMFULLNAME,
        Description = "Fetches all feeds an \"toot\"s them.",
        ExtendedHelpText = Constants.EXTENDEDHELPTEXT,
        ThrowOnUnexpectedArgument = false
    )]
    [VersionOption(Constants.VERSION)]
    public class Run
    {
        // Don't use mastodon prefill cache only
        [Option(Description = "Prefill cache only. Don't toot to mastodon", LongName = "populateCacheOnly", ShortName = "p")]
        private bool populateCacheOnly { get; set; } = false;

        private readonly ILogger<Run> log;

        // Save the services
        private IAppConfiguration cfg;
        private IConsole console;
        private IFeedService feed;

        // The required services are injected. Registration is in Program.cs
        public Run(ILogger<Run> logger, IFeedService feed, IAppConfiguration configuration, IConsole console)
        {
            this.log = logger;
            this.cfg = configuration;
            this.console = console;
            this.feed = feed;
        }

        // If the options are set and the command should run.
        private async Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            using (log.BeginScope($"{ nameof(Run) }->{ nameof(OnExecuteAsync) }"))
            {
                log.LogDebug("CommandLineParameters: p:\"{populateCacheOnly}\"", populateCacheOnly);

                // No configuration needed
                if (!cfg.FullInstanceRegistrationCompleted)
                {
                    log.LogError("No instance-registration found.");
                    app.ShowHelp();
                    console.Error.WriteLine("Not configured yet");
                    return 1;
                }

                var feedResult = await feed.Run(populateCacheOnly);

                return feedResult ? 0 : 1;
            }
        }
    }
}
