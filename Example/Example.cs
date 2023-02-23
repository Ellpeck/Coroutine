using System;
using System.Collections.Generic;
using System.Threading;
using Coroutine;

namespace Example {
    internal static class Example {

        private static readonly Event TestEvent = new Event();

        public static void Main() {
            var seconds = CoroutineHandler.Start(Example.WaitSeconds(), "Awesome Waiting Coroutine");
            CoroutineHandler.Start(Example.PrintEvery10Seconds(seconds));

            CoroutineHandler.Start(Example.EmptyCoroutine());

            CoroutineHandler.InvokeLater(new Wait(5), () => {
                Console.WriteLine("Raising test event");
                CoroutineHandler.RaiseEvent(Example.TestEvent);
            });
            CoroutineHandler.InvokeLater(new Wait(Example.TestEvent), () => Console.WriteLine("Example event received"));

            CoroutineHandler.InvokeLater(new Wait(Example.TestEvent), () => Console.WriteLine("I am invoked after 'Example event received'"), priority: -5);
            CoroutineHandler.InvokeLater(new Wait(Example.TestEvent), () => Console.WriteLine("I am invoked before 'Example event received'"), priority: 2);

            var lastTime = DateTime.Now;
            while (true) {
                var currTime = DateTime.Now;
                CoroutineHandler.Tick(currTime - lastTime);
                lastTime = currTime;
                Thread.Sleep(1);
            }
        }

        private static IEnumerator<Wait> WaitSeconds() {
            Console.WriteLine("First thing " + DateTime.Now);
            yield return new Wait(1);
            Console.WriteLine("After 1 second " + DateTime.Now);
            yield return new Wait(9);
            Console.WriteLine("After 10 seconds " + DateTime.Now);
            CoroutineHandler.Start(Example.NestedCoroutine());
            yield return new Wait(5);
            Console.WriteLine("After 5 more seconds " + DateTime.Now);
            yield return new Wait(10);
            Console.WriteLine("After 10 more seconds " + DateTime.Now);

            yield return new Wait(20);
            Console.WriteLine("First coroutine done");
        }

        private static IEnumerator<Wait> PrintEvery10Seconds(ActiveCoroutine first) {
            while (true) {
                yield return new Wait(10);
                Console.WriteLine("The time is " + DateTime.Now);
                if (first.IsFinished) {
                    Console.WriteLine("By the way, the first coroutine has finished!");
                    Console.WriteLine($"{first.Name} data: {first.MoveNextCount} moves, " +
                                      $"{first.TotalMoveNextTime.TotalMilliseconds} total time, " +
                                      $"{first.LastMoveNextTime.TotalMilliseconds} last time");
                    Environment.Exit(0);
                }
            }
        }

        private static IEnumerator<Wait> EmptyCoroutine() {
            yield break;
        }

        private static IEnumerable<Wait> NestedCoroutine() {
            Console.WriteLine("I'm a coroutine that was started from another coroutine!");
            yield return new Wait(5);
            Console.WriteLine("It's been 5 seconds since a nested coroutine was started, yay!");
        }

    }
}