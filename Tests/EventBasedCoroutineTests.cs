using System.Collections.Generic;
using Coroutine;
using NUnit.Framework;

namespace Tests {
    public class EventBasedCoroutineTests {

        [Test]
        public void TestEventBasedCoroutine() {
            var counter = 0;
            var myEvent = new Event();

            IEnumerator<Wait> OnEventTriggered() {
                counter++;
                yield return new Wait(myEvent);
                counter++;
            }

            var cr = CoroutineHandler.Start(OnEventTriggered());
            Assert.AreEqual(1, counter, "instruction before yield is not executed.");
            CoroutineHandler.Tick(1);
            CoroutineHandler.RaiseEvent(myEvent);
            Assert.AreEqual(2, counter, "instruction after yield is not executed.");
            CoroutineHandler.Tick(1);
            CoroutineHandler.RaiseEvent(myEvent);
            Assert.AreEqual(2, counter, "instruction after yield is not executed.");

            Assert.AreEqual(true, cr.IsFinished, "Incorrect IsFinished value.");
            Assert.AreEqual(false, cr.WasCanceled, "Incorrect IsCanceled value.");
            Assert.AreEqual(cr.MoveNextCount, 2, "Incorrect MoveNextCount value.");
        }

        [Test]
        public void TestInfiniteCoroutineNeverFinishesUnlessCanceled() {
            var myEvent = new Event();
            var myOtherEvent = new Event();
            var counter = 0;

            IEnumerator<Wait> OnEventTriggeredInfinite() {
                while (true) {
                    counter++;
                    yield return new Wait(myEvent);
                }
            }

            void SetCounterToUnreachableValue(ActiveCoroutine coroutine) {
                counter = -100;
            }

            var cr = CoroutineHandler.Start(OnEventTriggeredInfinite());
            CoroutineHandler.Tick(1);
            cr.OnFinished += SetCounterToUnreachableValue;
            for (var i = 0; i < 50; i++)
                CoroutineHandler.RaiseEvent(myOtherEvent);

            for (var i = 0; i < 50; i++)
                CoroutineHandler.RaiseEvent(myEvent);

            Assert.AreEqual(51, counter, "Incorrect counter value.");
            Assert.AreEqual(false, cr.IsFinished, "Incorrect IsFinished value.");
            Assert.AreEqual(false, cr.WasCanceled, "Incorrect IsCanceled value.");
            Assert.AreEqual(51, cr.MoveNextCount, "Incorrect MoveNextCount value.");

            cr.Cancel();
            Assert.AreEqual(true, cr.WasCanceled, "Incorrect IsCanceled value after canceling.");
            Assert.AreEqual(-100, counter, "OnFinished event not triggered when canceled.");
            Assert.AreEqual(51, cr.MoveNextCount, "Incorrect MoveNextCount value.");
            Assert.AreEqual(true, cr.IsFinished, "Incorrect IsFinished value.");
        }

        [Test]
        public void TestOnFinishedEventExecuted() {
            var myEvent = new Event();
            var counter = 0;

            IEnumerator<Wait> OnEvent() {
                counter++;
                yield return new Wait(myEvent);
            }

            void SetCounterToUnreachableValue(ActiveCoroutine coroutine) {
                counter = -100;
            }

            var cr = CoroutineHandler.Start(OnEvent());
            CoroutineHandler.Tick(1);
            cr.OnFinished += SetCounterToUnreachableValue;
            for (int i = 0; i < 10; i++)
                CoroutineHandler.RaiseEvent(myEvent);
            Assert.AreEqual(-100, counter, "Incorrect counter value.");
        }

