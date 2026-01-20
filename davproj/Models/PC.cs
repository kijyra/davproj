using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace davproj.Models
{
    public class PC
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Введите имя хоста")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Длина от 3 до 50 символов")]
        [DisplayName("HostName")]
        public string Hostname { get; set; }
        [Required(ErrorMessage = "Введите IP-адрес")]
        [RegularExpression(@"^((25[0-5]|(2[0-4]|1\d|[1-9]|)\d)\.?\b){4}$",
        ErrorMessage = "Некорректный формат IP-адреса")]
        [DisplayName("IP-адрес")]
        public string IP {  get; set; }
        [DisplayName("В домене")]
        public bool Domain { get; set; }
        [DisplayName("Тонкий клиент")]
        public bool Think { get; set; }
        [DisplayName("Номер Anydesk")]
        public string? Anydesk { get; set; }
        public Workplace? Workplace { get; set; }
    }
}
