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
            CoroutineHandler.RaiseEvent(myEvent);
            Assert.AreEqual(2, counter, "instruction after yield is not executed.");
            CoroutineHandler.RaiseEvent(myEvent);
            Assert.AreEqual(2, counter, "instruction after yield is not executed.");

            Assert.AreEqual(true, cr.IsFinished, "Incorrect IsFinished value.");
            Assert.AreEqual(false, cr.WasCanceled, "Incorrect IsCanceled value.");
            Assert.AreEqual(cr.MoveNextCount, 2, "Incorrect MoveNextCount value.");
        }

    }
}