        [Test]
        public void TestNestedCoroutine() {
            var onChildCreated = new Event();
            var onParentCreated = new Event();
            var myEvent = new Event();
            var counterAlwaysRunning = 0;

            IEnumerator<Wait> AlwaysRunning() {
                while (true) {
                    yield return new Wait(myEvent);
                    counterAlwaysRunning++;
                }
            }

            var counterChild = 0;

            IEnumerator<Wait> Child() {
                yield return new Wait(myEvent);
                counterChild++;
            }

            var counterParent = 0;

            IEnumerator<Wait> Parent() {
                yield return new Wait(myEvent);
                counterParent++;
                // OnFinish I will start child.
            }

            var counterGrandParent = 0;

            IEnumerator<Wait> GrandParent() {
                yield return new Wait(myEvent);
                counterGrandParent++;
                // Nested corotuine starting.
                var p = CoroutineHandler.Start(Parent());
                CoroutineHandler.RaiseEvent(onParentCreated);
                // Nested corotuine starting in OnFinished.
                p.OnFinished += ac => {
                    CoroutineHandler.Start(Child());
                    CoroutineHandler.RaiseEvent(onChildCreated);
                };
            }

            CoroutineHandler.Start(AlwaysRunning());
            CoroutineHandler.Start(GrandParent());
            Assert.AreEqual(0, counterAlwaysRunning, "Always running counter is invalid at event 0.");
            Assert.AreEqual(0, counterGrandParent, "Grand Parent counter is invalid at event 0.");
            Assert.AreEqual(0, counterParent, "Parent counter is invalid at event 0.");
            Assert.AreEqual(0, counterChild, "Child counter is invalid at event 0.");
            CoroutineHandler.Tick(1);
            CoroutineHandler.RaiseEvent(myEvent);
            Assert.AreEqual(1, counterAlwaysRunning, "Always running counter is invalid at event 1.");
            Assert.AreEqual(1, counterGrandParent, "Grand Parent counter is invalid at event 1.");
            Assert.AreEqual(0, counterParent, "Parent counter is invalid at event 1.");
            Assert.AreEqual(0, counterChild, "Child counter is invalid at event 1.");
            CoroutineHandler.Tick(1);
            CoroutineHandler.RaiseEvent(myEvent);
            Assert.AreEqual(2, counterAlwaysRunning, "Always running counter is invalid at event 2.");
            Assert.AreEqual(1, counterGrandParent, "Grand Parent counter is invalid at event 2.");
            Assert.AreEqual(1, counterParent, "Parent counter is invalid at event 2.");
            Assert.AreEqual(0, counterChild, "Child counter is invalid at event 2.");
            CoroutineHandler.Tick(1);
            CoroutineHandler.RaiseEvent(myEvent);
            Assert.AreEqual(3, counterAlwaysRunning, "Always running counter is invalid at event 3.");
            Assert.AreEqual(1, counterGrandParent, "Grand Parent counter is invalid at event 3.");
            Assert.AreEqual(1, counterParent, "Parent counter is invalid at event 3.");
            Assert.AreEqual(1, counterChild, "Child counter is invalid at event 3.");
            CoroutineHandler.Tick(1);
            CoroutineHandler.RaiseEvent(myEvent);
            Assert.AreEqual(4, counterAlwaysRunning, "Always running counter is invalid at event 4.");
            Assert.AreEqual(1, counterGrandParent, "Grand Parent counter is invalid at event 4.");
            Assert.AreEqual(1, counterParent, "Parent counter is invalid at event 4.");
            Assert.AreEqual(1, counterChild, "Child counter is invalid at event 4.");
        }

