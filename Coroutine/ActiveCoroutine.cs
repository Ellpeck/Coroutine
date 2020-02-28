using System.Collections.Generic;

namespace Coroutine {
    public class ActiveCoroutine {

        private readonly IEnumerator<IWait> enumerator;
        public bool IsFinished { get; private set; }

        internal ActiveCoroutine(IEnumerator<IWait> enumerator) {
            this.enumerator = enumerator;
            this.enumerator.MoveNext();
        }

        internal bool Tick(double deltaSeconds) {
            var curr = this.enumerator.Current;
            if (curr != null && curr.Tick(deltaSeconds)) {
                if (!this.enumerator.MoveNext())
                    this.IsFinished = true;
            }
            return this.IsFinished;
        }

        internal bool OnEvent(Event evt) {
            var curr = this.enumerator.Current;
            if (curr != null && curr.OnEvent(evt)) {
                if (!this.enumerator.MoveNext())
                    this.IsFinished = true;
            }
            return this.IsFinished;
        }

        internal WaitType GetCurrentType() {
            return this.enumerator.Current.GetWaitType();
        }

    }
}