using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace davproj.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Введите имя")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Длина от 3 до 20 символов")]
        [DisplayName("Имя")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Введите фамилию")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Длина от 3 до 20 символов")]
        [DisplayName("Фамилия")]
        public string SurName { get; set; }
        [Required(ErrorMessage = "Введите должность")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Длина от 3 до 50 символов")]
        [DisplayName("Должность")]
        public string Position { get; set; }
        [DisplayName("ID в битриксе")]
        public int? Bitrix { get; set; }
        public ADUser? ADUser { get; set; }
        [DisplayName("ID пользователя AD")]
        public int? ADUserId { get; set; }
        [DisplayName("ID рабочего места")]
        public Workplace? Workplace { get; set; }
        public Printer? Printer { get; set; }
        [DisplayName("ID принтера")]
        public int? PrinterId { get; set; }
        public string FullName => Name + " " + SurName;
        public string BitrixPath => $"https://dallari.bitrix24.ru/company/personal/user/{Bitrix}/";
    }
}
