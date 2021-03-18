using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoroutineTests
{
    using Coroutine;
    using System.Collections.Generic;
    using System.Threading;

    [TestClass]
    public class TimeBasedCoroutineTests
    {
        [TestMethod]
        public void TestTimerBasedCorotuine()
        {
            int counter = 0;
            IEnumerator<Wait> OnTimeTickCodeExecuted()
            {
                counter++;
                yield return new Wait(0.1d);
                counter++;
            }

            var cr = CoroutineHandler.Start(OnTimeTickCodeExecuted());
            Assert.AreEqual(1, counter, "instruction before yield is not executed.");
            Assert.AreEqual(string.Empty, cr.Name, "Incorrect default name found");
            Assert.AreEqual(0, cr.Priority, "Default priority is not minimum");
            for (int i = 0; i < 5; i++)
                this.SimulateTime(1);
            Assert.AreEqual(2, counter, "instruction after yield is not executed.");
            Assert.AreEqual(true, cr.IsFinished, "Incorrect IsFinished value.");
            Assert.AreEqual(false, cr.WasCanceled, "Incorrect IsCanceled value.");
            Assert.AreEqual(cr.MoveNextCount, 2, "Incorrect MoveNextCount value.");
        }

        [TestMethod]
        public void TestCorotuineReturningWeirdYields()
        {
            int counter = 0;
            IEnumerator<Wait> OnTimeTickNeverReturnYield()
            {
                counter++; // 1
                if (counter == 100) // condition that's expected to be false.
                {
                    yield return new Wait(0.1d);
                }

                counter++; // 2
            }

            IEnumerator<Wait> OnTimeTickYieldBreak()
            {
                counter++; // 3
                yield break;
                counter++; // still 3
            }

            var cr = new ActiveCoroutine[2];
            cr[0] = CoroutineHandler.Start(OnTimeTickNeverReturnYield());
            cr[1] = CoroutineHandler.Start(OnTimeTickYieldBreak());
            for (int i = 0; i < 5; i++)
                this.SimulateTime(1);

            Assert.AreEqual(3, counter, $"Incorrect counter value.");
            for (int i = 0; i < cr.Length; i++)
            {
                Assert.AreEqual(true, cr[i].IsFinished, $"Incorrect IsFinished value on index {i}.");
                Assert.AreEqual(false, cr[i].WasCanceled, $"Incorrect IsCanceled value on index {i}");
                Assert.AreEqual(1, cr[i].MoveNextCount, $"Incorrect MoveNextCount value on index {i}");
            }
        }

        [TestMethod]
        public void TestCorotuineReturningDefaultYield()
        {
            int counter = 0;
            IEnumerator<Wait> OnTimeTickYieldDefault()
            {
                counter++; // 1
                yield return default;
                counter++; // 2
            }

            var cr = CoroutineHandler.Start(OnTimeTickYieldDefault());
            for (int i = 0; i < 5; i++)
                this.SimulateTime(1);

            Assert.AreEqual(2, counter, $"Incorrect counter value.");
            Assert.AreEqual(true, cr.IsFinished, $"Incorrect IsFinished value.");
            Assert.AreEqual(false, cr.WasCanceled, $"Incorrect IsCanceled value.");
            Assert.AreEqual(2, cr.MoveNextCount, $"Incorrect MoveNextCount value.");
        }

        [TestMethod]
        public void TestInfiniteCorotuineNeverFinishesUnlessCanceled()
        {
            int counter = 0;
            IEnumerator<Wait> OnTimerTickInfinite()
            {
                while (true)
                {
                    counter++;
                    yield return new Wait(1);
                }
            }

            void setCounterToUnreachableValue(ActiveCoroutine coroutine)
            {
                counter = -100;
            }

            var cr = CoroutineHandler.Start(OnTimerTickInfinite());
            cr.OnFinished += setCounterToUnreachableValue;
            for (int i = 0; i < 50; i++)
                this.SimulateTime(1);

            Assert.AreEqual(51, counter, $"Incorrect counter value.");
            Assert.AreEqual(false, cr.IsFinished, $"Incorrect IsFinished value.");
            Assert.AreEqual(false, cr.WasCanceled, $"Incorrect IsCanceled value.");
            Assert.AreEqual(51, cr.MoveNextCount, $"Incorrect MoveNextCount value.");

            cr.Cancel();
            Assert.AreEqual(true, cr.WasCanceled, $"Incorrect IsCanceled value after canceling.");
            Assert.AreEqual(-100, counter, $"OnFinished event not triggered when canceled.");
            Assert.AreEqual(51, cr.MoveNextCount, $"Incorrect MoveNextCount value.");
            Assert.AreEqual(true, cr.IsFinished, $"Incorrect IsFinished value.");
        }

        [TestMethod]
        public void TestOnFinishedEventExecuted()
        {
            int counter = 0;
            IEnumerator<Wait> OnTimeTick()
            {
                counter++;
                yield return new Wait(0.1d);
            }

            void setCounterToUnreachableValue(ActiveCoroutine coroutine)
            {
                counter = -100;
            }

            var cr = CoroutineHandler.Start(OnTimeTick());
            cr.OnFinished += setCounterToUnreachableValue;
            this.SimulateTime(50);
            Assert.AreEqual(-100, counter, $"Incorrect counter value.");
        }

        [TestMethod]
        public void TestNestedCorotuine()
        {
            int counterAlwaysRunning = 0;
            IEnumerator<Wait> AlwaysRunning()
            {
                while (true)
                {
                    yield return new Wait(1);
                    counterAlwaysRunning++;
                }
            }

            int counterChild = 0;
            IEnumerator<Wait> Child()
            {
                yield return new Wait(1);
                counterChild++;
            }

            int counterParent = 0;
            IEnumerator<Wait> Parent()
            {
                yield return new Wait(1);
                counterParent++;
                // OnFinish I will start child.
            }

            int counterGrandParent = 0;
            IEnumerator<Wait> GrandParent()
            {
                yield return new Wait(1);
                counterGrandParent++;

                // Nested corotuine starting.
                var p = CoroutineHandler.Start(Parent());

                // Nested corotuine starting in OnFinished.
                p.OnFinished += (ActiveCoroutine ac) => { CoroutineHandler.Start(Child()); };
            }

            CoroutineHandler.Start(AlwaysRunning());
            CoroutineHandler.Start(GrandParent());
            Assert.AreEqual(0, counterAlwaysRunning, "Always running counter is invalid at time 0.");
            Assert.AreEqual(0, counterGrandParent, "Grand Parent counter is invalid at time 0.");
            Assert.AreEqual(0, counterParent, "Parent counter is invalid at time 0.");
            Assert.AreEqual(0, counterChild, "Child counter is invalid at time 0.");
            this.SimulateTime(1);
            Assert.AreEqual(1, counterAlwaysRunning, "Always running counter is invalid at time 1.");
            Assert.AreEqual(1, counterGrandParent, "Grand Parent counter is invalid at time 1.");
            Assert.AreEqual(0, counterParent, "Parent counter is invalid at time 1.");
            Assert.AreEqual(0, counterChild, "Child counter is invalid at time 1.");
            this.SimulateTime(1);
            Assert.AreEqual(2, counterAlwaysRunning, "Always running counter is invalid at time 2.");
            Assert.AreEqual(1, counterGrandParent, "Grand Parent counter is invalid at time 2.");
            Assert.AreEqual(1, counterParent, "Parent counter is invalid at time 2.");
            Assert.AreEqual(0, counterChild, "Child counter is invalid at time 2.");
            this.SimulateTime(1);
            Assert.AreEqual(3, counterAlwaysRunning, "Always running counter is invalid at time 3.");
            Assert.AreEqual(1, counterGrandParent, "Grand Parent counter is invalid at time 3.");
            Assert.AreEqual(1, counterParent, "Parent counter is invalid at time 3.");
            Assert.AreEqual(1, counterChild, "Child counter is invalid at time 3.");
            this.SimulateTime(1);
            Assert.AreEqual(4, counterAlwaysRunning, "Always running counter is invalid at time 4.");
            Assert.AreEqual(1, counterGrandParent, "Grand Parent counter is invalid at time 4.");
            Assert.AreEqual(1, counterParent, "Parent counter is invalid at time 4.");
            Assert.AreEqual(1, counterChild, "Child counter is invalid at time 4.");
        }

        [TestMethod]
        public void TestPriority()
        {
            int counterShouldExecuteBefore0 = 0;
            IEnumerator<Wait> ShouldExecuteBefore0()
            {
                while (true)
                {
                    yield return new Wait(1);
                    counterShouldExecuteBefore0++;
                }
            }

            int counterShouldExecuteBefore1 = 0;
            IEnumerator<Wait> ShouldExecuteBefore1()
            {
                while (true)
                {
                    yield return new Wait(1);
                    counterShouldExecuteBefore1++;
                }
            }

            int counterShouldExecuteAfter = 0;
            IEnumerator<Wait> ShouldExecuteAfter()
            {
                while (true)
                {
                    yield return new Wait(1);
                    if (counterShouldExecuteBefore0 == 1 &&
                        counterShouldExecuteBefore1 == 1)
                    {
                        counterShouldExecuteAfter++;
                    }
                }
            }

            int counterShouldExecuteFinally = 0;
            IEnumerator<Wait> ShouldExecuteFinally()
            {
                while (true)
                {
                    yield return new Wait(1);
                    if (counterShouldExecuteAfter > 0)
                    {
                        counterShouldExecuteFinally++;
                    }
                }
            }

            int highPriority = int.MaxValue;
            CoroutineHandler.Start(ShouldExecuteBefore1(), priority: highPriority);
            CoroutineHandler.Start(ShouldExecuteAfter());
            CoroutineHandler.Start(ShouldExecuteBefore0(), priority: highPriority);
            CoroutineHandler.Start(ShouldExecuteFinally(), priority: -1);
            this.SimulateTime(10);
            Assert.AreEqual(1, counterShouldExecuteAfter, $"ShouldExecuteAfter counter  {counterShouldExecuteAfter} is invalid.");
            Assert.AreEqual(1, counterShouldExecuteFinally, $"ShouldExecuteFinally counter  {counterShouldExecuteFinally} is invalid.");
        }

        [TestMethod]
        public void TestTimeBasedCorotuineIsAccurate()
        {
            int counter0 = 0;
            IEnumerator<Wait> IncrementCounter0Ever10Seconds()
            {
                while (true)
                {
                    yield return new Wait(10);
                    counter0++;
                }
            }

            int counter1 = 0;
            IEnumerator<Wait> IncrementCounter1Every5Seconds()
            {
                while (true)
                {
                    yield return new Wait(5);
                    counter1++;
                }
            }

            CoroutineHandler.Start(IncrementCounter0Ever10Seconds());
            CoroutineHandler.Start(IncrementCounter1Every5Seconds());
            this.SimulateTime(3);
            Assert.AreEqual(0, counter0, $"Incorrect counter0 value after 3 seconds.");
            Assert.AreEqual(0, counter1, $"Incorrect counter1 value after 3 seconds.");
            this.SimulateTime(3);
            Assert.AreEqual(0, counter0, $"Incorrect counter0 value after 6 seconds.");
            Assert.AreEqual(1, counter1, $"Incorrect counter1 value after 6 seconds.");

            // it's 5 over here because IncrementCounter1Every5Seconds
            // increments 5 seconds after last yield. not 5 seconds since start.
            // So the when we send 3 seconds in the last SimulateTime,
            // the 3rd second was technically ignored.
            this.SimulateTime(5);
            Assert.AreEqual(1, counter0, $"Incorrect counter0 value after 10 seconds.");
            Assert.AreEqual(2, counter1, $"Incorrect counter1 value after next 5 seconds.");
        }

        [TestMethod]
        public void InvokeLaterAndNameTest()
        {
            int counter = 0;
            var cr = CoroutineHandler.InvokeLater(new Wait(10), () => {
                counter++;
            }, "Bird");

            this.SimulateTime(5);
            Assert.AreEqual(0, counter, $"Incorrect counter value after 5 seconds.");
            this.SimulateTime(5);
            Assert.AreEqual(1, counter, $"Incorrect counter value after 10 seconds.");
            Assert.AreEqual(true, cr.IsFinished, "Incorrect IsFinished value.");
            Assert.AreEqual(false, cr.WasCanceled, "Incorrect IsCanceled value.");
            Assert.AreEqual(cr.MoveNextCount, 2, "Incorrect MoveNextCount value.");
            Assert.AreEqual(cr.Name, "Bird", "Incorrect name of the coroutine.");
        }

        [TestMethod]
        public void CorotuineStatsAre95PercentAccurate()
        {
            IEnumerator<Wait> CorotuineTakesMax500MS()
            {
                Thread.Sleep(200);
                yield return new Wait(10);
                Thread.Sleep(500);
            }

            var cr = CoroutineHandler.Start(CorotuineTakesMax500MS());
            for (int i = 0; i < 5; i++)
                this.SimulateTime(50);

            int expected1 = 350;
            float errorbar1 = (5 / 100f * expected1);
            bool gTA = cr.AverageMoveNextTime.Milliseconds > (expected1 - errorbar1); // 95% accuracy.
            bool lTB = cr.AverageMoveNextTime.Milliseconds < (expected1 + errorbar1); // 95% accuracy.
            Assert.IsTrue(gTA && lTB, $"Average Move Next Time {cr.AverageMoveNextTime.Milliseconds} is invalid.");

            int expected2 = 500;
            float errorbar2 = (5 / 100f * expected2);
            bool gTC = cr.MaxMoveNextTime.Milliseconds > (expected2 - errorbar2); // 95% accuracy.
            bool lTD = cr.MaxMoveNextTime.Milliseconds < (expected2 + errorbar2); // 95% accuracy.
            Assert.IsTrue(gTC && lTD, $"Maximum Move Next Time {cr.MaxMoveNextTime.Milliseconds} is invalid.");
        }

        private void SimulateTime(double totalSeconds)
        {
            CoroutineHandler.Tick(totalSeconds);
        }
    }
}
