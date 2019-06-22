using System.Collections.Generic;

namespace Coroutine {
    internal class Coroutine {

        private readonly IEnumerator<Wait> enumerator;

        public Coroutine(IEnumerator<Wait> enumerator) {
            this.enumerator = enumerator;
            this.enumerator.MoveNext();
        }

        public bool Tick(double deltaSeconds) {
            var curr = this.enumerator.Current;
            if (curr != null && curr.Tick(deltaSeconds)) {
                if (!this.enumerator.MoveNext())
                    return true;
            }
            return false;
        }

        public bool OnEvent(Event evt) {
            var curr = this.enumerator.Current;
            if (curr != null && curr.OnEvent(evt)) {
                if (!this.enumerator.MoveNext())
                    return true;
            }
            return false;
        }

        public WaitType GetCurrentType() {
            return this.enumerator.Current.GetWaitType();
        }

    }
}