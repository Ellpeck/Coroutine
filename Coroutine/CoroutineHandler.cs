using System;
using System.Collections.Generic;

namespace Coroutine {
    /// <summary>
    /// This class can be used for static coroutine handling of any kind.
    /// Note that it uses an underlying <see cref="CoroutineHandlerInstance"/> object for management.
    /// </summary>
    public static class CoroutineHandler {

        private static readonly CoroutineHandlerInstance Instance = new CoroutineHandlerInstance();

        /// <inheritdoc cref="CoroutineHandlerInstance.TickingCount"/>
        public static int TickingCount => Instance.TickingCount;
        /// <inheritdoc cref="CoroutineHandlerInstance.EventCount"/>
        public static int EventCount => Instance.EventCount;

        /// <inheritdoc cref="CoroutineHandlerInstance.Start(IEnumerable{Wait},string)"/>
        public static ActiveCoroutine Start(IEnumerable<Wait> coroutine, string name = "") {
            return Instance.Start(coroutine, name);
        }

        /// <inheritdoc cref="CoroutineHandlerInstance.Start(IEnumerator{Wait},string)"/>
        public static ActiveCoroutine Start(IEnumerator<Wait> coroutine, string name = "") {
            return Instance.Start(coroutine, name);
        }

        /// <inheritdoc cref="CoroutineHandlerInstance.InvokeLater"/>
        public static ActiveCoroutine InvokeLater(Wait wait, Action action) {
            return Instance.InvokeLater(wait, action);
        }

        /// <inheritdoc cref="CoroutineHandlerInstance.Tick"/>
        public static void Tick(double deltaSeconds) {
            Instance.Tick(deltaSeconds);
        }

        /// <inheritdoc cref="CoroutineHandlerInstance.RaiseEvent"/>
        public static void RaiseEvent(Event evt) {
            Instance.RaiseEvent(evt);
        }

        /// <inheritdoc cref="CoroutineHandlerInstance.GetActiveCoroutines"/>
        public static IEnumerable<ActiveCoroutine> GetActiveCoroutines() {
            return Instance.GetActiveCoroutines();
        }

    }
}