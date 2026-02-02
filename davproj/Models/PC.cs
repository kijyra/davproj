using HardwareShared;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace davproj.Models
{
    public record MonitorSpecs(string Model, string Diagonal, string Serial);
    public class PC
    {
        public int Id { get; set; }
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Длина от 3 до 50 символов")]
        [DisplayName("HostName")]
        public string? Hostname { get; set; }
        [Required(ErrorMessage = "Введите IP-адрес")]
        [RegularExpression(@"^((25[0-5]|(2[0-4]|1\d|[1-9]|)\d)\.?\b){4}$",
        ErrorMessage = "Некорректный формат IP-адреса")]
        [DisplayName("IP-адрес")]
        public string IP {  get; set; } = String.Empty;
        [DisplayName("В домене")]
        public bool Domain { get; set; }
        [DisplayName("Тонкий клиент")]
        public bool Think { get; set; }
        [DisplayName("Номер Anydesk")]
        public string? Anydesk { get; set; }
        public Workplace? Workplace { get; set; }
        public string FullName => IP + " - " + Hostname;
        public int? CurrentHardwareInfoId { get; set; }
        public virtual HardwareInfo CurrentHardwareInfo { get; set; } = new HardwareInfo();
        public virtual ICollection<HardwareInfo> HardwareHistory { get; set; } = new List<HardwareInfo>();
        [NotMapped]
        public List<MonitorSpecs> DisplayList
        {
            get
            {
                var raw = CurrentHardwareInfo?.MonitorInfo;
                if (string.IsNullOrEmpty(raw) || raw == "Мониторы не найдены")
                    return new List<MonitorSpecs>();

                return Regex.Matches(raw, @"(?<model>.*?)\s\[(?<diag>.*?)""\]\s\(S/N:\s(?<sn>.*?)\)")
                    .Cast<Match>()
                    .Select(m => new MonitorSpecs(
                        m.Groups["model"].Value.Trim(),
                        m.Groups["diag"].Value,
                        m.Groups["sn"].Value.Trim()
                    ))
                    .ToList();
            }
        }
    }
}
