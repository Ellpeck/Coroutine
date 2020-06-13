using System;

namespace Coroutine {
    /// <summary>
    /// Represents either an amount of time, or an <see cref="Event"/> that is being waited for by an <see cref="ActiveCoroutine"/>.
    /// </summary>
    public struct Wait {

        private readonly Event evt;
        private double seconds;

        /// <summary>
        /// Creates a new wait that waits for the given <see cref="Event"/>.
        /// </summary>
        /// <param name="evt">The event to wait for</param>
        public Wait(Event evt) {
            this.evt = evt;
            this.seconds = 0;
        }

        /// <summary>
        /// Creates a new wait that waits for the given amount of seconds.
        /// </summary>
        /// <param name="seconds">The amount of seconds to wait for</param>
        public Wait(double seconds) {
            this.seconds = seconds;
            this.evt = null;
        }

        /// <summary>
        /// Creates a new wait that waits for the given <see cref="TimeSpan"/>.
        /// Note that the exact value may be slightly different, since waits operate in <see cref="TimeSpan.TotalSeconds"/> rather than ticks.
        /// </summary>
        /// <param name="time">The time span to wait for</param>
        public Wait(TimeSpan time) : this(time.TotalSeconds) {
        }

        internal bool Tick(double deltaSeconds) {
            this.seconds -= deltaSeconds;
            return this.seconds <= 0;
        }

        internal bool OnEvent(Event evt) {
            return evt == this.evt;
        }

        internal bool IsWaitingForEvent() {
            return this.evt != null;
        }

    }
}