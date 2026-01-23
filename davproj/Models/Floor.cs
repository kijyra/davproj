using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace davproj.Models
{
    public class Floor
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Введите название этажа")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Длина от 3 до 20 символов")]
        [DisplayName("Номер этажа")]
        public string FloorNum { get; set; } = String.Empty;
        public List<Office>? Offices { get; set; }
        public Building? Building { get; set; }
        [DisplayName("ID здания")]
        public int? BuildingId { get; set; }
    }
}
