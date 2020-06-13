using System;

namespace Coroutine {
    public struct Wait {

        private readonly Event evt;
        private double seconds;

        public Wait(Event evt) {
            this.evt = evt;
            this.seconds = 0;
        }

        public Wait(double seconds) {
            this.seconds = seconds;
            this.evt = null;
        }

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