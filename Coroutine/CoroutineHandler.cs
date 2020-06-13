using System;
using System.Collections.Generic;
using System.Linq;

namespace Coroutine {
    /// <summary>
    /// This class can be used for static coroutine handling of any kind.
    /// Note that it uses an underlying <see cref="CoroutineHandlerInstance"/> object for management.
    /// </summary>
    public static class CoroutineHandler {

        private static readonly CoroutineHandlerInstance Instance = new CoroutineHandlerInstance();

        /// <inheritdoc cref="CoroutineHandlerInstance.Start(IEnumerable{Wait})"/>
        public static ActiveCoroutine Start(IEnumerable<Wait> coroutine) {
            return Instance.Start(coroutine);
        }

        /// <inheritdoc cref="CoroutineHandlerInstance.Start(IEnumerator{Wait})"/>
        public static ActiveCoroutine Start(IEnumerator<Wait> coroutine) {
            return Instance.Start(coroutine);
        }

        /// <inheritdoc cref="CoroutineHandlerInstance.InvokeLater"/>
        public static void InvokeLater(Wait wait, Action action) {
            Instance.InvokeLater(wait, action);
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