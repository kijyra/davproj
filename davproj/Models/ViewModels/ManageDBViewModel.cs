using HardwareShared;

namespace davproj.Models.ViewModels
{
    public class ManageDBViewModel
    {
        public List<ADUser> ADUsers { get; set; }
        public List<Building> Buildings { get; set; }
        public List<Cartridge> Cartridges { get; set; }
        public List<Floor> Floors { get; set; }
        public List<Location> Locations { get; set; }
        public List<Manufactor> Manufactors { get; set; }
        public List<Office> Offices { get; set; }
        public List<PC> PCs { get; set; }
        public List<Phone> Phones { get; set; }
        public List<Printer> Printers { get; set; }
        public List<PrinterModel> PrinterModels { get; set; }
        public List<User> Users { get; set; }
        public List<Workplace> Workplaces { get; set; }
        public List<HardwareInfo> HardwareInfo { get; set; }
    }
}
