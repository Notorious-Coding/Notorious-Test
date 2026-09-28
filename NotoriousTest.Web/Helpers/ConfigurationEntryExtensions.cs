using System.Collections;
using System.Globalization;
using System.Reflection;
using NotoriousTest.Core.Configuration;

namespace NotoriousTest.Web.Helpers;

public static class ConfigurationEntryExtensions
{
    private const string APP_SETTINGS_SEPARATOR = ":";
    private const string ENVIRONMENT_VARIABLE_SEPARATOR = "__";

    /// <summary>
    ///     Flattens configuration entries into IConfiguration keys (appsettings format).
    ///     e.g. <c>Section:Property</c>, <c>Section:Array:0</c>.
    /// </summary>
    public static Dictionary<string, string?> ToAppSettings(this List<ConfigurationEntry<object>> entries)
        => Flatten(entries, APP_SETTINGS_SEPARATOR);

    /// <summary>
    ///     Flattens configuration entries into environment variables understood by IConfiguration.
    ///     e.g. <c>Section__Property</c>, <c>Section__Array__0</c>.
    /// </summary>
    public static Dictionary<string, string?> ToEnvironmentVariables(this List<ConfigurationEntry<object>> entries)
        => Flatten(entries, ENVIRONMENT_VARIABLE_SEPARATOR);

    private static Dictionary<string, string?> Flatten(List<ConfigurationEntry<object>> entries, string separator)
    {
        var dictionary = new Dictionary<string, string?>();

        foreach (ConfigurationEntry<object> entry in entries) Flatten(dictionary, entry.Value, entry.Key, separator);

        return dictionary;
    }

    private static void Flatten(
        Dictionary<string, string?> dictionary,
        object? obj,
        string prefix,
        string separator)
    {
        if (obj == null)
        {
            dictionary[prefix] = null;

            return;
        }

        Type objType = obj.GetType();

        if (objType.IsValueType || objType == typeof(string))
        {
            dictionary[prefix] = Convert.ToString(obj, CultureInfo.InvariantCulture);
        }
        else if (obj is IDictionary subDictionary)
        {
            foreach (DictionaryEntry subEntry in subDictionary)
                Flatten(dictionary, subEntry.Value, Combine(prefix, subEntry.Key.ToString()!, separator), separator);
        }
        else if (obj is IEnumerable subObjects)
        {
            int counter = 0;

            foreach (object? subObj in subObjects)
                Flatten(dictionary, subObj, Combine(prefix, (counter++).ToString(), separator), separator);
        }
        else
        {
            IEnumerable<PropertyInfo> properties =
                objType.GetProperties().Where(x => x.CanRead && x.GetIndexParameters().Length == 0);

            foreach (PropertyInfo property in properties)
                Flatten(dictionary, property.GetValue(obj), Combine(prefix, property.Name, separator), separator);
        }
    }

    private static string Combine(string prefix, string key, string separator)
        => string.IsNullOrEmpty(prefix) ? key : $"{prefix}{separator}{key}";
}
