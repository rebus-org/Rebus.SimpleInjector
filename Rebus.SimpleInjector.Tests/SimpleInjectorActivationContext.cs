using System;
using System.Collections.Generic;
using System.Linq;
using Rebus.Activation;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Extensions;
using Rebus.Handlers;
using Rebus.Tests.Contracts.Activation;
using SimpleInjector;

namespace Rebus.SimpleInjector.Tests
{
    public class SimpleInjectorActivationContext : IActivationContext
    {
        public IHandlerActivator CreateActivator(Action<IHandlerRegistry> configureHandlers, out IActivatedContainer container)
        {
            var registry = new HandlerRegistry();
            configureHandlers(registry);

            var simpleInjContainer = new Container();
            registry.ApplyRegistrations(simpleInjContainer);

            container = new ActivatedContainer(simpleInjContainer);

            return new SimpleInjectorContainerAdapter(simpleInjContainer);
        }

        public IBus CreateBus(Action<IHandlerRegistry> configureHandlers, Func<RebusConfigurer, RebusConfigurer> configureBus, out IActivatedContainer container)
        {
            var registry = new HandlerRegistry();
            configureHandlers(registry);

            var simpleInjContainer = new Container();

            registry.ApplyRegistrations(simpleInjContainer);

            container = new ActivatedContainer(simpleInjContainer);

            //return configureBus(Configure.With(new SimpleInjectorContainerAdapter(simpleInjContainer))).Start();
            simpleInjContainer.ConfigureRebus(configurer => configureBus(configurer).Start());

            simpleInjContainer.StartBus();

            return simpleInjContainer.GetInstance<IBus>();
        }

        class HandlerRegistry : IHandlerRegistry
        {
            public HashSet<Type> HandlerTypesToRegister { get; } = new HashSet<Type>();

            public IHandlerRegistry Register<THandler>() where THandler : class, IHandleMessages
            {
                HandlerTypesToRegister.Add(typeof(THandler));

                return this;
            }

            public void ApplyRegistrations(Container container)
            {
                HandlerTypesToRegister
                    .SelectMany(type => GetHandlerInterfaces(type)
                        .Select(handlerType =>
                            new
                            {
                                HandlerType = handlerType,
                                ConcreteType = type
                            }))
                    .GroupBy(a => a.HandlerType)
                    .ForEach(a =>
                    {
                        var serviceType = a.Key;

                        Console.WriteLine("Registering {0} => {1}", serviceType, string.Join(", ", a));

                        container.RegisterCollection(serviceType, a.Select(g => g.ConcreteType));
                    });
            }

            static IEnumerable<Type> GetHandlerInterfaces(Type handlerType)
            {
#if NETSTANDARD1_6
            return System.Reflection.IntrospectionExtensions.GetTypeInfo(handlerType)
                .GetInterfaces().Where(i => System.Reflection.IntrospectionExtensions.GetTypeInfo(i).IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandleMessages<>));
#else
                return handlerType.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandleMessages<>));
#endif
            }
        }

        class ActivatedContainer : IActivatedContainer
        {
            readonly Container _container;

            public ActivatedContainer(Container container)
            {
                _container = container;
            }

            public IBus ResolveBus()
            {
                return _container.GetInstance<IBus>();
            }

            public void Dispose()
            {
                _container.Dispose();
            }
        }
    }
}
