using System;
using System.Collections.Generic;
using System.Linq;

namespace Coroutine {
    public class CoroutineHandlerInstance {

        private readonly List<ActiveCoroutine> tickingCoroutines = new List<ActiveCoroutine>();
        private readonly List<ActiveCoroutine> eventCoroutines = new List<ActiveCoroutine>();

        public ActiveCoroutine Start(IEnumerable<Wait> coroutine) {
            return this.Start(coroutine.GetEnumerator());
        }

        public ActiveCoroutine Start(IEnumerator<Wait> coroutine) {
            var inst = new ActiveCoroutine(coroutine);
            if (inst.MoveNext()) {
                if (inst.IsWaitingForEvent()) {
                    this.eventCoroutines.Add(inst);
                } else {
                    this.tickingCoroutines.Add(inst);
                }
            }
            return inst;
        }

        public void InvokeLater(Wait wait, Action action) {
            this.Start(InvokeLaterImpl(wait, action));
        }

        public void Tick(double deltaSeconds) {
            for (var i = this.tickingCoroutines.Count - 1; i >= 0; i--) {
                var coroutine = this.tickingCoroutines[i];
                if (coroutine.Tick(deltaSeconds)) {
                    this.tickingCoroutines.RemoveAt(i);
                } else if (coroutine.IsWaitingForEvent()) {
                    this.tickingCoroutines.RemoveAt(i);
                    this.eventCoroutines.Add(coroutine);
                }
            }
        }

        public void RaiseEvent(Event evt) {
            for (var i = this.eventCoroutines.Count - 1; i >= 0; i--) {
                var coroutine = this.eventCoroutines[i];
                if (coroutine.OnEvent(evt)) {
                    this.eventCoroutines.RemoveAt(i);
                } else if (!coroutine.IsWaitingForEvent()) {
                    this.eventCoroutines.RemoveAt(i);
                    this.tickingCoroutines.Add(coroutine);
                }
            }
        }

        public IEnumerable<ActiveCoroutine> GetActiveCoroutines() {
            return this.tickingCoroutines.Concat(this.eventCoroutines);
        }

        private static IEnumerator<Wait> InvokeLaterImpl(Wait wait, Action action) {
            yield return wait;
            action();
        }

    }
}