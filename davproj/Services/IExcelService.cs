using davproj.Models;

namespace davproj.Services
{
    public interface IExcelService
    {
        byte[] GetMonitorsReport(List<Building> buildings);
        byte[] GetFullReport(List<Building> buildings);
        byte[] GetHardwareReport(List<Building> buildings);
        byte[] GetPrinterReport(List<Building> buildings);
        byte[] GetSoftwareReport(List<Building> buildings);
        byte[] GetUsbReport(List<Building> buildings);
    }
}