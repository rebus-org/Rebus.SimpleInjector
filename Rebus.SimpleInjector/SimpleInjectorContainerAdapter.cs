using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rebus.Activation;
using Rebus.Bus;
using Rebus.Bus.Advanced;
using Rebus.Extensions;
using Rebus.Handlers;
using Rebus.Messages;
using Rebus.Pipeline;
using Rebus.Transport;
using SimpleInjector;
// ReSharper disable ArgumentsStyleLiteral

#pragma warning disable 1998

namespace Rebus.SimpleInjector
{
    /// <summary>
    /// Implementation of <see cref="IContainerAdapter"/> that uses Simple Injector to do its thing
    /// </summary>
    public class SimpleInjectorContainerAdapter : IContainerAdapter, IDisposable
    {
        readonly Container _container;
        IBus _bus;

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
            if (TryGetInstance<IEnumerable<IHandleMessages<TMessage>>>(_container, out var handlerInstances))
            {
                var handlerList = handlerInstances.ToList();

                transactionContext.OnDisposed(() =>
                {
                    handlerList
                        .OfType<IDisposable>()
                        .ForEach(disposable =>
                        {
                            disposable.Dispose();
                        });
                });

                return handlerList;
            }

            return new IHandleMessages<TMessage>[0];
        }

        static bool TryGetInstance<TService>(Container container, out TService instance)
            where TService : class
        {
            IServiceProvider provider = container;
            instance = (TService)provider.GetService(typeof(TService));
            return instance != null;
        }

        /// <summary>
        /// Stores the bus instance
        /// </summary>
        public void SetBus(IBus bus)
        {
            if (_container.GetCurrentRegistrations().Any(r => r.ServiceType == typeof(IBus)))
            {
                throw new InvalidOperationException($"Cannot register IBus in the container because it has already been registered. If you want to host multiple Rebus instances in a single process, please use separate container instances for them.");
            }

            _container.RegisterSingleton(bus);
            _bus = bus;

            _container.Register(() =>
            {
                if (_container.IsVerifying)
                {
                    return new FakeSyncBus();
                }

                return bus.Advanced.SyncBus;
            });

            _container.Register(() =>
            {
                var currentMessageContext = MessageContext.Current;

                if (currentMessageContext != null)
                {
                    return currentMessageContext;
                }

                if (_container.IsVerifying)
                {
                    return new FakeMessageContext();
                }

                throw new InvalidOperationException("Attempted to inject the current message context from MessageContext.Current, but it was null! Did you attempt to resolve IMessageContext from outside of a Rebus message handler?");
            });
        }

        class FakeSyncBus : ISyncBus
        {
            public void SendLocal(object commandMessage, Dictionary<string, string> optionalHeaders = null)
            {
                throw new NotImplementedException();
            }

            public void Send(object commandMessage, Dictionary<string, string> optionalHeaders = null)
            {
                throw new NotImplementedException();
            }

            public void Reply(object replyMessage, Dictionary<string, string> optionalHeaders = null)
            {
                throw new NotImplementedException();
            }

            public void Defer(TimeSpan delay, object message, Dictionary<string, string> optionalHeaders = null)
            {
                throw new NotImplementedException();
            }

            public void Subscribe<TEvent>()
            {
                throw new NotImplementedException();
            }

            public void Subscribe(Type eventType)
            {
                throw new NotImplementedException();
            }

            public void Unsubscribe<TEvent>()
            {
                throw new NotImplementedException();
            }

            public void Unsubscribe(Type eventType)
            {
                throw new NotImplementedException();
            }

            public void Publish(object eventMessage, Dictionary<string, string> optionalHeaders = null)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Fake implementation of <see cref="IMessageContext"/> that can be returned by SimpleInjector while verifying the configuration
        /// </summary>
        class FakeMessageContext : IMessageContext
        {
            public ITransactionContext TransactionContext { get; }
            public IncomingStepContext IncomingStepContext { get; }
            public TransportMessage TransportMessage { get; }
            public Message Message { get; }
            public Dictionary<string, string> Headers { get; }
        }

        /// <summary>
        /// Disposes the bus
        /// </summary>
        public void Dispose()
        {
            _bus?.Dispose();
        }
    }
}
