using Rebus.Bus;
using Rebus.Config;

namespace Rebus.Internals;

/// <summary>
/// Decorator that ensures that there's only one single call to Start and that it's serialized
/// </summary>
class SimpleInjectorBusStarterDecorator : IBusStarter
{
    readonly IBusStarter _busStarter;
    readonly object _lock = new();

    volatile bool _started;

    public SimpleInjectorBusStarterDecorator(IBusStarter busStarter) => _busStarter = busStarter;

    public IBus Start()
    {
        // double-check locking
        if (_started) return Bus;

        lock (_lock)
        {
            if (_started) return Bus;

            _started = true;

            return _busStarter.Start();
        }
    }

    public IBus Bus => _busStarter.Bus;
}