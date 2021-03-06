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

        /// <inheritdoc cref="CoroutineHandlerInstance.Start(IEnumerable{Wait},string,int)"/>
        public static ActiveCoroutine Start(IEnumerable<Wait> coroutine, string name = "", int priority = 0) {
            return Instance.Start(coroutine, name, priority);
        }

        /// <inheritdoc cref="CoroutineHandlerInstance.Start(IEnumerator{Wait},string,int)"/>
        public static ActiveCoroutine Start(IEnumerator<Wait> coroutine, string name = "", int priority = 0) {
            return Instance.Start(coroutine, name, priority);
        }

        /// <inheritdoc cref="CoroutineHandlerInstance.InvokeLater"/>
        public static ActiveCoroutine InvokeLater(Wait wait, Action action, string name = "", int priority = 0) {
            return Instance.InvokeLater(wait, action, name, priority);
        }

        /// <inheritdoc cref="CoroutineHandlerInstance.Tick(double)"/>
        public static void Tick(double deltaSeconds) {
            Instance.Tick(deltaSeconds);
        }
        /// <inheritdoc cref="CoroutineHandlerInstance.Tick(TimeSpan)"/>
        public static void Tick(TimeSpan delta) {
            Instance.Tick(delta);
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