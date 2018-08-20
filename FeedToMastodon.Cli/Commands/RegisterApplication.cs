/*
    Copyright (c) 2018 Christoph Jahn.

    This file is licensed to you under the MIT license.
    See the LICENSE.txt file in the project root for more information.
*/

using System;
using System.ComponentModel.DataAnnotations;

using McMaster.Extensions.CommandLineUtils;

namespace FeedToMastodon.Cli.Commands
{
    [Command(
        "register",
        Name = Program.PROGRAMNAME,
        FullName = Program.PROGRAMFULLNAME,
        Description = "Registers the application with a mastodon instance",
        ExtendedHelpText = Program.EXTENDEDHELPTEXT
    )]
    public class RegisterApplication
    {

        [Required(ErrorMessage = "You must specify an instance name")]
        [Option("--instance", Description = "The name of the mastodon instance (without https://)")]
        private string instanceName { get; set; }

        [Option("--appName", Description = "The name used to register this application (optional)")]
        private string appName { get; set; } = Program.APPNAME;


        [Option("--appSite", Description = "The website of the application (optional)")]
        private string appSite { get; set; } = Program.APPSITE;

        private void OnExecute(IConsole console)
        {
            Console.WriteLine($"i: {instanceName} n: {appName} s: {appSite}");
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
