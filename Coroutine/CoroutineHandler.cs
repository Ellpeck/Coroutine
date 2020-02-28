using System;
using System.Collections.Generic;

namespace Coroutine {
    public static class CoroutineHandler {

        private static readonly List<ActiveCoroutine> TickingCoroutines = new List<ActiveCoroutine>();
        private static readonly List<ActiveCoroutine> EventCoroutines = new List<ActiveCoroutine>();

        public static ActiveCoroutine Start(IEnumerator<IWait> coroutine) {
            var inst = new ActiveCoroutine(coroutine);
            var type = inst.GetCurrentType();
            if (type == WaitType.Tick)
                TickingCoroutines.Add(inst);
            else if (type == WaitType.Event)
                EventCoroutines.Add(inst);
            return inst;
        }

        public static void InvokeLater(IWait wait, Action action) {
            Start(InvokeLaterImpl(wait, action));
        }

        public static void Tick(double deltaSeconds) {
            for (var i = TickingCoroutines.Count - 1; i >= 0; i--) {
                var coroutine = TickingCoroutines[i];
                if (coroutine.Tick(deltaSeconds)) {
                    TickingCoroutines.RemoveAt(i);
                } else if (coroutine.GetCurrentType() != WaitType.Tick) {
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
                } else if (coroutine.GetCurrentType() != WaitType.Event) {
                    EventCoroutines.RemoveAt(i);
                    TickingCoroutines.Add(coroutine);
                }
            }
        }

        private static IEnumerator<IWait> InvokeLaterImpl(IWait wait, Action action) {
            yield return wait;
            action();
        }

    }
}