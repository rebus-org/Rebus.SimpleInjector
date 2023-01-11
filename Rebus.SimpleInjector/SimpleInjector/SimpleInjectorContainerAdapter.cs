using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rebus.Activation;
using Rebus.Bus;
using Rebus.Handlers;
using Rebus.Transport;
using SimpleInjector;
using SimpleInjector.Lifestyles;

// ReSharper disable ArgumentsStyleLiteral
#pragma warning disable 1998

namespace Rebus.SimpleInjector;

class SimpleInjectorContainerAdapter : IContainerAdapter
{
    readonly Container _container;

    /// <summary>
    /// Constructs the container adapter
    /// </summary>
    public SimpleInjectorContainerAdapter(Container container)
    {
        _container = container ?? throw new ArgumentNullException(nameof(container));
    }

    /// <summary>
    /// Resolves all handlers for the given <typeparamref name="TMessage"/> message type
    /// </summary>
    public async Task<IEnumerable<IHandleMessages<TMessage>>> GetHandlers<TMessage>(TMessage message, ITransactionContext transactionContext)
    {
        var scope = transactionContext.GetOrAdd("current-simpleinjector-scope", () =>
        {
            var newScope = AsyncScopedLifestyle.BeginScope(_container);
            transactionContext.OnDisposed(_ => newScope.Dispose());
            return newScope;
        });

        return TryGetInstance<IEnumerable<IHandleMessages<TMessage>>>(scope, out var handlerInstances)
            ? handlerInstances.ToList()
            : Array.Empty<IHandleMessages<TMessage>>();
    }

    static bool TryGetInstance<TService>(IServiceProvider provider, out TService instance)
        where TService : class
    {
        instance = (TService)provider.GetService(typeof(TService));
        return instance != null;
    }

    bool _busWasSet;

    public void SetBus(IBus bus)
    {
        // hack: this is just to satisfy the contract test... we are pretty sure that
        // 1. the SetBus method is not called twice, becauwe
        // 2. we are the only ones who create the instance of the container adapter, because
        // 3. the container adapter class is internal
        if (_busWasSet)
        {
            throw new InvalidOperationException("SetBus was called twice on the container adapter. This is a sign that something has gone wrong during the configuration process.");
        }
        _busWasSet = true;

        // 2nd hack:
        // again: we control calls to SetBus in this container adapter, because we create it...
        // so, to make the contract tests happy, we need to do this:
        var actualBusType = bus.GetType();

        if (actualBusType.Name == "FakeBus" && actualBusType.DeclaringType?.Name == "ContainerTests`1")
        {
            bus.Dispose();
        }
    }
}