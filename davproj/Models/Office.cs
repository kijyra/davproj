using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace davproj.Models
{
    public class Office
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Введите название кабинета")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Длина от 3 до 20 символов")]
        [DisplayName("Название")]
        public string Name { get; set; }
        public List<Workplace>? Workplaces { get; set; }
        public Floor? Floor { get; set; }
        [DisplayName("ID этажа")]
        public int? FloorId { get; set; }
        public string FullTitle => $"{Name} —> {Floor?.FloorNum} -> " +
            $"{Floor?.Building?.Name} -> {Floor?.Building?.Location?.Name}";
    }
}
