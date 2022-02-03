using System.Collections.Generic;
using Rebus.Messages;
using Rebus.Pipeline;
using Rebus.Transport;

namespace Rebus.Internals.Fakes;

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