namespace NotoriousTest.Core.Registry
{
    public class InfrastuctureRegistryEntry
    {
        public Guid InfrastructureId { get; set; }
        public Type InfrastructureType { get; set; }
        public Guid EnvironmentId { get; set; }
        public int ProcessPID { get; set; }
        public object? Metadata { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}
