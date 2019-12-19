// Copyright (c) .NET Foundation & CloudNimble, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Microsoft.Extensions.Configuration.ConfigFile
{
    /// <summary>
    /// ConfigurationProvider for *.config files.  Only elements that contain
    /// &lt;add KeyName=&quot;A&quot; ValueName=&quot;B&quot;/&gt; or &lt;remove KeyName=&quot;A&quot; ValueName=&quot;B&quot;/&gt; (value is not
    /// considered for a remove action) as their descendents are used.
    /// All others are skipped.
    /// KeyName/ValueName can be configured. Default is &quot;key&quot; and &quot;value&quot;, respectively.
    /// </summary>
    /// <example>
    /// The following configuration file will result in the following key-value
    /// pairs in the dictionary:
    /// @{
    ///     { &quot;nodea:TheKey&quot; : &quot;TheValue&quot; },
    ///     { &quot;nodeb:nested:NestedKey&quot; : &quot;ValueA&quot; },
    ///     { &quot;nodeb:nested:NestedKey2&quot; : &quot;ValueB&quot; },
    /// }
    ///
    /// &lt;configuration&gt;
    ///     &lt;nodea&gt;
    ///         &lt;add key=&quot;TheKey&quot; value=&quot;TheValue&quot; /&gt;
    ///     &lt;/nodea&gt;
    ///     &lt;nodeb&gt;
    ///         &lt;nested&gt;
    ///             &lt;add key=&quot;NestedKey&quot; value=&quot;ValueA&quot; /&gt;
    ///             &lt;add key=&quot;NestedKey2&quot; value=&quot;ValueB&quot; /&gt;
    ///             &lt;remove key=&quot;SomeTestKey&quot; /&gt;
    ///         &lt;/nested&gt;
    ///     &lt;/nodeb&gt;
    /// &lt;/configuration&gt;
    /// 
    /// </example>
    public class KeyValueParser : IConfigurationParser
    {

        #region Private Members

        private readonly ILogger _logger;
        private readonly string _keyName = "key";
        private readonly string _valueName = "value";
        private readonly string[] _unsupportedElementNames = { "configSections", "system.web", "system.webServer", "entityFramework", "runtime" };
        private readonly string[] _supportedActions = Enum.GetNames(typeof(ConfigurationAction)).Select(x => x.ToUpperInvariant()).ToArray();

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public KeyValueParser() : this("key", "value")
        {
        }

        /// <summary>
        /// The key/value attribute names.
        /// </summary>
        public KeyValueParser(string key, string value)
            : this(key, value, null)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="logger"></param>
        public KeyValueParser(string key, string value, ILogger logger)
        {
            _keyName = key;
            _valueName = value;
            _logger = logger;
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public bool CanParseElement(XElement element)
        {
            var hasKeyAttribute = element != null && !_unsupportedElementNames.Contains(element.Name.LocalName) && element.DescendantsAndSelf().Any(x => x.Attribute(_keyName) != null);

            return hasKeyAttribute;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <param name="context"></param>
        /// <param name="results"></param>
        public void ParseElement(XElement element, Stack<string> context, SortedDictionary<string, string> results)
        {
            if (element is null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (results is null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            if (!CanParseElement(element))
            {
                return;
            }

            if (!element.Elements().Any())
            {
                AddToDictionary(element, context, results);
            }

            context.Push(element.Name.ToString());

            foreach (var node in element.Elements())
            {
                var hasSupportedAction = node.DescendantsAndSelf().Any(x => _supportedActions.Contains(x.Name.ToString().ToUpperInvariant()));

                if (!hasSupportedAction)
                {
                    if (_logger != null)
                    {
                        _logger.LogWarning($"Contains an unsupported config element. [{node.ToString()}]");
                    }

                    continue;
                }

                ParseElement(node, context, results);
            }

            context.Pop();
        }

        #region Private Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <param name="context"></param>
        /// <param name="results"></param>
        private void AddToDictionary(XElement element, Stack<string> context, SortedDictionary<string, string> results)
        {
            ConfigurationAction action;

            if (!Enum.TryParse(element.Name.ToString(), true, out action))
            {
                if (_logger != null)
                {
                    _logger.LogInformation($"Element with an unsupported action. [{element.ToString()}]");
                }

                return;
            }

            var key = element.Attribute(_keyName);
            var value = element.Attribute(_valueName);

            if (key == null)
            {
                if (_logger != null)
                {
                    _logger.LogInformation($"[{element.ToString()}] is not supported because it does not have an attribute with {_keyName}");
                }

                return;
            }

            var fullkey = GetKey(context, key.Value);

            switch (action)
            {
                case ConfigurationAction.Add:
                    string valueToAdd = null;

                    if (value == null && _logger != null)
                    {
                        _logger.LogWarning($"Could not parse the value attribute [{_valueName}] from [{element.ToString()}]. Using null as value...");
                    }
                    else
                    {
                        valueToAdd = value?.Value;
                    }

                    if (results.ContainsKey(fullkey))
                    {
                        if (_logger != null)
                        {
                            _logger.LogWarning($"{fullkey} exists. Replacing existing value [{results[fullkey]}] with {valueToAdd}");
                        }

                        results[fullkey] = valueToAdd;
                    }
                    else
                    {
                        results.Add(fullkey, valueToAdd);
                    }
                    break;
                case ConfigurationAction.Remove:
                    results.Remove(fullkey);
                    break;
                default:
                    throw new NotSupportedException($"Unsupported action: [{action}]");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private static string GetKey(Stack<string> context, string name)
        {
            return string.Join(ConfigurationPath.KeyDelimiter, context.Reverse().Concat(new[] { name }));
        }

        #endregion
    }

}