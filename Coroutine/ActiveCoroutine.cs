using System;
using System.Collections.Generic;

namespace Coroutine {
    /// <summary>
    /// A reference to a currently running coroutine.
    /// This is returned by <see cref="CoroutineHandler.Start(IEnumerator{Wait})"/>.
    /// </summary>
    public class ActiveCoroutine {

        private readonly IEnumerator<Wait> enumerator;
        private Wait current;

        /// <summary>
        /// This property stores whether or not this active coroutine is finished.
        /// A coroutine is finished if all of its waits have passed, or if it <see cref="WasCanceled"/>.
        /// </summary>
        public bool IsFinished { get; private set; }
        /// <summary>
        /// This property stores whether or not this active coroutine was cancelled using <see cref="Cancel"/>.
        /// </summary>
        public bool WasCanceled { get; private set; }
        /// <summary>
        /// An event that gets fired when this active coroutine finishes or gets cancelled.
        /// When this event is called, <see cref="IsFinished"/> is always true.
        /// </summary>
        public event FinishCallback OnFinished;

        internal ActiveCoroutine(IEnumerator<Wait> enumerator) {
            this.enumerator = enumerator;
        }

        /// <summary>
        /// Cancels this coroutine, causing all subsequent <see cref="Wait"/>s and any code in between to be skipped.
        /// </summary>
        /// <returns>Whether the cancellation was successful, or this coroutine was already cancelled or finished</returns>
        public bool Cancel() {
            if (this.IsFinished || this.WasCanceled)
                return false;
            this.WasCanceled = true;
            this.IsFinished = true;
            this.OnFinished?.Invoke(this);
            return true;
        }

        internal bool Tick(double deltaSeconds) {
            if (!this.WasCanceled) {
                if (this.current.Tick(deltaSeconds))
                    this.MoveNext();
            }
            return this.IsFinished;
        }

        internal bool OnEvent(Event evt) {
            if (!this.WasCanceled) {
                if (this.current.OnEvent(evt))
                    this.MoveNext();
            }
            return this.IsFinished;
        }

        internal bool MoveNext() {
            if (!this.enumerator.MoveNext()) {
                this.IsFinished = true;
                this.OnFinished?.Invoke(this);
                return false;
            }
            this.current = this.enumerator.Current;
            return true;
        }

        internal bool IsWaitingForEvent() {
            return this.current.IsWaitingForEvent();
        }

        /// <summary>
        /// A delegate method used by <see cref="ActiveCoroutine.OnFinished"/>.
        /// </summary>
        /// <param name="coroutine">The coroutine that finished</param>
        public delegate void FinishCallback(ActiveCoroutine coroutine);

    }
}