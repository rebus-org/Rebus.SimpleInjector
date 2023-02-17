# Rebus.SimpleInjector

[![install from nuget](https://img.shields.io/nuget/v/Rebus.SimpleInjector.svg?style=flat-square)](https://www.nuget.org/packages/Rebus.SimpleInjector)

Provides a SimpleInjector-based container adapter for [Rebus](https://github.com/rebus-org/Rebus).

![](https://raw.githubusercontent.com/rebus-org/Rebus/master/artwork/little_rebusbus2_copy-200x200.png)

---

To configure Rebus to work with your SimpleInjector container, simply call

```csharp
container.RegisterRebus(
    configurer => configurer
        .(...)
);
```

where the `(...)` is the part usually omitted from the Rebus configuration examples.

A slightly more realistic example (using Serilog, RabbitMQ and SQL Server) could look like this:

```csharp
var rabbitMqConnectionString = "amqp://rebususer:blablasecret@BIGRABBIT01.local";
var sqlServerConnectionString = "server=SQLMOTEL01.local; database=RebusStuff; trusted_connection=true";

container.RegisterRebus(
	configurer => configurer
		.Logging(l => l.Serilog())
		.Transport(t => t.UseRabbitMq(rabbitMqConnectionString, "simpleinjectortest"))
		.Sagas(s => s.StoreInSqlServer(sqlServerConnectionString, "Sagas", "SagaIndex"))
);
```

The examples shown so far will make the necessary container registrations, but the bus will not be started until either

1. The container resolves the `IBus` instance, or
1. You call the `container.StartBus()` extension method

so you should probably always remember to call `container.StartBus()` when your application starts (after it has
finished making ALL of its container registrations).

If you would like to be able to resolve `IBus` WITHOUT starting consuming messages, you can set `startAutomatically` to `false`
when configuring it like so:

```csharp
container.RegisterRebus(
	configurer => configurer
		.Transport(...),

	startAutomatically: false
);
```

which will configure the bus to have 0 workers when it's resolved from the container. You must then call

```csharp
container.StartBus();
```

to start it.


### So why is it different from all the other container adapters?

Because SimpleInjector is very opinionated about its registration API and Rebus is pretty loose about it :)


### How to register handlers?

Since Rebus' container adapters resolve ALL handlers that can handle an incoming message, handlers must be registered with the
registration API for collections, e.g. like

```csharp
container.Collection.Register<IHandleMessages<SomeMessage>>(typeof(SomeMessageHandler));
```

Due to limitations in SimpleInjector, you must be sure that all handlers for a given message type get registered with a single registration call similar to the one above.

There also exists a couple of extension methods that help you register handlers (up to three per message type) just the right way:

```csharp
// one message handler
container.RegisterHandlers<SomeMessage, SomeMessageHandler>();

// two handlers of SomeMessage
container.RegisterHandlers<SomeMessage, SomeMessageHandler, AnotherMessageHandler>();

// three handlers of SomeMessage
container.RegisterHandlers<SomeMessage, SomeMessageHandler, AnotherMessageHandler, YetAnotherMessageHandler>();
```

The advantage of using Rebus' configuration extension is that it's easier to get right, because the extension will register it the right way in the container, and
the API uses C# generics constraints to validate that the given handler types do in fact implement `IHandleMessages<>` closed with the right type.