using NUnit.Framework;
using Rebus.Config;
using Rebus.Tests.Contracts;
using Rebus.Transport.InMem;
using SimpleInjector;
// ReSharper disable AccessToDisposedClosure

namespace Rebus.SimpleInjector.Tests.Bug;

[TestFixture]
[Description(@"Rebus defaults to start automatically, when the container is locked, which happens on the first resolve or when Verify() is called.

This would be a problem in cases where additional ContainerLocking callbacks were registered, if they expected to find the container in a state that would accept further registrations.

This test reproduced the issue, and then it went on to mitigate it, by setting startAutomatically: false, causing the bus to skip doing anything in the callback.

The user must then of course start the bus manually.")]
public class CheckContainerLockingCallback : FixtureBase
{
    [Test]
    public void CanAvoidThrowingInThisSpecialCase()
    {
        using var container = new Container();

        // first, register Rebus
        container.RegisterRebus(
            configure => configure.Transport(t => t.UseInMemoryTransport(new(), "inmem")),
            
            startAutomatically: false
        );

        // then add a callback that makes yet another registration
        container.Options.ContainerLocking += (_, _) => container.RegisterInstance(new Whatever());

        // force the container to lock itself - this used to fail 🙂
        container.Verify();
        
        // start the bus
        container.StartBus();
    }

    record Whatever;
}