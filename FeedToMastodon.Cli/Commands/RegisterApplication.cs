/*
    Copyright (c) 2018 Christoph Jahn.

    This file is licensed to you under the MIT license.
    See the LICENSE.txt file in the project root for more information.
*/

using System;
using System.ComponentModel.DataAnnotations;
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
        Name = Program.PROGRAMNAME,
        FullName = Program.PROGRAMFULLNAME,
        Description = "Registers the application with a mastodon instance",
        ExtendedHelpText = Program.EXTENDEDHELPTEXT,
        ThrowOnUnexpectedArgument = false
    )]
    public class RegisterApplication
    {
#region "Options"

        // The instanceName: we create the Uri with it.
        [Required(ErrorMessage = "You must specify an instance name")]
        [Option("--instance", Description = "The name of the mastodon instance (without https://)")]
        private string instanceName { get; set; }

        // Choose an individual name for the application if you want to
        // use multiple instances.
        [Option("--appName", Description = "The name used to register this application (optional)")]
        private string appName { get; set; } = Program.APPNAME;

        // You may set this in mastodon. Not really needed here.
        [Option("--appSite", Description = "The website of the application (optional)")]
        private string appSite { get; set; } = Program.APPSITE;

#endregion

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

        // If the Options are set and the command should run.
        private void OnExecute()
        {
            console.WriteLine($"i: {instanceName} n: {appName} s: {appSite}");
            /*
                    TODO: Put to library
                    TODO: Add config file option to write data to

                    Example source for registering:

                    using (var appHandler = new AppHandler(InstanceName))
                    {
                        var scopes = AppScopeEnum.Write;
                        var appData = appHandler.CreateAppAsync(ClientName, scopes, Website).Result;
                    }
             */
        }
    }
}