        [Test]
        public void TestNestedRaiseEvent() {
            var event1 = new Event();
            var event2 = new Event();
            var event3 = new Event();
            var CoroutineCreated = new Event();
            int counterCoroutineA = 0;
            int counter = 0;

            CoroutineHandler.Start(OnCoroutineCreatedInfinite());
            CoroutineHandler.Start(OnEvent1());
            CoroutineHandler.Tick(1);
            CoroutineHandler.RaiseEvent(event1);
            CoroutineHandler.Tick(1);
            CoroutineHandler.RaiseEvent(event2);
            CoroutineHandler.Tick(1);
            CoroutineHandler.RaiseEvent(event3);
            Assert.AreEqual(3, counter);
            Assert.AreEqual(2, counterCoroutineA);

            IEnumerator<Wait> OnCoroutineCreatedInfinite() {
                while (true)
                {
                    yield return new Wait(CoroutineCreated);
                    counterCoroutineA++;
                }
            }

            IEnumerator<Wait> OnEvent1() {
                yield return new Wait(event1);
                counter++;
                CoroutineHandler.Start(OnEvent2());
                CoroutineHandler.RaiseEvent(CoroutineCreated);
            }

            IEnumerator<Wait> OnEvent2() {
                yield return new Wait(event2);
                counter++;
                CoroutineHandler.Start(OnEvent3());
                CoroutineHandler.RaiseEvent(CoroutineCreated);
            }

            IEnumerator<Wait> OnEvent3() {
                yield return new Wait(event3);
                counter++;
            }
        }

        [Test]
        public void TestPriority() {
            var myEvent = new Event();
            var counterShouldExecuteBefore0 = 0;

            IEnumerator<Wait> ShouldExecuteBefore0() {
                while (true) {
                    yield return new Wait(myEvent);
                    counterShouldExecuteBefore0++;
                }
            }

            var counterShouldExecuteBefore1 = 0;

            IEnumerator<Wait> ShouldExecuteBefore1() {
                while (true) {
                    yield return new Wait(myEvent);
                    counterShouldExecuteBefore1++;
                }
            }

            var counterShouldExecuteAfter = 0;

            IEnumerator<Wait> ShouldExecuteAfter() {
                while (true) {
                    yield return new Wait(myEvent);
                    if (counterShouldExecuteBefore0 == 1 &&
                        counterShouldExecuteBefore1 == 1) {
                        counterShouldExecuteAfter++;
                    }
                }
            }

            var counterShouldExecuteFinally = 0;

            IEnumerator<Wait> ShouldExecuteFinally() {
                while (true) {
                    yield return new Wait(myEvent);
                    if (counterShouldExecuteAfter > 0) {
                        counterShouldExecuteFinally++;
                    }
                }
            }

            var highPriority = int.MaxValue;
            CoroutineHandler.Start(ShouldExecuteBefore1(), priority: highPriority);
            CoroutineHandler.Start(ShouldExecuteAfter());
            CoroutineHandler.Start(ShouldExecuteBefore0(), priority: highPriority);
            CoroutineHandler.Start(ShouldExecuteFinally(), priority: -1);
            CoroutineHandler.Tick(1);
            CoroutineHandler.RaiseEvent(myEvent);
            Assert.AreEqual(1, counterShouldExecuteAfter, $"ShouldExecuteAfter counter  {counterShouldExecuteAfter} is invalid.");
            Assert.AreEqual(1, counterShouldExecuteFinally, $"ShouldExecuteFinally counter  {counterShouldExecuteFinally} is invalid.");
        }

        [Test]
        public void InvokeLaterAndNameTest() {
            var myEvent = new Event();
            var counter = 0;
            var cr = CoroutineHandler.InvokeLater(new Wait(myEvent), () => {
                counter++;
            }, "Bird");

            CoroutineHandler.InvokeLater(new Wait(myEvent), () => {
                counter++;
            });

            CoroutineHandler.InvokeLater(new Wait(myEvent), () => {
                counter++;
            });

            Assert.AreEqual(0, counter, "Incorrect counter value after 5 seconds.");
            CoroutineHandler.Tick(1);
            CoroutineHandler.RaiseEvent(myEvent);
            Assert.AreEqual(3, counter, "Incorrect counter value after 10 seconds.");
            Assert.AreEqual(true, cr.IsFinished, "Incorrect IsFinished value.");
            Assert.AreEqual(false, cr.WasCanceled, "Incorrect IsCanceled value.");
            Assert.AreEqual(cr.MoveNextCount, 2, "Incorrect MoveNextCount value.");
            Assert.AreEqual(cr.Name, "Bird", "Incorrect name of the coroutine.");
        }

    }
}