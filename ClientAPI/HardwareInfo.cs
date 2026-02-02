using HardwareShared;
using Microsoft.Win32;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Management;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
namespace ClientAPI
{
    public class HardwareAgent
    {
        private static string DecodeWmiString(ushort[] array)
        {
            if (array == null) return "N/A";
            StringBuilder sb = new StringBuilder();
            foreach (ushort c in array)
            {
                if (c == 0) break;
                sb.Append((char)c);
            }
            return sb.ToString().Trim();
        }

        // Словарь для определения производителя ОЗУ
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
                .FirstOrDefault(ip => ip.StartsWith("10.")) ?? string.Empty;
            info.OSVersion = Environment.OSVersion.ToString();
            using (var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_Processor"))
                foreach (var obj in searcher.Get()) { info.ProcessorName = obj["Name"]?.ToString() ?? string.Empty; break; }

            using (var searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize FROM Win32_OperatingSystem"))
                foreach (var obj in searcher.Get()) info.TotalMemoryGB = Convert.ToInt64(obj["TotalVisibleMemorySize"]) / (1024 * 1024);

            using (var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_VideoController"))
                foreach (var obj in searcher.Get()) { info.VideoCard = obj["Name"]?.ToString() ?? string.Empty; break; }

            using (var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard"))
                foreach (var obj in searcher.Get()) { info.SerialNumber = obj["SerialNumber"]?.ToString() ?? string.Empty; break; }

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
            List<string> monitorList = new List<string>();
            try
            {
                var diagonals = new Dictionary<string, double>();
                using (var searcherSize = new ManagementObjectSearcher(@"root\WMI", "SELECT InstanceName, MaxHorizontalImageSize, MaxVerticalImageSize FROM WmiMonitorBasicDisplayParams"))
                {
                    foreach (ManagementObject obj in searcherSize.Get())
                    {
                        string? instName = obj["InstanceName"]?.ToString();
                        double w = Convert.ToDouble(obj["MaxHorizontalImageSize"]);
                        double h = Convert.ToDouble(obj["MaxVerticalImageSize"]);
                        if (w > 0 && h > 0 && !string.IsNullOrEmpty(instName))
                        {
                            double diag = Math.Sqrt(Math.Pow(w, 2) + Math.Pow(h, 2)) / 2.54;
                            diagonals[instName] = Math.Round(diag, 1);
                        }
                    }
                }
                using (var searcherID = new ManagementObjectSearcher(@"root\WMI", "SELECT InstanceName, UserFriendlyName, SerialNumberID FROM WmiMonitorID"))
                {
                    foreach (ManagementObject obj in searcherID.Get())
                    {
                        string instName = obj["InstanceName"]?.ToString();
                        var nameCodes = (ushort[])obj["UserFriendlyName"];
                        string model = nameCodes != null
                            ? Encoding.ASCII.GetString(Array.ConvertAll(nameCodes, x => (byte)x)).Replace("\0", "").Trim()
                            : "Unknown Model";
                        var serialCodes = (ushort[])obj["SerialNumberID"];
                        string serial = serialCodes != null
                            ? Encoding.ASCII.GetString(Array.ConvertAll(serialCodes, x => (byte)x)).Replace("\0", "").Trim()
                            : "No S/N";
                        string diagStr = diagonals.ContainsKey(instName) ? $"{diagonals[instName]}\"" : "??\"";
                        monitorList.Add($"{model} [{diagStr}] (S/N: {serial})");
                    }
                }
                info.MonitorInfo = monitorList.Count > 0
                    ? string.Join(" | ", monitorList)
                    : "Мониторы не найдены";
            }
            catch (Exception ex)
            {
                info.MonitorInfo = "Ошибка сбора данных: " + ex.Message;
            }
            using (var searcher = new ManagementObjectSearcher("SELECT Tag FROM Win32_PhysicalMemory"))
            {
                info.UsedRamSlots = searcher.Get().Count;
            }
            using (var searcher = new ManagementObjectSearcher("SELECT Manufacturer, SMBIOSMemoryType FROM Win32_PhysicalMemory"))
            {
                foreach (var obj in searcher.Get())
                {
                    string rawId = obj["Manufacturer"]?.ToString()?.Trim() ?? string.Empty;
                    if (Manufacturers.TryGetValue(rawId, out string? name))
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
            info.DiskType = "Unspecified";
            try
            {
                using (var searcher = new ManagementObjectSearcher(@"root\Microsoft\Windows\Storage", "SELECT MediaType FROM MSFT_PhysicalDisk"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        int type = Convert.ToInt32(obj["MediaType"]);
                        info.DiskType = type switch { 3 => "HDD", 4 => "SSD", 5 => "SCM", _ => "Unspecified" };
                    }
                }
            }
            catch (ManagementException ex)
            {
                Console.WriteLine($"Ошибка при проверке типа диска: {ex.Message}");
                info.DiskType = "Ошибка сбора данных";
            }

            using (var searcher = new ManagementObjectSearcher("SELECT PartOfDomain FROM Win32_ComputerSystem"))
            {
                foreach (var obj in searcher.Get())
                {
                    info.IsDomainJoined = (bool)obj["PartOfDomain"];
                }
            }

            using (var searcher = new ManagementObjectSearcher("SELECT UserName FROM Win32_ComputerSystem"))
                foreach (var obj in searcher.Get())
                    info.CurrentUserName = obj["UserName"]?.ToString() ?? "No interactive user";

            var speeds = new List<string>();
            using (var searcher = new ManagementObjectSearcher("SELECT ConfiguredClockSpeed FROM Win32_PhysicalMemory"))
                foreach (var obj in searcher.Get())
                    speeds.Add($"{obj["ConfiguredClockSpeed"]} MHz");
            info.RamSpeed = speeds.Count > 0 ? string.Join(", ", speeds.Distinct()) : "Unknown";

            info.DiskHealth = "OK";
            try
            {
                using (var searcher = new ManagementObjectSearcher(@"root\WMI", "SELECT PredictFailure FROM MSStorageDriver_FailurePredictStatus"))
                    foreach (var obj in searcher.Get())
                        if ((bool)obj["PredictFailure"]) info.DiskHealth = "ВНИМАНИЕ: Возможен отказ!";
            }
            catch (ManagementException ex)
            {
                Console.WriteLine($"Ошибка при проверке S.M.A.R.T.: {ex.Message}");
                info.DiskHealth = "Ошибка сбора S.M.A.R.T. данных";
            }


            var software = new List<string>();
            string[] registryPaths = {
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
                @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
            };
            foreach (var path in registryPaths)
            {
                using (var key = Registry.LocalMachine.OpenSubKey(path))
                {
                    if (key == null) continue;
                    foreach (var subkeyName in key.GetSubKeyNames())
                    {
                        using (var subkey = key.OpenSubKey(subkeyName))
                        {
                            var name = subkey?.GetValue("DisplayName")?.ToString();
                            var version = subkey?.GetValue("DisplayVersion")?.ToString();
                            var installDate = subkey?.GetValue("InstallDate")?.ToString();
                            var isSystem = subkey?.GetValue("SystemComponent")?.ToString() == "1";

                            if (!string.IsNullOrWhiteSpace(name) && !isSystem && !name.Contains("KB") && !name.Contains("Update for Microsoft"))
                            {
                                string dateStr = (installDate?.Length == 8)
                                    ? $" ({installDate.Substring(6, 2)}.{installDate.Substring(4, 2)}.{installDate.Substring(0, 4)})"
                                    : string.Empty;
                                string versionStr = !string.IsNullOrEmpty(version) ? $" [v{version}]" : string.Empty;
                                software.Add($"{name}{versionStr}{dateStr}");
                            }
                        }
                    }
                }
            }
            info.SoftwareList = software.Distinct().OrderBy(s => s).ToList();

            var avList = new List<string>();
            try
            {
                using (var searcher = new ManagementObjectSearcher(@"root\SecurityCenter2", "SELECT displayName FROM AntiVirusProduct"))
                    foreach (var obj in searcher.Get())
                        avList.Add(obj["displayName"]?.ToString());
            }
            catch (ManagementException ex)
            {
                Console.WriteLine($"Ошибка при сборе данных об антивирусе: {ex.Message}");
                avList.Add("Ошибка сбора данных");
            }
            info.Antivirus = avList.Count > 0 ? string.Join(", ", avList) : "Not Found";


            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT LastBootUpTime FROM Win32_OperatingSystem"))
                    foreach (var obj in searcher.Get())
                    {
                        var lastBoot = ManagementDateTimeConverter.ToDateTime(obj["LastBootUpTime"].ToString());
                        var uptime = DateTime.Now - lastBoot;
                        info.Uptime = $"{uptime.Days}d {uptime.Hours}h {uptime.Minutes}m";
                    }
            }
            catch { info.Uptime = "Unknown"; }

            var usbTmp = new List<string>();
            using (var searcher = new ManagementObjectSearcher("SELECT Name, DeviceID FROM Win32_PnPEntity WHERE DeviceID LIKE '%USB%' AND Name IS NOT NULL"))
                foreach (var obj in searcher.Get())
                    usbTmp.Add($"{obj["Name"]} (ID: {obj["DeviceID"]})");
            info.UsbDevices = usbTmp.Distinct().ToList();

            using (var searcher = new ManagementObjectSearcher("SELECT Name, PortName FROM Win32_Printer"))
            {
                foreach (var obj in searcher.Get())
                {
                    string printerName = obj["Name"]?.ToString();

                    if (printerName != "Microsoft XPS Document Writer" &&
                        printerName != "Microsoft Print to PDF" &&
                        printerName != "Fax")
                    {
                        info.Printers.Add($"{printerName} on {obj["PortName"]}");
                    }
                }
            }

            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = "netstat.exe",
                    Arguments = "-a -n -o",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };
                process.Start();
                if (process.WaitForExit(5000))
                {
                    string[] lines = process.StandardOutput.ReadToEnd().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var line in lines)
                    {
                        if (line.Contains("LISTENING"))
                        {
                            var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            if (parts.Length >= 5 && int.TryParse(parts[1].Split(':').Last(), out int port) && int.TryParse(parts[4], out int pid))
                            {
                                try
                                {
                                    using (var p = Process.GetProcessById(pid))
                                        info.OpenPorts.Add($"{port} [{p.ProcessName}]");
                                }
                                catch { info.OpenPorts.Add($"{port} [PID: {pid}]"); }
                            }
                        }
                    }
                }
                else { process.Kill(); }
            }
            info.OpenPorts = info.OpenPorts.Distinct().OrderBy(x => x).ToList();


            return info;
        }
    }
}