namespace NotoriousTest.Core.Infrastructures
{
    public interface IInfrastructure
    {
        bool AutoReset { get; set; }
        ContextId ContextId { get; set; }
        int? Order { get; }

        Task Destroy();
        Task Initialize();
        Task Reset();
    }
}