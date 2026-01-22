using HardwareShared;
using System.Diagnostics.Eventing.Reader;
using System.Management;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace ClientAPI
{
    public class HardwareAgent
    {
        private static readonly Dictionary<string, string> Manufacturers = new()
            {
                { "0080", "Samsung" },
                { "80AD", "SK Hynix" },
                { "802C", "Micron" },
                { "059B", "Crucial" },
                { "0198", "Kingston" },
                { "01BA", "Kingston" },
                { "029E", "Corsair" },
                { "04CB", "ADATA" },
                { "017A", "Apacer" },
                { "0702", "Patriot" },
                { "0420", "G.Skill" },
                { "0610", "Silicon Power" },
                { "00A4", "TeamGroup" },
                { "0B0E", "Netac" },
                { "0CD5", "Hikvision" },
                { "096E", "Gloway" },
                { "0A2F", "Asgard" }
            };
        public HardwareInfo CollectHardwareInfo()
        {
            HardwareInfo info = new HardwareInfo();
            info.ComputerName = Environment.MachineName;
            info.IpAddress = NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni => ni.OperationalStatus == OperationalStatus.Up)
                .SelectMany(ni => ni.GetIPProperties().UnicastAddresses)
                .Where(addr => addr.Address.AddressFamily == AddressFamily.InterNetwork)
                .Select(addr => addr.Address.ToString())
                .FirstOrDefault(ip => ip.StartsWith("10."));
            info.OSVersion = Environment.OSVersion.ToString();
            using (var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_Processor"))
                foreach (var obj in searcher.Get()) { info.ProcessorName = obj["Name"]?.ToString(); break; }

            using (var searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize FROM Win32_OperatingSystem"))
                foreach (var obj in searcher.Get()) info.TotalMemoryGB = Convert.ToInt64(obj["TotalVisibleMemorySize"]) / (1024 * 1024);

            using (var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_VideoController"))
                foreach (var obj in searcher.Get()) { info.VideoCard = obj["Name"]?.ToString(); break; }

            using (var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard"))
                foreach (var obj in searcher.Get()) { info.SerialNumber = obj["SerialNumber"]?.ToString(); break; }

            using (var searcher = new ManagementObjectSearcher("SELECT Size, FreeSpace FROM Win32_LogicalDisk WHERE DeviceID = 'C:'"))
                foreach (var obj in searcher.Get())
                {
                    long total = Convert.ToInt64(obj["Size"]) / (1024 * 1024 * 1024);
                    long free = Convert.ToInt64(obj["FreeSpace"]) / (1024 * 1024 * 1024);
                    info.DiskInfo = $"C: {free}GB free of {total}GB";
                }
            using (var searcher = new ManagementObjectSearcher("SELECT MemoryDevices FROM Win32_PhysicalMemoryArray"))
            {
                foreach (var obj in searcher.Get())
                {
                    info.TotalRamSlots = Convert.ToInt32(obj["MemoryDevices"]);
                    break;
                }
            }
            using (var searcher = new ManagementObjectSearcher("SELECT Tag FROM Win32_PhysicalMemory"))
            {
                info.UsedRamSlots = searcher.Get().Count;
            }
            using (var searcher = new ManagementObjectSearcher("SELECT Manufacturer, SMBIOSMemoryType FROM Win32_PhysicalMemory"))
            {
                foreach (var obj in searcher.Get())
                {
                    string rawId = obj["Manufacturer"]?.ToString().Trim() ?? "";
                    if (Manufacturers.TryGetValue(rawId, out string name))
                    {
                        info.RamManufacturer = name;
                    }
                    else
                    {
                        info.RamManufacturer = string.IsNullOrEmpty(rawId) ? "Unknown" : $"Other ({rawId})";
                    }
                    if (obj["SMBIOSMemoryType"] != null)
                    {
                        int typeCode = Convert.ToInt32(obj["SMBIOSMemoryType"]);
                        info.RamType = typeCode switch
                        {
                            20 => "DDR",
                            21 => "DDR2",
                            24 => "DDR3",
                            26 => "DDR4",
                            34 => "DDR5",
                            _ => $"Unknown ({typeCode})"
                        };
                    }
                    else
                    {
                        info.RamType = "Not reported";
                    }

                    break;
                }
            }
            using (var searcher = new ManagementObjectSearcher("SELECT Model, CapabilityDescriptions FROM Win32_DiskDrive"))
            {
                foreach (var obj in searcher.Get())
                {
                    string model = obj["Model"]?.ToString().ToLower() ?? "";
                    if (model.Contains("ssd") || model.Contains("nvme") || model.Contains("fixed media"))
                    {
                        info.DiskType = "SSD";
                    }
                    else
                    {
                        info.DiskType = "HDD/Other";
                    }
                    break;
                }
            }
            using (var searcher = new ManagementObjectSearcher("SELECT PartOfDomain FROM Win32_ComputerSystem"))
            {
                foreach (var obj in searcher.Get())
                {
                    info.IsDomainJoined = (bool)obj["PartOfDomain"];
                }
            }
            return info;
        }
    }
}