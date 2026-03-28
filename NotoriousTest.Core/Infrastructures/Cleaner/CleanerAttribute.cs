namespace NotoriousTest.Core.Infrastructures.Cleaner
{

    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class CleanerAttribute(Type CleanerType) : Attribute
    {
        public Type CleanerType { get; } = CleanerType;
    }
}
