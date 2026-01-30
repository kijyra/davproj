using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace davproj.Models
{
    public class PrinterModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Введите название модели принтера")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Длина от 3 до 20 символов")]
        [DisplayName("Название")]
        public string Name { get; set; } = String.Empty;
        public Cartridge Cartridge { get; set; } = new Cartridge();
        [DisplayName("ID картриджа")]
        public int? CartridgeId { get; set; }
        public List<Printer> Printers { get; set; } = new List<Printer>();
        [DisplayName("МФУ?")]
        public bool MFP {  get; set; }
    }
}
