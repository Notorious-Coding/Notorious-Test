namespace NotoriousTest.Core.Infrastructures.Cleaner
{

    [AttributeUsage(AttributeTargets.Class)]
    public class InfrastructureCleanerAttribute(Type InfrastructureType) : Attribute
    {
    }
}
