namespace NotoriousTest.Common.Infrastructures.Async
{
    public abstract class AsyncConfiguredInfrastructure<TConfig> : AsyncInfrastructure where TConfig: new()
    {
        public TConfig Configuration { get; set; } = new();
        public AsyncConfiguredInfrastructure(bool initialize = false) : base(initialize)
        {

        }
    }

    public abstract class AsyncConfiguredInfrastructure : AsyncConfiguredInfrastructure<Dictionary<string, string>>
    {
        public AsyncConfiguredInfrastructure(bool initialize = false) : base(initialize)
        {

        }
    }
}
