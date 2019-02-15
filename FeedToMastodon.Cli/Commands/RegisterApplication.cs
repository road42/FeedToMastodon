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
        "register",
        Description = "Registers the application with a mastodon instance",
        ExtendedHelpText = Constants.EXTENDEDHELPTEXT,
        ThrowOnUnexpectedArgument = false
    )]
    [VersionOption(Constants.VERSION)]
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

        private readonly ILogger<RegisterApplication> log;

        #endregion

        // Save the services
        private IAppConfiguration cfg;
        private IInstanceService instanceService;
        private IConsole console;

        // The required services are injected. Registration is in Program.cs
        public RegisterApplication(ILogger<RegisterApplication> logger, IAppConfiguration configuration, IInstanceService instanceService, IConsole console)
        {
            this.log = logger;
            this.cfg = configuration;
            this.instanceService = instanceService;
            this.console = console;
        }

        // If the options are set and the command should run.
        private async Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            using (log.BeginScope($"{ nameof(RegisterApplication) }->{ nameof(OnExecuteAsync) }"))
            {
                log.LogDebug("CommandLineParameters: i:\"{instanceName}\" n:\"{appName}\" s:\"{appSite}\"", instanceName, appName, appSite);

                // No configuration needed
                if (cfg.FullInstanceRegistrationCompleted)
                {
                    log.LogDebug("Application is already configured");
                    console.WriteLine("Configuration has already ClientCredentials and AccessToken.");
                    return 0;
                }

                // If no instance is registered we need one by parameter
                if (!cfg.InstanceSaved &&
                    string.IsNullOrWhiteSpace(instanceName))
                {
                    log.LogDebug("No Instance configured and none given by parameter.");
                    console.Error.WriteLine("No Instance configured and none given by parameter.");
                    return 1;
                }

                // Check if we need to register
                if (!cfg.ClientCredentialsSaved)
                {
                    log.LogDebug("Empty client credentials.");
                    console.WriteLine("Empty client credentials. Need to register app.");

                    if (await instanceService.RegisterApplication(instanceName, appName, appSite))
                    {
                        console.WriteLine($"Application on instance {instanceName} registered.");
                    }
                    else
                    {
                        console.Error.WriteLine($"Error while registering appliction on instance {instanceName}.");
                        return 1;
                    }
                }

                if (!cfg.FullInstanceRegistrationCompleted)
                {
                    // Is registered check if we need to get a refresh token
                    log.LogDebug("AccessToken missing. Getting one.");
                    console.WriteLine("Need to get an initial accessToken. Your credentials are used to get one (not saved). Enshure two-factor authentication is disabled during registration.");

                    var email = Prompt.GetString("   - Enter your account-email here:",
                                promptColor: ConsoleColor.DarkGreen);

                    var password = Prompt.GetPassword("   - What is your password:",
                                promptColor: ConsoleColor.Blue);

                    // Are two values entered?
                    if (string.IsNullOrWhiteSpace(email) ||
                        string.IsNullOrWhiteSpace(password))
                    {
                        log.LogError($"Error: You need to enter your account-email and password.");
                        return 1;
                    }

                    // Get Token
                    var tokenResult = await instanceService.FetchAccessToken(email, password);

                    if (tokenResult)
                    {
                        log.LogDebug($"Success: AccessToken fetched. Registration complete.");
                    }
                    else
                    {
                        log.LogError($"Error: No AccessToken fetched. Enter correct credentials and enshure two-factor authentication is disabled during registration.");
                        return 1;
                    }
                }

                return 0;
            }
        }
    }
}
