using System;
using System.Collections.Generic;
using System.Linq;

namespace Coroutine {
    public static class CoroutineHandler {

        private static readonly CoroutineHandlerInstance Instance = new CoroutineHandlerInstance();

        public static ActiveCoroutine Start(IEnumerable<Wait> coroutine) {
            return Instance.Start(coroutine);
        }

        public static ActiveCoroutine Start(IEnumerator<Wait> coroutine) {
            return Instance.Start(coroutine);
        }

        public static void InvokeLater(Wait wait, Action action) {
            Instance.InvokeLater(wait, action);
        }

        public static void Tick(double deltaSeconds) {
            Instance.Tick(deltaSeconds);
        }

        public static void RaiseEvent(Event evt) {
            Instance.RaiseEvent(evt);
        }

        public static IEnumerable<ActiveCoroutine> GetActiveCoroutines() {
            return Instance.GetActiveCoroutines();
        }

    }
}