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
        private async Task<int> OnExecuteAsync(CommandLineApplication app, IConsole console)
        {
            var instanceConfig = this.configuration.GetConfiguration().Instance;
            var registrationResult = true;

            // If no instance is registered we need one by parameter
            if (string.IsNullOrWhiteSpace(instanceConfig.Name) &&
                string.IsNullOrWhiteSpace(instanceName))
            {
                app.ShowHelp();
                await console.Error.WriteLineAsync("\n- No Instance configured and none given by parameter. Please enter one.");
                return 1;
            }

            // Check if we need to register
            if (string.IsNullOrWhiteSpace(instanceConfig.Name) ||
                string.IsNullOrWhiteSpace(instanceConfig.ClientId) ||
                string.IsNullOrWhiteSpace(instanceConfig.ClientSecret))
            {
                console.WriteLine("- Empty client credentials. Need to register app.");

                registrationResult = await instanceService.RegisterApplication(instanceName, appName, appSite);

                if (registrationResult)
                {
                    console.WriteLine("   - Application registered");
                }
                else
                {
                    await console.Error.WriteLineAsync("   - Application NOT registered.");
                    return 1;
                }
            }

            // No configuration neededs
            if (registrationResult && !string.IsNullOrWhiteSpace(instanceConfig.AccessToken))
                Console.WriteLine("- Configuration has already clientCredentials and refreshToken.");

            // Is registered check if we need to get a refresh token
            console.WriteLine("- Need to get an initial refreshToken. Your credentials are used to get one (not saved).\n");

            var email = Prompt.GetString("   - Enter your account-email here:",
                        promptColor: ConsoleColor.DarkGreen);

            var password = Prompt.GetPassword("   - What is your password:",
                        promptColor: ConsoleColor.Blue);

            // Are two values entered?
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                await console.Error.WriteLineAsync("   - You need to enter your account-email and password once.");
                return 1;
            }

            // Get Token
            var tokenResult = await instanceService.FetchAccessToken(email, password);

            if (tokenResult)
                console.WriteLine("   - RefreshToken fetched");
            else
                await console.Error.WriteLineAsync("   - NO refreshToken fetched.");

            return 0;
        }
    }
}
