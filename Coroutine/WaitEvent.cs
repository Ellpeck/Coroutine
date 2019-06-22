namespace Coroutine {
    public class WaitEvent : Wait {

        private readonly Event evt;

        public WaitEvent(Event evt) {
            this.evt = evt;
        }

        public override WaitType GetWaitType() {
            return WaitType.Event;
        }

        public override bool OnEvent(Event evt) {
            return evt == this.evt;
        }

    }
}