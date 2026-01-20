namespace davproj.Models
{
    public class UserSettings
    {
        public static Dictionary<string, string> Connections { get; set; } = new ()
        {
            { "Any", "Anydesk Remote" },
            { "VNC",     "VNC Viewer" },
            { "RDPc",    "RDP с запросом" },
            { "RDPs",    "RDP без запроса" },
            { "WTRC",    "WTRC без запроса" },
            { "WEB",     "Web Интерфейс" }
        };
        public static Dictionary<string, string> ConnectString { get; set; } = new()
        {
            { "Any", "anydesk://" },
            { "VNC",     "vnc://" },
            { "RDPc",    "rdpc://" },
            { "RDPs",    "rdps://" },
            { "WTRC",    "wtrc://" },
            { "WEB",     "http://" }
        };
        public static Dictionary<string, string> Languages { get; set; } = new ()
        {
            { "ru", "Русский" },
            { "en", "English" }
        };

        public string DefaultConnection { get; set; } = "Any";
        public string DomainConnection { get; set; } = "Any";
        public string ThinkConnection { get; set; } = "WEB";
        public string Language { get; set; } = "ru";
    }
}
