using System.ComponentModel.DataAnnotations;

namespace HardwareShared
{
    public class HardwareInfo
    {
        [Key]
        public int Id { get; set; }
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
        public bool IsDomainJoined {  get; set; } = false;
        public string IpAddress {  get; set; } = string.Empty;
        public DateTime CollectedAtUtc { get; set; } = DateTime.UtcNow;
        

public HardwareInfo CollectHardwareInfo()
        {
            throw new NotImplementedException();
        }
    }
}
