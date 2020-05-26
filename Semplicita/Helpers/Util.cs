public static class Util
{        
    public static T GetSetting<T>(string name) {
        string value = System.Configuration.ConfigurationManager.AppSettings[ name ];

        if( value == null ) {
            throw new System.Exception(string.Format("Could not find setting '{0}',", name));
        }

        return (T)System.Convert.ChangeType(value, typeof(T), System.Globalization.CultureInfo.InvariantCulture);
    }
    public static string GetSetting(string name) {
        string value = System.Configuration.ConfigurationManager.AppSettings[ name ];

        if( value == null ) {
            throw new System.Exception(string.Format("Could not find setting '{0}',", name));
        }

        return (string)System.Convert.ChangeType(value, typeof(string), System.Globalization.CultureInfo.InvariantCulture);
    }
}