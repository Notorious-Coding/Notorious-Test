using System.Collections;
namespace NotoriousTest.Common.Helpers
{
    public static class ObjectExtensions
    {
        public static Dictionary<string, string> ToDictionary(
            this object obj,
            string name = "")
        {

            if (obj is Dictionary <string, string> dict)
            {
                return dict;
            }

            var dictionary = new Dictionary<string, string?>();

            Flatten(dictionary, obj, name);

            return dictionary;
        }

        private static void Flatten(
            IDictionary<string, string> dictionary,
            object? obj,
            string prefix)
        {
            if (obj == null)
            {
                dictionary.Add(prefix, null);

                return;
            }

            var objType = obj.GetType();

            if (objType.IsValueType || objType == typeof(string))
            {
                dictionary.Add(prefix, obj.ToString());
            }
            else if (obj is IEnumerable subObjects)
            {
                var counter = 0;

                foreach (var subObj in subObjects)
                {
                    Flatten(dictionary, subObj, $"{prefix}[{counter++}]");
                }
            }
            else
            {
                var properties = objType.GetProperties().Where(x => x.CanRead);

                foreach (var property in properties)
                {
                    Flatten(
                        dictionary,
                        property.GetValue(obj),
                        string.IsNullOrEmpty(prefix)
                            ? property.Name
                            : $"{prefix}.{property.Name}");
                }
            }
        }
    }



}
