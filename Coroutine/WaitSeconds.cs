namespace Coroutine {
    public class WaitSeconds : Wait {

        private double seconds;

        public WaitSeconds(double seconds) {
            this.seconds = seconds;
        }

        public override WaitType GetWaitType() {
            return WaitType.Tick;
        }

        public override bool Tick(double deltaSeconds) {
            this.seconds -= deltaSeconds;
            return this.seconds <= 0;
        }

    }
}