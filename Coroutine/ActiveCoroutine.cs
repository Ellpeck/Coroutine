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
                var curr = this.enumerator.Current;
                if (curr != null && curr.Tick(deltaSeconds))
                    this.MoveNext();
            }
            return this.IsFinished;
        }

        internal bool OnEvent(Event evt) {
            if (!this.WasCanceled) {
                var curr = this.enumerator.Current;
                if (curr != null && curr.OnEvent(evt))
                    this.MoveNext();
            }
            return this.IsFinished;
        }

        private void MoveNext() {
            if (!this.enumerator.MoveNext()) {
                this.IsFinished = true;
                this.OnFinished?.Invoke(this);
            }
        }

        internal WaitType GetCurrentType() {
            return this.enumerator.Current.GetWaitType();
        }

        public delegate void FinishCallback(ActiveCoroutine coroutine);

    }
}