using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Rebus.Handlers;
using Rebus.Tests.Contracts.Activation;
using Rebus.Transport;

namespace Rebus.SimpleInjector.Tests;

[TestFixture]
public class SimpleInjectorAssumptions
{
    [Test]
    public void RegisterWorks()
    {
        var activationCtx = new SimpleInjectorActivationContext();

        using (var scope = new RebusTransactionScope())
        {
            const string stringMessage = "bimse";

            var activator = activationCtx.CreateActivator(handlerReg => handlerReg.Register<SomeHandler>().Register<AnotherHandler>());

            var handlers = activator.GetHandlers(stringMessage, scope.TransactionContext).Result.ToList();
                
            Assert.That(handlers.Count, Is.EqualTo(2));
        }
    }

    class SomeHandler : IHandleMessages<string>
    {
        public Task Handle(string message)
        {
            throw new NotImplementedException();
        }
    }

    class AnotherHandler : IHandleMessages<string>
    {
        public Task Handle(string message)
        {
            throw new NotImplementedException();
        }
    }

}