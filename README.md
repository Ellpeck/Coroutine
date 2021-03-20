# Coroutine
A simple implementation of Unity's Coroutines to be used for any C# project

# Features
Coroutine adds the ability to run coroutines. Coroutines are methods that run in parallel to the rest of the application through the use of an `Enumerator`. This allows for the coroutine to pause execution using the `yield return` statement.

There are two predefined ways to pause a coroutine:
- Waiting for a certain amount of seconds to have passed
- Waiting for a certain custom event to occur

Additionally, Coroutine provides the following features:
- Creation of custom events to wait for
- Creation of custom wait conditions
- No multi-threading, which allows for any kind of process to be executed in a coroutine, including rendering

# How to Use
## Setting up the CoroutineHandler
The `CoroutineHandler` is the place where coroutines get executed. For this to occur, the `Tick` method needs to be called continuously. The `Tick` method takes a single parameter which represents the amount of time since the last time it was called. It can either be called in your application's existing update loop or as follows.
```cs
var lastTime = DateTime.Now;
while (true) {
    var currTime = DateTime.Now;
    CoroutineHandler.Tick(currTime - lastTime);
    lastTime = currTime;
    Thread.Sleep(1);
}
```

## Creating a Coroutine
To create a coroutine, simply create a method with the return type `IEnumerator<Wait>`. Then, you can use `yield return` to cause the coroutine to wait at any point:
```cs
private static IEnumerator<Wait> WaitSeconds() {
    Console.WriteLine("First thing " + DateTime.Now);
    yield return new Wait(1);
    Console.WriteLine("After 1 second " + DateTime.Now);
    yield return new Wait(5);
    Console.WriteLine("After 5 seconds " + DateTime.Now);
    yield return new Wait(10);
    Console.WriteLine("After 10 seconds " + DateTime.Now);
}
```

## Starting a Coroutine
To start a coroutine, simply call `Start`:
```cs 
CoroutineHandler.Start(WaitSeconds());
```

## Using Events
To use an event, an `Event` instance first needs to be created. When not overriding any equality operators, only a single instance of each event should be used.
```cs
private static readonly Event TestEvent = new Event();
```

Waiting for an event in a coroutine works as follows:
```cs
private static IEnumerator<Wait> WaitForTestEvent() {
    yield return new Wait(TestEvent);
    Console.WriteLine("Test event received");
}
```
Of course, having time-based waits and event-based waits in the same coroutine is also supported.

To actually cause the event to be raised, causing all currently waiting coroutines to be continued, simply call `RaiseEvent`:
```cs
CoroutineHandler.RaiseEvent(TestEvent);
```

Note that, since `Tick` is an important lifecycle method, it has to be [called continuously](#Setting-up-the-CoroutineHandler) even if only event-based coroutines are used.

## Additional Examples
For additional examples, take a look at the [Example class](https://github.com/Ellpeck/Coroutine/blob/master/Example/Example.cs).