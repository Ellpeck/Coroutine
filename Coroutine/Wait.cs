using System;

namespace Coroutine {
    public abstract class Wait {

        public abstract WaitType GetWaitType();

        public virtual bool Tick(double deltaSeconds) {
            throw new NotSupportedException();
        }

        public virtual bool OnEvent(Event evt) {
            throw new NotSupportedException();
        }

    }

    public enum WaitType {

        Tick,
        Event

    }
}