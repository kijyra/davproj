using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace HardwareShared
{
    public class HardwareInfo
    {
        [Key]
        public int Id { get; set; }
        public int? PCId { get; set; }
        public string ComputerName { get; set; } = string.Empty;
        public string ProcessorName { get; set; } = string.Empty;
        public string MonitorInfo { get; set; } = string.Empty;
        public long TotalMemoryGB { get; set; }
        public string VideoCard { get; set; } = string.Empty;
        public string OSVersion { get; set; } = string.Empty;
        public string DiskInfo { get; set; } = string.Empty;
        public string DiskType { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public int TotalRamSlots { get; set; }
        public int UsedRamSlots { get; set; }
        public string RamType { get; set; } = string.Empty;
        public string RamManufacturer { get; set; } = string.Empty;
        public bool IsDomainJoined { get; set; } = false;
        public string IpAddress { get; set; } = string.Empty;
        public DateTime CollectedAtUtc { get; set; } = DateTime.UtcNow;
        public string MotherboardModel { get; set; } = string.Empty;
        public string CurrentUserName { get; set; } = string.Empty;
        public string RamSpeed { get; set; } = string.Empty;
        public string DiskHealth { get; set; } = string.Empty;
        public string Antivirus { get; set; } = string.Empty;
        public string Uptime { get; set; } = string.Empty;

        // Списки данных
        public List<string> SoftwareList { get; set; } = new List<string>();
        public List<string> UsbDevices { get; set; } = new List<string>();
        public List<string> Printers { get; set; } = new List<string>();
        public List<string> OpenPorts { get; set; } = new List<string>();

        // Обновления Windows
        public int PendingUpdatesCount { get; set; }
        public string LastUpdateDate { get; set; } = string.Empty;

        public HardwareInfo CollectHardwareInfo()
        {
            throw new NotImplementedException();
        }
    }
}
