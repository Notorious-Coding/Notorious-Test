namespace NotoriousTest.Infrastructures
{
    public abstract class Infrastructure : IDisposable
    {
        public abstract int Order { get; }
        public abstract void Initialize();
        public abstract void Reset();
        public abstract void Destroy();

        public void Dispose()
        {
            Destroy();
        }
    }
}
