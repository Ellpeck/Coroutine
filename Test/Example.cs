using System;
using System.Collections.Generic;
using System.Threading;
using Coroutine;

namespace Test {
    internal static class Example {

        private static readonly Event TestEvent = new Event();

        public static void Main() {
            var seconds = CoroutineHandler.Start(WaitSeconds());
            CoroutineHandler.Start(PrintEvery10Seconds(seconds));

            CoroutineHandler.InvokeLater(new WaitSeconds(10), () => {
                Console.WriteLine("Raising test event");
                CoroutineHandler.RaiseEvent(TestEvent);
            });
            CoroutineHandler.InvokeLater(new WaitEvent(TestEvent), () => Console.WriteLine("Test event received"));

            var lastTime = DateTime.Now;
            while (true) {
                var currTime = DateTime.Now;
                CoroutineHandler.Tick((currTime - lastTime).TotalSeconds);
                lastTime = currTime;
                Thread.Sleep(1);
            }
        }

        private static IEnumerator<IWait> WaitSeconds() {
            Console.WriteLine("First thing " + DateTime.Now);
            yield return new WaitSeconds(1);
            Console.WriteLine("After 1 second " + DateTime.Now);
            yield return new WaitSeconds(9);
            Console.WriteLine("After 10 seconds " + DateTime.Now);
            yield return new WaitSeconds(5);
            Console.WriteLine("After 5 more seconds " + DateTime.Now);
            yield return new WaitSeconds(10);
            Console.WriteLine("After 10 more seconds " + DateTime.Now);

            yield return new WaitSeconds(20);
            Console.WriteLine("First coroutine done");
        }

        private static IEnumerator<IWait> PrintEvery10Seconds(ActiveCoroutine first) {
            while (true) {
                yield return new WaitSeconds(10);
                Console.WriteLine("The time is " + DateTime.Now);
                if (first.IsFinished) {
                    Console.WriteLine("By the way, the first coroutine has finished!");
                    Environment.Exit(0);
                }
            }
        }

    }
}