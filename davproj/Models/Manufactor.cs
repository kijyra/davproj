using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace davproj.Models
{
    public class Manufactor
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Введите название производителя")]
        [StringLength(30, MinimumLength = 3, ErrorMessage = "Длина от 3 до 30 символов")]
        [DisplayName("Название")]
        public string Name { get; set; } = String.Empty;
        public List<Cartridge>? Cartridges { get; set; }
    }
}
