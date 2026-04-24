using System.Collections;

namespace NotoriousTest.Web.Helpers
{
    /// <summary>
    /// Extension methods for converting objects to flat string dictionaries suitable for configuration injection.
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// Flattens an object into a dictionary of string key/value pairs using colon-separated paths as keys.
        /// </summary>
        /// <param name="obj">The object to flatten.</param>
        /// <param name="name">Optional prefix for all keys.</param>
        /// <returns>A flat dictionary with colon-separated keys and nullable string values.</returns>
        public static Dictionary<string, string?> ToDictionary(
            this object obj,
            string name = "")
        {

            if (obj is Dictionary<string, string?> dict)
            {
                return dict.ToDictionary(
                    kvp => string.IsNullOrEmpty(name) ? kvp.Key : $"{name}:{kvp.Key}",
                    kvp => kvp.Value
                );
            }

            var dictionary = new Dictionary<string, string?>();

            Flatten(dictionary, obj, name);

            return dictionary;
        }

        private static void Flatten(
            IDictionary<string, string?> dictionary,
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
                            : $"{prefix}:{property.Name}");
                }
            }
        }
    }
}
