using HardwareShared;

namespace davproj.Models.ViewModels
{
    public class ManageDBViewModel
    {
        public List<ADUser> ADUsers { get; set; } = new List<ADUser>();
        public List<Building> Buildings { get; set; } = new List<Building>();
        public List<Cartridge> Cartridges { get; set; } = new List<Cartridge>();
        public List<Floor> Floors { get; set; } = new List<Floor>();
        public List<Location> Locations { get; set; } = new List<Location>();
        public List<Manufactor> Manufactors { get; set; } = new List<Manufactor>();
        public List<Office> Offices { get; set; } = new List<Office>();
        public List<PC> PCs { get; set; } = new List<PC>();
        public List<Phone> Phones { get; set; } = new List<Phone>();
        public List<Printer> Printers { get; set; } = new List<Printer>();
        public List<PrinterModel> PrinterModels { get; set; } = new List<PrinterModel>();
        public List<User> Users { get; set; } = new List<User>();
        public List<Workplace> Workplaces { get; set; } = new List<Workplace>();
        public List<HardwareInfo> HardwareInfo { get; set; } = new List<HardwareInfo>();
    }
}
