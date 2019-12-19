using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Dynamic;
using System.Linq;

namespace CloudNimble.Extensions.Configuration.Tests.ConfigFile
{
    [TestClass]
    public class FullWebConfigTests
    {

        [TestMethod]
        public void RunTests()
        {
            var builder = new ConfigurationBuilder()
                 .AddConfigFile("Web.Full.config");
            var configuration = builder.Build();

            configuration.GetAppSetting("owin:AutomaticAppStartup").Should().Be("true");
            configuration.GetAppSetting("SomeOtherKey").Should().Be("SomeOtherValue");

            var connectionStrings = configuration.GetSection("ConnectionStrings").GetChildren();
            connectionStrings.Should().NotBeNullOrEmpty();
            connectionStrings.First().Key.Should().Be("AlexisContext");
            connectionStrings.First().Value.Should().Contain("Alexis");

            var alexisApp = configuration.GetValueDynamic("alexis", "app");
            (alexisApp as ExpandoObject).Should().NotBeNull();
            (alexisApp.endpoint as string).Should().Be("https://test.com/");

            var alexisAppObject = configuration.GetValue<AlexisApp>("alexis", "app");
            alexisAppObject.Should().NotBeNull();
            alexisAppObject.Endpoint.Should().Be("https://test.com/");
        }

    }

}
