namespace NotoriousTest.Infrastructures
{
    public interface IInfrastructure
    {
        bool AutoReset { get; set; }
        Guid ContextId { get; set; }
        int? Order { get; }

        Task Destroy();
        Task Initialize();
        Task Reset();
    }
}