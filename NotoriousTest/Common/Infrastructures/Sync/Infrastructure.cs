using NotoriousTest.Common.Helpers;

namespace NotoriousTest.Common.Infrastructures.Sync
{
    public abstract class Infrastructure : IDisposable
    {
        public Dictionary<string, string> Configuration { get; protected set; } = new Dictionary<string, string>();

        public Infrastructure(bool initialize = false)
        {
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
