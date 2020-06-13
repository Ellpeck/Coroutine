using System;
using System.Collections.Generic;
using System.Linq;

namespace Coroutine {
    public static class CoroutineHandler {

        private static readonly List<ActiveCoroutine> TickingCoroutines = new List<ActiveCoroutine>();
        private static readonly List<ActiveCoroutine> EventCoroutines = new List<ActiveCoroutine>();

        public static ActiveCoroutine Start(IEnumerable<Wait> coroutine) {
            return Start(coroutine.GetEnumerator());
        }

        public static ActiveCoroutine Start(IEnumerator<Wait> coroutine) {
            var inst = new ActiveCoroutine(coroutine);
            if (inst.MoveNext()) {
                if (inst.IsWaitingForEvent()) {
                    EventCoroutines.Add(inst);
                } else {
                    TickingCoroutines.Add(inst);
                }
            }
            return inst;
        }

        public static void InvokeLater(Wait wait, Action action) {
            Start(InvokeLaterImpl(wait, action));
        }

        public static void Tick(double deltaSeconds) {
            for (var i = TickingCoroutines.Count - 1; i >= 0; i--) {
                var coroutine = TickingCoroutines[i];
                if (coroutine.Tick(deltaSeconds)) {
                    TickingCoroutines.RemoveAt(i);
                } else if (coroutine.IsWaitingForEvent()) {
                    TickingCoroutines.RemoveAt(i);
                    EventCoroutines.Add(coroutine);
                }
            }
        }

        public static void RaiseEvent(Event evt) {
            for (var i = EventCoroutines.Count - 1; i >= 0; i--) {
                var coroutine = EventCoroutines[i];
                if (coroutine.OnEvent(evt)) {
                    EventCoroutines.RemoveAt(i);
                } else if (!coroutine.IsWaitingForEvent()) {
                    EventCoroutines.RemoveAt(i);
                    TickingCoroutines.Add(coroutine);
                }
            }
        }

        public static IEnumerable<ActiveCoroutine> GetActiveCoroutines() {
            return TickingCoroutines.Concat(EventCoroutines);
        }

        private static IEnumerator<Wait> InvokeLaterImpl(Wait wait, Action action) {
            yield return wait;
            action();
        }

    }
}