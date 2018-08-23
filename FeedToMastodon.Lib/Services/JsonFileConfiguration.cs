/*
    Copyright (c) 2018 Christoph Jahn.

    This file is licensed to you under the MIT license.
    See the LICENSE file in the project root for more information.
*/

using System;
using System.Collections;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FeedToMastodon.Lib.Services
{
    /*
        A JSON-implementation of the IAppConfiguration. Uses
        a filename as connectionString.
    */
    public class JsonFileConfiguration : Interfaces.IAppConfiguration
    {
        private Models.Configuration.Application configuration = new Models.Configuration.Application();
        private readonly ILogger<JsonFileConfiguration> log;

        // Returns the connectionString from environment or
        // the default one
        private string ConnectionString => Environment
                    .GetEnvironmentVariable(Constants.ENVIRONMENT_CONFIGCONNECTIONSTRING_NAME) ??
                        Constants.DEFAULT_CONFIGCONNECTIONSTRING;

        private Models.Configuration.Instance instance
        {
            get => this.configuration.Instance;
        }

        // Read an initialize config
        public JsonFileConfiguration(ILogger<JsonFileConfiguration> logger)
        {
            this.log = logger;

            InitializeFromConnectionString();
        }

        // True if everything is set
        public bool FullInstanceRegistrationCompleted
        {
            get
            {
                return (
                    !string.IsNullOrWhiteSpace(instance.Name) &&
                    !string.IsNullOrWhiteSpace(instance.ClientId) &&
                    !string.IsNullOrWhiteSpace(instance.ClientSecret) &&
                    !string.IsNullOrWhiteSpace(instance.AccessToken)
                );
            }
        }

        // True if only instanceName is set
        public bool InstanceSaved
        {
            get
            {
                return (
                    !string.IsNullOrWhiteSpace(instance.Name) &&
                    string.IsNullOrWhiteSpace(instance.ClientId) &&
                    string.IsNullOrWhiteSpace(instance.ClientSecret) &&
                    string.IsNullOrWhiteSpace(instance.AccessToken)
                );
            }
        }

        // True if all but accessToken is set
        public bool ClientCredentialsSaved
        {
            get
            {
                return (
                    !string.IsNullOrWhiteSpace(instance.Name) &&
                    !string.IsNullOrWhiteSpace(instance.ClientId) &&
                    !string.IsNullOrWhiteSpace(instance.ClientSecret) &&
                    string.IsNullOrWhiteSpace(instance.AccessToken)
                );
            }
        }

        public Models.Configuration.Application Application =>
            configuration ?? new Models.Configuration.Application();

        /*
            Initializes configuration and reads the configfile.
            If the configfile does not exist a new one is created.
        */
        private bool InitializeFromConnectionString()
        {
            using (log.BeginScope($"{ nameof(JsonFileConfiguration) }->{ nameof(InitializeFromConnectionString) }"))
            {
                log.LogDebug("ConnectionString: '{ConnectionString}'", ConnectionString);

                // No need to check if empty
                if (string.IsNullOrEmpty(ConnectionString))
                    return false;

                // Create fileInformation object from name
                // the file does not have to exist to do this.
                System.IO.FileInfo fileInfo = null;

                // Try to initialize fileInfo
                try
                {
                    // Create fileInfo ... this will fail on invalid filenames
                    fileInfo = new FileInfo(ConnectionString);
                }
                // catch exceptions so fileInfo stays null
                catch (Exception) { }

                // false if fileInfo is null
                if (fileInfo == null)
                {
                    log.LogError("Invalid configFileName given.");
                    return false;
                }

                // Now initialize and read the file
                try
                {
                    // Configuration file does not exist yet => create it from empty cfg-object
                    if (!fileInfo.Exists)
                    {
                        log.LogDebug("Creating new file");
                        var app = new Models.Configuration.Application();

                        // serialize JSON to a string and then write string to a file
                        File.WriteAllText(ConnectionString, JsonConvert.SerializeObject(app, Formatting.Indented,
                            new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore
                            }));
                    }

                    // Create and configure the ConfigurationBuilder;
                    var builder = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile(fileInfo.FullName, false, true);

                    // Read the file and bind to configuration-object
                    log.LogDebug("Execute configuationBuilder");

                    builder
                        .Build()
                        .Bind(this.configuration);

                }
                catch (Exception ex)
                {
                    log.LogError(ex, "Initialize configFile: '{connectionString}'.", ConnectionString);
                    return false;
                }

                return true;
            }
        }

        // Writes the file to disk
        public bool Save()
        {
            using (log.BeginScope($"{ nameof(JsonFileConfiguration) }->{ nameof(Save) }"))
            {
                try
                {
                    log.LogDebug("Writing configFile: '{ConnectionString}'", ConnectionString);

                    // Just serialize and write
                    File.WriteAllText(ConnectionString, JsonConvert.SerializeObject(
                            this.configuration,
                            Formatting.Indented,
                            new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore
                            }
                        ));
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "Saving configFile: '{ConnectionString}'", ConnectionString);
                    return false;
                }

                return true;
            }
        }
    }
}