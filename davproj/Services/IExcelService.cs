using davproj.Models;

namespace davproj.Services
{
    public interface IExcelService
    {
        byte[] GetMonitorsReport(List<Building> buildings);
        byte[] GetFullReport(List<Building> buildings);
    }
}