using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rebus.Bus;
using Rebus.Bus.Advanced;
using Rebus.Handlers;
using Rebus.Internals;
using Rebus.Internals.Fakes;
using Rebus.Pipeline;
using Rebus.SimpleInjector;
using SimpleInjector;

namespace Rebus.Config;

/// <summary>
/// Configuration extensions for configuring a Rebus endpoint in your SimpleInjector container
/// </summary>
public static class SimpleInjectorConfigurationExtensions
{
    /// <summary>
    /// Registers <typeparamref name="THandler"/> as a handler of messages of type <typeparamref name="TMessage"/>
    /// </summary>
    public static void RegisterHandlers<TMessage, THandler>(this Container container)
        where THandler : IHandleMessages<TMessage>
    {
        RegisterHandlers(container, typeof(IHandleMessages<TMessage>), new[] { typeof(THandler) });
    }

    /// <summary>
    /// Registers <typeparamref name="THandler1"/> and <typeparamref name="THandler2"/> as handlers of messages of type <typeparamref name="TMessage"/>
    /// </summary>
    public static void RegisterHandlers<TMessage, THandler1, THandler2>(this Container container)
        where THandler1 : IHandleMessages<TMessage>
        where THandler2 : IHandleMessages<TMessage>
    {
        RegisterHandlers(container, typeof(IHandleMessages<TMessage>), new[] { typeof(THandler1), typeof(THandler2) });
    }

    /// <summary>
    /// Registers <typeparamref name="THandler1"/> and <typeparamref name="THandler2"/> and <typeparamref name="THandler3"/> as handlers of messages of type <typeparamref name="TMessage"/>
    /// </summary>
    public static void RegisterHandlers<TMessage, THandler1, THandler2, THandler3>(this Container container)
        where THandler1 : IHandleMessages<TMessage>
        where THandler2 : IHandleMessages<TMessage>
        where THandler3 : IHandleMessages<TMessage>
    {
        RegisterHandlers(container, typeof(IHandleMessages<TMessage>), new[] { typeof(THandler1), typeof(THandler2), typeof(THandler3) });
    }

    static void RegisterHandlers(Container container, Type messageHandlerType, IEnumerable<Type> concreteHandlerTypes)
    {
        container.Collection.Register(messageHandlerType, concreteHandlerTypes, Lifestyle.Scoped);
    }

    /// <summary>
    /// Makes the necessary registrations in the container, registering the passed-in <paramref name="configurationCallback"/>
    /// as a configuration callback. The callback is invoked with a <see cref="RebusConfigurer"/> which must have
    /// its <see cref="RebusConfigurer.Start"/> method called at the end, returning the resulting <see cref="IBus"/> instance.
    /// The configuration callback is called the first time the bus is resolved, which may be done manually or simply by calling
    /// <see cref="StartBus"/>
    /// </summary>
    public static void RegisterRebus(this Container container, Func<RebusConfigurer, RebusConfigurer> configurationCallback, bool startAutomatically = true)
    {
        if (container.GetCurrentRegistrations().Any(r => r.ServiceType == typeof(IBus)))
        {
            throw new InvalidOperationException("Cannot register IBus in the container because it has already been registered. If you want to host multiple Rebus instances in a single process, please use separate container instances for them.");
        }

        container.Register(() =>
        {
            if (container.IsVerifying) return new FakeSyncBus();

            return container.GetInstance<IBus>().Advanced.SyncBus;
        });

        container.Register(() =>
        {
            if (container.IsVerifying) return new FakeMessageContext();

            var currentMessageContext = MessageContext.Current;

            if (currentMessageContext != null)
            {
                return currentMessageContext;
            }

            throw new InvalidOperationException("Attempted to inject the current message context from MessageContext.Current, but it was null! Did you attempt to resolve IMessageContext from outside of a Rebus message handler?");
        });

        container.Register<IBusStarter>(() =>
        {
            var rebusConfigurer = Configure.With(new SimpleInjectorContainerAdapter(container));
            var busStarter = configurationCallback(rebusConfigurer).Create();
            return new SimpleInjectorBusStarterDecorator(busStarter);
        }, Lifestyle.Singleton);

        container.Register(() => container.GetInstance<IBusStarter>().Bus, Lifestyle.Singleton);

        container.Options.ContainerLocking += (_, _) =>
        {
            var starter = container.GetInstance<IBusStarter>();
            
            if (!startAutomatically) return;

            starter.Start();
        };
    }

    /// <summary>
    /// After having configured the bus with <see cref="RegisterRebus"/> the bus may be started by calling this method
    /// </summary>
    public static void StartBus(this Container container) => container.GetInstance<IBusStarter>().Start();
}