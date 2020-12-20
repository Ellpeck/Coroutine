using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Coroutine {
    /// <summary>
    /// A reference to a currently running coroutine.
    /// This is returned by <see cref="CoroutineHandler.Start(IEnumerator{Wait},string)"/>.
    /// </summary>
    public class ActiveCoroutine {

        private readonly IEnumerator<Wait> enumerator;
        private readonly Stopwatch stopwatch;
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
        /// The total amount of time that <see cref="MoveNext"/> took.
        /// This is the amount of time that this active coroutine took for the entirety of its "steps", or yield statements.
        /// </summary>
        public TimeSpan TotalMoveNextTime { get; private set; }
        /// <summary>
        /// The total amount of times that <see cref="MoveNext"/> was invoked.
        /// This is the amount of "steps" in your coroutine, or the amount of yield statements.
        /// </summary>
        public int MoveNextCount { get; private set; }
        /// <summary>
        /// The average amount of time that <see cref="MoveNext"/> took.
        /// This is the average amount of time that each "step", or each yield statement, of this coroutine took to execute.
        /// </summary>
        public TimeSpan AverageMoveNextTime => new TimeSpan(this.TotalMoveNextTime.Ticks / this.MoveNextCount);
        /// <summary>
        /// An event that gets fired when this active coroutine finishes or gets cancelled.
        /// When this event is called, <see cref="IsFinished"/> is always true.
        /// </summary>
        public event FinishCallback OnFinished;
        /// <summary>
        /// The name of this coroutine.
        /// When not specified on startup of this coroutine, the name defaults to an empty string.
        /// </summary>
        public readonly string Name;

        internal ActiveCoroutine(IEnumerator<Wait> enumerator, string name, Stopwatch stopwatch) {
            this.enumerator = enumerator;
            this.stopwatch = stopwatch;
            this.Name = name;
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
            this.stopwatch.Restart();
            var result = this.enumerator.MoveNext();
            this.stopwatch.Stop();
            this.TotalMoveNextTime += this.stopwatch.Elapsed;
            this.MoveNextCount++;

            if (!result) {
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