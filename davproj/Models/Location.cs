using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace davproj.Models
{
    public class Location
    {
        public int Id { get; set; }
        [DisplayName("Адрес")]
        public string? Address { get; set; }
        public List<Building>? Buildings { get; set; }
        [Required(ErrorMessage = "Введите название локации")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Длина от 3 до 20 символов")]
        [DisplayName("Название")]
        public string Name { get; set; } = String.Empty;

    }
}
