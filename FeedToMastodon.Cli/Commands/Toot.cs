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
        "toot",
        Description = "Toots a status message",
        ExtendedHelpText = Constants.EXTENDEDHELPTEXT,
        ThrowOnUnexpectedArgument = false
    )]
    [VersionOption(Constants.VERSION)]
    public class Toot
    {
        #region "Options"

        // The instanceName: we create the Uri with it.
        [Required(ErrorMessage = "You need to give a status message")]
        [Option(Description = "The status to toot", LongName = "toot", ShortName = "t", ValueName = "STATUS")]
        private string status { get; set; }

        #endregion

        // Save the services
        private readonly ILogger<Toot> log;
        private readonly IAppConfiguration cfg;
        private readonly IInstanceService instanceService;
        private readonly IConsole console;

        // The required services are injected. Registration is in Program.cs
        public Toot(ILogger<Toot> logger, IAppConfiguration configuration, IInstanceService instanceService, IConsole console)
        {
            this.log = logger;
            this.cfg = configuration;
            this.instanceService = instanceService;
            this.console = console;
        }

        // If the options are set and the command should run.
        private async Task<int> OnExecuteAsync(CommandLineApplication app, IConsole console)
        {
            using (log.BeginScope($"{ nameof(Toot) }->{ nameof(OnExecuteAsync) }"))
            {
                log.LogDebug("CommandLineParameters: t:\"{status}\"", status);
                // No configuration needed
                if (!cfg.FullInstanceRegistrationCompleted)
                {
                    log.LogError("No instance-configuration found.");
                    app.ShowHelp();
                    console.Error.WriteLine("Not configured yet");
                    return 1;
                }

                var tootResult = await instanceService.Toot(status);

                return tootResult ? 0 : 1;
            }
        }
    }
}
