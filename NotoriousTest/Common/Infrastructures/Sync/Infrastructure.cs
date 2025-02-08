using NotoriousTest.Common.Infrastructures.Common;

namespace NotoriousTest.Common.Infrastructures.Sync
{
    /// <summary>
    /// Infrastructure is a base class to define a test infrastructure.
    /// </summary>
    public abstract class Infrastructure : IInfrastructure, IDisposable
    {

        public bool AutoReset { get; set; } = true;

        public virtual int? Order { get; }

        public Guid ContextId { get; set; } = Guid.NewGuid();

        public Infrastructure(bool initialize = false)
        {
            if (initialize) Initialize();
        }

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
