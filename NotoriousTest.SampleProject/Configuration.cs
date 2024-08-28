namespace NotoriousTest.SampleProject
{
    public class Configuration
    {
        public DatabaseConfiguration DatabaseConfiguration { get; set; } = new DatabaseConfiguration();
    }

    public class DatabaseConfiguration
    {
        public string ConnectionString { get; set; }
    }
}
