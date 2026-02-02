namespace davproj.Models
{
    public class UserSettings
    {
        public static Dictionary<string, string> Connections { get; set; } = new ()
        {
            { "Any",        "Anydesk Remote" },
            { "VNC00",      "VNC просмотр без запроса" },
            { "VNC01",      "VNC просмотр с запросом" },
            { "VNC10",      "VNC управление без запроса" },
            { "VNC11",      "VNC управление с запросом" },
            { "WTRC",       "WTRC без запроса" },
            { "WEB",        "Web Интерфейс" }
        };
        public static Dictionary<string, string> ConnectString { get; set; } = new()
        {
            { "Any",     "anydesk://" },
            { "VNC",     "vnc://" },
            { "WTRC",    "wtrc://" },
            { "WEB",     "http://" }
        };
        public static Dictionary<string, string> Languages { get; set; } = new ()
        {
            { "ru", "Русский" },
            { "en", "English" }
        };

        public string DefaultPCConnection { get; set; } = "Any";
        public string ThinkConnection { get; set; } = "WEB";
        public string Language { get; set; } = "ru";
    }
}
