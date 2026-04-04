namespace NotoriousTest.Core.Infrastructures
{
    public interface IInfrastructure
    {
        bool AutoReset { get; set; }
        EnvironmentId EnvironmentId { get; set; }
        int? Order { get; }

        Task Destroy();
        Task Initialize();
        Task Reset();
    }
}