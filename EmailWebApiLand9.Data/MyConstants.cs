namespace EmailWebApiLand9.Data
{
    public class MyConstants
    {
        public const string AppSettingsFile = "appsettings.json";
        public const string ConnectionString = "ConnectionString";
        public const string MyAppName = "EmailWebApiLand9";

        public string BaseWebUrl { get; set; }

        public string DbConfigSettingsApplication { get; set; }
        public string DbConfigSettingsType { get; set; }
        public string DbConfigSettingsProcess { get; set; }
        public string DbConfigSettingsNameFilter { get; set; }
        public string DbConfigSettingsUser { get; set; }

    }
}
