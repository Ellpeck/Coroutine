using System;
using System.Collections.Generic;
using System.Linq;

namespace Coroutine {
    /// <summary>
    /// An object of this class can be used to start, tick and otherwise manage active <see cref="ActiveCoroutine"/>s as well as their <see cref="Event"/>s.
    /// Note that a static implementation of this can be found in <see cref="CoroutineHandler"/>.
    /// </summary>
    public class CoroutineHandlerInstance {

        private readonly List<ActiveCoroutine> tickingCoroutines = new List<ActiveCoroutine>();
        private readonly List<ActiveCoroutine> eventCoroutines = new List<ActiveCoroutine>();

        /// <summary>
        /// Starts the given coroutine, returning a <see cref="ActiveCoroutine"/> object for management.
        /// Note that this calls <see cref="IEnumerable{T}.GetEnumerator"/> to get the enumerator.
        /// </summary>
        /// <param name="coroutine">The coroutine to start</param>
        /// <returns>An active coroutine object representing this coroutine</returns>
        public ActiveCoroutine Start(IEnumerable<Wait> coroutine) {
            return this.Start(coroutine.GetEnumerator());
        }

        /// <summary>
        /// Starts the given coroutine, returning a <see cref="ActiveCoroutine"/> object for management.
        /// </summary>
        /// <param name="coroutine">The coroutine to start</param>
        /// <returns>An active coroutine object representing this coroutine</returns>
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

        /// <summary>
        /// Causes the given action to be invoked after the given <see cref="Wait"/>.
        /// This is equivalent to a coroutine that waits for the given wait and then executes the given <see cref="Action"/>.
        /// </summary>
        /// <param name="wait">The wait to wait for</param>
        /// <param name="action">The action to execute after waiting</param>
        public void InvokeLater(Wait wait, Action action) {
            this.Start(InvokeLaterImpl(wait, action));
        }

        /// <summary>
        /// Ticks this coroutine handler, causing all time-based <see cref="Wait"/>s to be ticked.
        /// </summary>
        /// <param name="deltaSeconds">The amount of seconds that have passed since the last time this method was invoked</param>
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

        /// <summary>
        /// Raises the given event, causing all event-based <see cref="Wait"/>s to be updated.
        /// </summary>
        /// <param name="evt">The event to raise</param>
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

        /// <summary>
        /// Returns a list of all currently active <see cref="ActiveCoroutine"/> objects under this handler.
        /// </summary>
        /// <returns>All active coroutines</returns>
        public IEnumerable<ActiveCoroutine> GetActiveCoroutines() {
            return this.tickingCoroutines.Concat(this.eventCoroutines);
        }

        private static IEnumerator<Wait> InvokeLaterImpl(Wait wait, Action action) {
            yield return wait;
            action();
        }

    }
}