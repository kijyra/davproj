using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace davproj.Models
{
    public class Workplace
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Введите название рабочего места")]
        [StringLength(30, MinimumLength = 3, ErrorMessage = "Длина от 3 до 30 символов")]
        [DisplayName("Название")]
        public string Name { get; set; }
        [DisplayName("Принтер?")]
        public bool Print { get; set; } 
        public Printer? Printer { get; set; }
        [DisplayName("ID принтера")]
        public int? PrinterId { get; set; }
        public PC? PC { get; set; }
        public Phone? Phone { get; set; }
        public User? User { get; set; } 
        public Office? Office { get; set; }
        [DisplayName("ID кабинета")]
        public int? OfficeId { get; set; }
    }
}
