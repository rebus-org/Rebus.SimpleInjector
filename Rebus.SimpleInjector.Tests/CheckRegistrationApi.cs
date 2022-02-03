using NUnit.Framework;
using Rebus.Config;
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

        container.Verify();
    }
}