namespace NotoriousTest.SampleProject
{
    public class Configuration
    {
        public DatabaseConfiguration DatabaseConfiguration = new DatabaseConfiguration();
    }

    public class DatabaseConfiguration
    {
        public string ConnectionString { get; set; }
    }
}
