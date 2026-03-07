using NotoriousTest.Infrastructures.Async;

using System.Reflection;

namespace NotoriousTest.DoggyDog
{
    internal class InfrastructureLoader
    {
        public static IEnumerable<Type> LoadExternalInfrastructure(Assembly assembly)
        {
            IEnumerable<Type> asyncTypes = assembly.GetTypes()
                                                    .Where(t => IsExternalInfrastructure(t) && !t.IsAbstract);
            Console.WriteLine("Async Infrastructures found: {0}", string.Join("|", asyncTypes.Select(t => t.Name).ToArray()));
            return asyncTypes;
        }


        static bool IsExternalInfrastructure(Type t)
        {
            while (t != null)
            {
                if (t.IsGenericType && (t.GetGenericTypeDefinition() == typeof(ExternalInfrastructure<,>) || t.GetGenericTypeDefinition() == typeof(ExternalAsyncInfrastructure<,>)))
                    return true;
                t = t.BaseType;
            }
            return false;
        }
    }
}
