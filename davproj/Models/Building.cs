using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace davproj.Models
{
    public class Building
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Введите название")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Длина от 3 до 20 символов")]
        [DisplayName("Название")]
        public string Name { get; set; } = String.Empty;
        public List<Floor>? Floors { get; set; }
        public Location? Location { get; set; }
        [DisplayName("ID локации")]
        public int? LocationId { get; set; }
    }
}
