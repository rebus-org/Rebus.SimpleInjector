using System;
using System.Collections.Generic;
using Rebus.Bus.Advanced;

namespace Rebus.Internals.Fakes;

class FakeSyncBus : ISyncBus
{
    public void SendLocal(object commandMessage, IDictionary<string, string> optionalHeaders = null)
    {
        throw new NotImplementedException();
    }

    public void Send(object commandMessage, IDictionary<string, string> optionalHeaders = null)
    {
        throw new NotImplementedException();
    }

    public void Reply(object replyMessage, IDictionary<string, string> optionalHeaders = null)
    {
        throw new NotImplementedException();
    }

    public void Defer(TimeSpan delay, object message, IDictionary<string, string> optionalHeaders = null)
    {
        throw new NotImplementedException();
    }

    public void DeferLocal(TimeSpan delay, object message, IDictionary<string, string> optionalHeaders = null)
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

    public void Publish(object eventMessage, IDictionary<string, string> optionalHeaders = null)
    {
        throw new NotImplementedException();
    }
}