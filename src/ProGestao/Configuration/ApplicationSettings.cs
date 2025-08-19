namespace ProGestao.Configuration
{
    public class ApplicationSettings
    {
        public string AppName { get; set; } = "ProGestao";
        public string Version { get; set; } = "1.0.0";
        public string Environment { get; set; } = "Development";
        public bool EnableDetailedErrors { get; set; } = false;
        public bool EnableSqlLogging { get; set; } = false;
    }
}
