using System;
using System.Reflection;
using System.Threading;
using NUnit.Framework;
using Rebus.Tests.Contracts;
using Rebus.Transport.InMem;
using SimpleInjector;
using SimpleInjector.Packaging;

namespace Rebus.SimpleInjector.Tests
{
    [TestFixture]
    public class TestRealisticConfigurationScenario : FixtureBase
    {
        [Test]
        public void SimpleInjectorDoesNotThrow()
        {
            var container = new Container { Options = { DefaultScopedLifestyle = ScopedLifestyle.Flowing } };

            Using(container);

            container.RegisterPackages(new[]
            {
                typeof(TestRealisticConfigurationScenario).GetTypeInfo().Assembly,
            });

            container.StartBus();

            Thread.Sleep(1000);
        }
    }

    public class RebusPackage : IPackage
    {
        public void RegisterServices(Container container)
        {
            Console.WriteLine("Calling RebusPackage");

            container.ConfigureRebus(
                configurer => configurer
                    .Transport(t => t.UseInMemoryTransport(new InMemNetwork(), "test"))
                    .Start()
            );

        }
    }

    public class AnotherPackage : IPackage
    {
        public void RegisterServices(Container container)
        {
            Console.WriteLine("Calling AnotherPackage");

            container.Register<Func<string>>(() => () => "HEJ MED DIG");
        }
    }
}