using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace davproj.Models
{
    public class Printer
    {
        public int Id { get; set; }
        public PrinterModel? PrinterModel { get; set; }
        [Required(ErrorMessage = "Введите имя принтера")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Длина от 3 до 20 символов")]
        [DisplayName("Название принтера")]
        public string PrinterName { get; set; }
        [Required(ErrorMessage = "Выберите модель принтера")]
        [DisplayName("ID модели")]
        public int? PrinterModelId { get; set; }
        [DisplayName("Счётчик принтера")]
        public int? PrintCount { get; set; }
        [DisplayName("Счётчик сканера")]
        public int? ScanCount { get; set; }
        [DisplayName("Время последнего обновления по SNMP")]
        public string? LastUpdateSNMP { get; set; }
        [Required(ErrorMessage = "Введите IP-адрес")]
        [RegularExpression(@"^((25[0-5]|(2[0-4]|1\d|[1-9]|)\d)\.?\b){4}$",
        ErrorMessage = "Некорректный формат IP-адреса")]
        [DisplayName("IP-адрес")]
        public string? IP {  get; set; }
        [DisplayName("HostName")]
        public string? HostName { get; set; }
        [DisplayName("Последний ремонт печи")]
        public List<string>? LastFuserRepair { get; set; } = new List<string>();
        public ICollection<Workplace> Workplaces { get; set; } = new List<Workplace>();
        public ICollection<User> Users { get; set; } = new List<User>();
        public string FullName => IP + " - " + HostName;
    }
}
