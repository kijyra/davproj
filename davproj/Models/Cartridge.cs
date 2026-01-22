using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace davproj.Models
{
    public class Cartridge
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Введите название модели")]
        [StringLength(15, MinimumLength = 3, ErrorMessage = "Длина от 3 до 15 символов")]
        [DisplayName("Название")]
        public string Model { get; set; }
        public Manufactor? Manufactor { get; set; }
        [DisplayName("ID производителя")]
        public int? ManufactorId { get; set; }
        public List<PrinterModel>? PrinterModels { get; set; }
        [DisplayName("Ресурс")]
        public int? Yield { get; set; }
        public string? Name => Model + " - " + Manufactor?.Name;
    }
}