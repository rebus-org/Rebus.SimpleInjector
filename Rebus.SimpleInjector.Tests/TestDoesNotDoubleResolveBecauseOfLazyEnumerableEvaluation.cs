using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Handlers;
using Rebus.Tests.Contracts;
using Rebus.Tests.Contracts.Extensions;
using Rebus.Transport.InMem;
using SimpleInjector;

namespace Rebus.SimpleInjector.Tests;

[TestFixture]
[Description("Diagnose why handler instances seemed to be created one extra time... turned out to be caused by the container's auto-verification mechanism")]
public class TestDoesNotDoubleResolveBecauseOfLazyEnumerableEvaluation : FixtureBase
{
    [Test]
    public async Task SureOk()
    {
        var container = Using(new Container { Options = { DefaultScopedLifestyle = ScopedLifestyle.Flowing } });

        var gotTheString = new ManualResetEvent(initialState: false);

        container.RegisterInstance(gotTheString);

        container.RegisterRebus(
            configure => configure
                .Transport(t => t.UseInMemoryTransport(new InMemNetwork(), "whatever-man"))
        );

        container.RegisterHandlers<string, MyMessageHandler>();

        var bus = container.GetInstance<IBus>();

        await bus.SendLocal("HEJ MED DIG");

        gotTheString.WaitOrDie(timeout: TimeSpan.FromSeconds(3), errorMessage: "Didn't ");
    }

    class MyMessageHandler : IHandleMessages<string>
    {
        readonly ManualResetEvent _gotTheString;

        public MyMessageHandler(ManualResetEvent gotTheString)
        {
            _gotTheString = gotTheString;
            Console.WriteLine($@"CREATED here:

{Environment.StackTrace}

???");
        }

        public async Task Handle(string message) => _gotTheString.Set();
    }
}