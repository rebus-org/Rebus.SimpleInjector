using System.Threading.Tasks;
using NUnit.Framework;
using Rebus.Config;
using Rebus.Handlers;
using Rebus.Logging;
using Rebus.Transport.InMem;
using SimpleInjector;

namespace Rebus.SimpleInjector.Tests;

[TestFixture]
public class CheckRegistrationApi
{
    [Test]
    public void ThisIsHowItLooks()
    {
        var container = new Container();

        container.RegisterRebus(
            configure => configure
                .Logging(l => l.Console(minLevel: LogLevel.Debug))
                .Transport(t => t.UseInMemoryTransport(new InMemNetwork(), "whatever"))
        );

        container.Collection.Register<IHandleMessages<string>>(typeof(MyStringHandler));

        container.Verify();
    }

    class MyStringHandler : IHandleMessages<string>
    {
        public Task Handle(string message)
        {
            throw new System.NotImplementedException();
        }
    }
}