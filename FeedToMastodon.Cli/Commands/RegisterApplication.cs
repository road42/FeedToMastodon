/*
    Copyright (c) 2018 Christoph Jahn.

    This file is licensed to you under the MIT license.
    See the LICENSE file in the project root for more information.
*/

using System;
using System.ComponentModel.DataAnnotations;
using FeedToMastodon.Lib;
using FeedToMastodon.Lib.Interfaces;
using McMaster.Extensions.CommandLineUtils;

namespace FeedToMastodon.Cli.Commands
{
    /*
        This command is needed first. You have to register the
        application with the instance.

        If you want to run multiple versions of the application you
        have to choose an individual name for each one.
    */
    [Command(
        "register",
        Name = Constants.PROGRAMNAME,
        FullName = Constants.PROGRAMFULLNAME,
        Description = "Registers the application with a mastodon instance",
        ExtendedHelpText = Constants.EXTENDEDHELPTEXT,
        ThrowOnUnexpectedArgument = false
    )]
    public class RegisterApplication
    {
        #region "Options"

        // The instanceName: we create the Uri with it.
        [Required(ErrorMessage = "You must specify an instance name")]
        [Option(Description = "The name of the mastodon instance (without https://)", LongName = "instance", ShortName = "i", ValueName = "INSTANCE")]
        private string instanceName { get; set; }

        // Choose an individual name for the application if you want to
        // use multiple instances.
        [Option(Description = "The name used to register this application (optional)", LongName = "appName", ShortName = "n", ValueName = "NAME")]
        private string appName { get; set; } = Constants.APPNAME;

        // You may set this in mastodon. Not really needed here.
        [Option(Description = "The website of the application (optional)", LongName = "appSite", ShortName = "s", ValueName = "SITE")]
        private string appSite { get; set; } = Constants.APPSITE;

        #endregion

        // Save the services
        private IAppConfiguration configuration;
        private IInstanceService instanceService;
        private IConsole console;

        // The required services are injected. Registration is in Program.cs
        public RegisterApplication(IAppConfiguration configuration, IInstanceService instanceService, IConsole console)
        {
            this.configuration = configuration;
            this.instanceService = instanceService;
            this.console = console;
        }

        // If the options are set and the command should run.
        private int OnExecute(CommandLineApplication app, IConsole console)
        {
            if (instanceService.RegisterApplication(instanceName, appName, appSite))
            {
                console.WriteLine("Application registered");
                return 0;
            }

            console.WriteLine("Application NOT registered.");
            return 1;
        }
    }
}
