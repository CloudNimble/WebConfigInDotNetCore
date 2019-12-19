using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace CloudNimble.Extensions.Configuration.Tests.ConfigFile
{
    [TestClass]
    public class SimpleWebConfigTests
    {

        [TestMethod]
        public void RunTests()
        {
            var builder = new ConfigurationBuilder()
                 .AddConfigFile("Web.Simple.config");
            var configuration = builder.Build();

            configuration.GetAppSetting("PreserveLoginUrl").Should().Be("true");
            configuration.GetAppSetting("ClientValidationEnabled").Should().Be("true");
            configuration.GetAppSetting("UnobtrusiveJavaScriptEnabled").Should().Be("false");

            var connectionStrings = configuration.GetSection("ConnectionStrings").GetChildren();
            connectionStrings.Should().NotBeNullOrEmpty();
            connectionStrings.First().Key.Should().Be("DefaultConnection");
            connectionStrings.First().Value.Should().Contain("-MvcMovie-");
            connectionStrings.Last().Key.Should().Be("MovieDBContext");
            connectionStrings.Last().Value.Should().Contain("Movies.mdf");

            configuration.GetValue("sampleSection", "setting2").Should().Be("This is the setting2 value");

            var nodes = configuration.GetSection("configNode", "nestedNode");
            nodes.Should().HaveCount(2);

            nodes.First().Key.Should().Be("NestedKey");
            nodes.Last().Key.Should().Be("NestedKey2");
        }

    }

}
