// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.Configuration.ConfigFile
{

    /// <summary>
    /// 
    /// </summary>
    public class ConfigFileConfigurationSource : IConfigurationSource
    {

        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public ILogger Logger { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Configuration { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool LoadFromFile { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool Optional { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<IConfigurationParser> Parsers { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="loadFromFile"></param>
        /// <param name="optional"></param>
        /// <param name="parsers"></param>
        public ConfigFileConfigurationSource(string configuration, bool loadFromFile, bool optional, params IConfigurationParser[] parsers)
            : this(configuration, loadFromFile, optional, null, parsers)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="loadFromFile"></param>
        /// <param name="optional"></param>
        /// <param name="logger"></param>
        /// <param name="parsers"></param>
        public ConfigFileConfigurationSource(string configuration, bool loadFromFile, bool optional, ILogger logger, params IConfigurationParser[] parsers)
        {
            LoadFromFile = loadFromFile;
            Configuration = configuration;
            Optional = optional;
            Logger = logger;

            var parsersToUse = new List<IConfigurationParser> {
                new KeyValueParser(),
                new KeyValueParser("name", "connectionString"),
                new ExpandoObjectParser(),
            };

            parsersToUse.AddRange(parsers);

            Parsers = parsersToUse.ToArray();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new ConfigFileConfigurationProvider(Configuration, LoadFromFile, Optional, Logger, Parsers);
        }

        #endregion

    }

}