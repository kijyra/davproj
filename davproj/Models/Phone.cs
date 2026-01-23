using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace davproj.Models
{
    public class Phone
    {
        public int Id { get; set; }
        [DisplayName("Трубка")]
        public bool Handset { get; set; }
        [Required(ErrorMessage = "Введите IP-адрес")]
        [RegularExpression(@"^((25[0-5]|(2[0-4]|1\d|[1-9]|)\d)\.?\b){4}$",
        ErrorMessage = "Некорректный формат IP-адреса")]
        [DisplayName("IP-адрес")]
        public string Ip {  get; set; } = String.Empty;
        [DisplayName("Имя в базе")]
        public string? NameInBase { get; set; }
        [Required(ErrorMessage = "Введите модель телефона/трубки")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Длина от 3 до 20 символов")]
        [DisplayName("Модель")]
        public string Model { get; set; } = String.Empty;
        [Range(100, 9999, ErrorMessage = "Номер должен быть от 100 до 9999")]
        [DisplayName("Номер")]
        public int Number {  get; set; }
        public Workplace? Workplace { get; set; }
    }
}
