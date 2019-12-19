// Copyright (c) .NET Foundation & CloudNimble, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Immutable;
using System.Dynamic;
using System.Linq;

namespace Microsoft.Extensions.Configuration
{

    /// <summary>
    /// 
    /// </summary>
    public static class ConfigurationExtensions
    {

        #region Private Members

        private const string AppSettings = "appSettings";

        #endregion

        #region Public Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetAppSetting(this IConfiguration configuration, string name)
        {
            return configuration?.GetSection(AppSettings)[name];
        }

        /// <summary>
        /// Gets all of the configuration sections for a set of keys
        /// </summary>
        /// <param name="sectionNames">The names of the sections from the top-most level to lowest</param>
        public static ImmutableDictionary<string, IConfigurationSection> GetSection(this IConfiguration configuration, params string[] sectionNames)
        {
            if (sectionNames.Length == 0)
                return ImmutableDictionary<string, IConfigurationSection>.Empty;

            var fullKey = string.Join(ConfigurationPath.KeyDelimiter, sectionNames);

            return configuration?.GetSection(fullKey).GetChildren()?.ToImmutableDictionary(x => x.Key, x => x);
        }

        /// <summary>
        /// Given a set of keys, will join them and get the value at that level
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="keys">Names of the sections from top-level to lowest level</param>
        /// <returns>The value of that key</returns>
        public static string GetValue(this IConfiguration configuration, params string[] keys)
        {
            if (keys.Length == 0)
            {
                throw new ArgumentException("Need to provide keys", nameof(keys));
            }

            var fullKey = string.Join(ConfigurationPath.KeyDelimiter, keys);

            return configuration?[fullKey];
        }

        /// <summary>
        /// Given a set of keys, will join them and get the value at that level
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="keys">Names of the sections from top-level to lowest level</param>
        /// <returns>The value of that key</returns>
        public static dynamic GetValueDynamic(this IConfiguration configuration, params string[] keys)
        {
            if (keys.Length == 0)
            {
                throw new ArgumentException("Need to provide keys", nameof(keys));
            }

            var fullKey = string.Join(ConfigurationPath.KeyDelimiter, keys);
            var dynamicObject = JsonConvert.DeserializeObject<ExpandoObject>(configuration?[fullKey].Replace("\"@", "\""));
            return dynamicObject.First().Value;
        }

        /// <summary>
        /// Given a set of keys, will join them and get the value at that level
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="keys">Names of the sections from top-level to lowest level</param>
        /// <returns>The value of that key</returns>
        public static T GetValue<T>(this IConfiguration configuration, params string[] keys)
        {
            if (keys.Length == 0)
            {
                throw new ArgumentException("Need to provide keys", nameof(keys));
            }

            var fullKey = string.Join(ConfigurationPath.KeyDelimiter, keys);
            var jObject = JObject.Parse(configuration?[fullKey].Replace("\"@", "\""));
            return jObject.First.First.ToObject<T>();
        }

        #endregion

    }

}