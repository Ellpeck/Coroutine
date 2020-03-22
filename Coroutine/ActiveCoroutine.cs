using System;
using System.Collections.Generic;

namespace Coroutine {
    public class ActiveCoroutine {

        private readonly IEnumerator<IWait> enumerator;
        public bool IsFinished { get; private set; }
        public bool WasCanceled { get; private set; }
        public FinishCallback OnFinished;

        internal ActiveCoroutine(IEnumerator<IWait> enumerator) {
            this.enumerator = enumerator;
            this.enumerator.MoveNext();
        }

        internal bool Tick(double deltaSeconds) {
            var curr = this.enumerator.Current;
            if (curr != null && curr.Tick(deltaSeconds))
                this.MoveNext();
            return this.IsFinished;
        }

        internal bool OnEvent(Event evt) {
            var curr = this.enumerator.Current;
            if (curr != null && curr.OnEvent(evt))
                this.MoveNext();
            return this.IsFinished;
        }

        internal void Finish(bool cancel) {
            this.IsFinished = true;
            this.WasCanceled = cancel;
            this.OnFinished?.Invoke(this, cancel);
        }

        private void MoveNext() {
            if (!this.enumerator.MoveNext())
                this.Finish(false);
        }

        internal WaitType GetCurrentType() {
            return this.enumerator.Current.GetWaitType();
        }

        public delegate void FinishCallback(ActiveCoroutine coroutine, bool canceled);

    }
}