using System;

namespace Coroutine {
    public struct WaitSeconds : IWait {

        private double seconds;

        public WaitSeconds(double seconds) {
            this.seconds = seconds;
        }

        public WaitType GetWaitType() {
            return WaitType.Tick;
        }

        public bool Tick(double deltaSeconds) {
            this.seconds -= deltaSeconds;
            return this.seconds <= 0;
        }

        public bool OnEvent(Event evt) {
            throw new NotSupportedException();
        }

    }
}