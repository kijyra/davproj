using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace davproj.Models
{
    public class ADUser
    {
        public int Id { get; set; }
        [DisplayName("Имя входа")]
        public string Cn {  get; set; } = String.Empty;
        [DisplayName("Имя")]
        public string? Name { get; set; }
        [DisplayName("Фамилия")]
        public string? SurName { get; set; }
        [DisplayName("Заданное имя")]
        public string? GivenName { get; set; }
        [DisplayName("Админ?")]
        public bool Admin  { get; set; }
        [DisplayName("Группы")]
        public List<string> Group { get; set; } = new List<string>();
        [DisplayName("Активна?")]
        public bool? Enabled { get; set; }
        [DisplayName("Привязанный пользователь")]
        public User? User { get; set; }
        [Column(TypeName = "jsonb")]
        [DisplayName("Настройки")]
        public UserSettings? Settings { get; set; }
        public void InitializeDefaultSettings()
        {
            if (Admin)
            {
                Settings = new UserSettings();
            }
            else
            {
                Settings = null;
            }
        }
    }
}
