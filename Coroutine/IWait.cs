namespace Coroutine {
    public interface IWait {

        WaitType GetWaitType();

        bool Tick(double deltaSeconds);

        bool OnEvent(Event evt);

    }

    public enum WaitType {

        Tick,
        Event

    }
}