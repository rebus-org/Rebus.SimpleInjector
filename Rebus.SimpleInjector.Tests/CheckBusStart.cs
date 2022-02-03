using NUnit.Framework;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Tests.Contracts;
using Rebus.Transport.InMem;
using SimpleInjector;

namespace Rebus.SimpleInjector.Tests;

[TestFixture]
public class CheckBusStart : FixtureBase
{
    [Test]
    public void CheckBusRegistration_StartAutomatically()
    {
        using var container = new Container();

        container.RegisterRebus(
            configure => configure
                .Transport(t => t.UseInMemoryTransport(new InMemNetwork(), "who-cares"))
        );

        container.Verify();

        var bus = container.GetInstance<IBus>();

        Assert.That(bus.Advanced.Workers.Count, Is.EqualTo(1));
    }

    [Test]
    public void CheckBusRegistration_StartManually()
    {
        using var container = new Container();

        container.RegisterRebus(
            configure => configure
                .Transport(t => t.UseInMemoryTransport(new InMemNetwork(), "who-cares")),

            startAutomatically: false
        );

        container.Verify();

        var bus = container.GetInstance<IBus>();

        Assert.That(bus.Advanced.Workers.Count, Is.EqualTo(0));

        container.StartBus();
    
        Assert.That(bus.Advanced.Workers.Count, Is.EqualTo(1));
    }
}