using System.Management;
using System.Text.RegularExpressions;

public static class HardwareHelper
{
    public static string GetUsbDeviceName(string deviceId)
    {
        var match = Regex.Match(deviceId, @"VID_([0-9A-F]{4})&PID_([0-9A-F]{4})");
        if (!match.Success) return deviceId;

        string vid = match.Groups[1].Value;
        string pid = match.Groups[2].Value;

        string query = $"SELECT Description FROM Win32_PnPSignedDriver WHERE DeviceID LIKE '%VID_{vid}%PID_{pid}%'";

        try
        {
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    return obj["Description"]?.ToString() ?? deviceId;
                }
            }
        }
        catch (ManagementException)
        {
            return deviceId + " (Ошибка WMI)";
        }

        return deviceId;
    }
}