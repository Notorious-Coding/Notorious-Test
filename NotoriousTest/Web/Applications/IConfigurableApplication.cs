namespace NotoriousTest.Web.Applications
{
    public interface IConfigurableApplication
    {
        public Dictionary<string, string>? Configuration { get; set; } 
    }
}
