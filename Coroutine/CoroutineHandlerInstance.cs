using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Coroutine {
    /// <summary>
    /// An object of this class can be used to start, tick and otherwise manage active <see cref="ActiveCoroutine"/>s as well as their <see cref="Event"/>s.
    /// Note that a static implementation of this can be found in <see cref="CoroutineHandler"/>.
    /// </summary>
    public class CoroutineHandlerInstance {

        private readonly List<ActiveCoroutine> tickingCoroutines = new List<ActiveCoroutine>();
        private readonly List<ActiveCoroutine> eventCoroutines = new List<ActiveCoroutine>();
        private readonly Queue<ActiveCoroutine> outstandingCoroutines = new Queue<ActiveCoroutine>();
        private readonly ISet<int> eventCoroutinesToRemove = new HashSet<int>();
        private readonly Stopwatch stopwatch = new Stopwatch();

        /// <summary>
        /// The amount of <see cref="ActiveCoroutine"/> instances that are currently waiting for a tick (waiting for time to pass)
        /// </summary>
        public int TickingCount => this.tickingCoroutines.Count;
        /// <summary>
        /// The amount of <see cref="ActiveCoroutine"/> instances that are currently waiting for an <see cref="Event"/>
        /// </summary>
        public int EventCount => this.eventCoroutines.Count;

        /// <summary>
        /// Starts the given coroutine, returning a <see cref="ActiveCoroutine"/> object for management.
        /// Note that this calls <see cref="IEnumerable{T}.GetEnumerator"/> to get the enumerator.
        /// </summary>
        /// <param name="coroutine">The coroutine to start</param>
        /// <param name="name">The <see cref="ActiveCoroutine.Name"/> that this coroutine should have. Defaults to an empty string.</param>
        /// <param name="priority">The <see cref="ActiveCoroutine.Priority"/> that this coroutine should have. The higher the priority, the earlier it is advanced. Defaults to 0.</param>
        /// <returns>An active coroutine object representing this coroutine</returns>
        public ActiveCoroutine Start(IEnumerable<Wait> coroutine, string name = "", int priority = 0) {
            return this.Start(coroutine.GetEnumerator(), name, priority);
        }

        /// <summary>
        /// Starts the given coroutine, returning a <see cref="ActiveCoroutine"/> object for management.
        /// </summary>
        /// <param name="coroutine">The coroutine to start</param>
        /// <param name="name">The <see cref="ActiveCoroutine.Name"/> that this coroutine should have. Defaults to an empty string.</param>
        /// <param name="priority">The <see cref="ActiveCoroutine.Priority"/> that this coroutine should have. The higher the priority, the earlier it is advanced compared to other coroutines that advance around the same time. Defaults to 0.</param>
        /// <returns>An active coroutine object representing this coroutine</returns>
        public ActiveCoroutine Start(IEnumerator<Wait> coroutine, string name = "", int priority = 0) {
            var inst = new ActiveCoroutine(coroutine, name, priority, this.stopwatch);
            if (inst.MoveNext())
                this.outstandingCoroutines.Enqueue(inst);
            return inst;
        }

        /// <summary>
        /// Causes the given action to be invoked after the given <see cref="Wait"/>.
        /// This is equivalent to a coroutine that waits for the given wait and then executes the given <see cref="Action"/>.
        /// </summary>
        /// <param name="wait">The wait to wait for</param>
        /// <param name="action">The action to execute after waiting</param>
        /// <param name="name">The <see cref="ActiveCoroutine.Name"/> that the underlying coroutine should have. Defaults to an empty string.</param>
        /// <param name="priority">The <see cref="ActiveCoroutine.Priority"/> that the underlying coroutine should have. The higher the priority, the earlier it is advanced compared to other coroutines that advance around the same time. Defaults to 0.</param>
        /// <returns>An active coroutine object representing this coroutine</returns>
        public ActiveCoroutine InvokeLater(Wait wait, Action action, string name = "", int priority = 0) {
            return this.Start(InvokeLaterImpl(wait, action), name, priority);
        }

        /// <summary>
        /// Ticks this coroutine handler, causing all time-based <see cref="Wait"/>s to be ticked.
        /// Note that this method needs to be called even if only event-based coroutines are used.
        /// </summary>
        /// <param name="deltaSeconds">The amount of seconds that have passed since the last time this method was invoked</param>
        public void Tick(double deltaSeconds) {
            this.MoveOutstandingCoroutines();
            this.tickingCoroutines.RemoveAll(c => {
                if (c.Tick(deltaSeconds)) {
                    return true;
                } else if (c.IsWaitingForEvent()) {
                    this.outstandingCoroutines.Enqueue(c);
                    return true;
                }
                return false;
            });
        }

        /// <summary>
        /// Ticks this coroutine handler, causing all time-based <see cref="Wait"/>s to be ticked.
        /// This is a convenience method that calls <see cref="Tick(double)"/>, but accepts a <see cref="TimeSpan"/> instead of an amount of seconds.
        /// </summary>
        /// <param name="delta">The time that has passed since the last time this method was invoked</param>
        public void Tick(TimeSpan delta) {
            this.Tick(delta.TotalSeconds);
        }

        /// <summary>
        /// Raises the given event, causing all event-based <see cref="Wait"/>s to be updated.
        /// </summary>
        /// <param name="evt">The event to raise</param>
        public void RaiseEvent(Event evt) {
            for (var i = 0; i < this.eventCoroutines.Count; i++) {
                var c = this.eventCoroutines[i];
                if (c.OnEvent(evt)) {
                    this.eventCoroutinesToRemove.Add(i);
                } else if (!c.IsWaitingForEvent()) {
                    this.outstandingCoroutines.Enqueue(c);
                    this.eventCoroutinesToRemove.Add(i);
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

        private void MoveOutstandingCoroutines() {
            var i = 0;
            this.eventCoroutines.RemoveAll(c => this.eventCoroutinesToRemove.Contains(i++));
            this.eventCoroutinesToRemove.Clear();

            while (this.outstandingCoroutines.Count > 0) {
                var coroutine = this.outstandingCoroutines.Dequeue();
                var list = coroutine.IsWaitingForEvent() ? this.eventCoroutines : this.tickingCoroutines;
                var position = list.BinarySearch(coroutine);
                list.Insert(position < 0 ? ~position : position, coroutine);
            }
        }

        private static IEnumerator<Wait> InvokeLaterImpl(Wait wait, Action action) {
            yield return wait;
            action();
        }

    }
}