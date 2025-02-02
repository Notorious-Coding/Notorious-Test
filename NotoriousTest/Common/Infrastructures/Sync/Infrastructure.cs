namespace NotoriousTest.Common.Infrastructures.Sync
{
    public abstract class Infrastructure : IDisposable
    {
        public bool AutoReset { get; private set; } = true;
        public Infrastructure(bool initialize = false, bool autoreset = false)
        {
            AutoReset = autoreset;
            if (initialize) Initialize();
        }

        public abstract int Order { get; }
        public abstract void Initialize();
        public abstract void Reset();
        public abstract void Destroy();

        public void Dispose()
        {
            Destroy();
        }

        ~Infrastructure()
        {
            Destroy();
        }
    }


}
