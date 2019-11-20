using System;

namespace Coroutine {
    public struct WaitEvent : IWait {

        private readonly Event evt;

        public WaitEvent(Event evt) {
            this.evt = evt;
        }

        public WaitType GetWaitType() {
            return WaitType.Event;
        }

        public bool Tick(double deltaSeconds) {
            throw new NotSupportedException();
        }

        public bool OnEvent(Event evt) {
            return evt == this.evt;
        }

    }
}