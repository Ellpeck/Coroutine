using System;
using System.Collections.Generic;

namespace Coroutine {
    public class ActiveCoroutine {

        private readonly IEnumerator<Wait> enumerator;
        private Wait current;

        public bool IsFinished { get; private set; }
        public bool WasCanceled { get; private set; }
        public FinishCallback OnFinished;

        internal ActiveCoroutine(IEnumerator<Wait> enumerator) {
            this.enumerator = enumerator;
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
                if (this.current.Tick(deltaSeconds))
                    this.MoveNext();
            }
            return this.IsFinished;
        }

        internal bool OnEvent(Event evt) {
            if (!this.WasCanceled) {
                if (this.current.OnEvent(evt))
                    this.MoveNext();
            }
            return this.IsFinished;
        }

        internal bool MoveNext() {
            if (!this.enumerator.MoveNext()) {
                this.IsFinished = true;
                this.OnFinished?.Invoke(this);
                return false;
            }
            this.current = this.enumerator.Current;
            return true;
        }

        internal bool IsWaitingForEvent() {
            return this.current.IsWaitingForEvent();
        }

        public delegate void FinishCallback(ActiveCoroutine coroutine);

    }
}