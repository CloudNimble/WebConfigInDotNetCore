// Copyright (c) CloudNimble, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
    public class ExpandoObjectParser : IConfigurationParser
    {

        #region Private Members

        private readonly ILogger _logger;
        private readonly string[] _unsupportedElementNames = { "configSections", "appSettings", "system.web", "system.webServer", "entityFramework", "runtime" };

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public ExpandoObjectParser()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        public ExpandoObjectParser(ILogger logger)
        {
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
            var hasKeyAttribute = element != null && !_unsupportedElementNames.Contains(element.Name.LocalName);

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
                var hasSupportedAction = node.DescendantsAndSelf().Any(x => !_unsupportedElementNames.Contains(x.Name.LocalName));

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

            var fullkey = GetKey(context, element.Name.LocalName);
            results.Add(fullkey, JsonConvert.SerializeXNode(element));
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