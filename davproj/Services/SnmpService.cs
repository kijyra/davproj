using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using System.Net;
using davproj.Models;

namespace davproj.Services
{
    public class KyoceraSnmpService : IKyoceraSnmpService
    {
        private readonly int _port = 161;

        public async Task<KyoceraCounters> GetCountersAsync(string ipAddress, string community = "public")
        {
            var oidPrint = new ObjectIdentifier("1.3.6.1.4.1.1347.42.3.1.1.1.1.1");
            var oidScan = new ObjectIdentifier("1.3.6.1.4.1.1347.46.10.1.1.5.3");
            var oidCopies = new ObjectIdentifier("1.3.6.1.4.1.1347.42.3.1.1.1.1.2");
            var endpoint = new IPEndPoint(IPAddress.Parse(ipAddress), _port);
            var communityName = new OctetString(community);
            try
            {
                var result = await Task.Run(() =>
                    Messenger.Get(
                        VersionCode.V2,
                        endpoint,
                        communityName,
                        new List<Variable> {
                            new Variable(oidPrint),
                            new Variable(oidScan),
                            new Variable(oidCopies)
                        },
                        5000)
                );
                return new KyoceraCounters
                {
                    PrintCounter = int.Parse(result[0].Data.ToString()) + int.Parse(result[2].Data.ToString()),
                    ScanCounter = int.Parse(result[1].Data.ToString())
                };
            }
            catch (Exception ex)
            {
                return new KyoceraCounters { PrintCounter = 0, ScanCounter = 0 };
            }
        }
        public async Task<int> GetPrintCounterAsync(string ipAddress, string community = "public")
        {
            var counters = await GetCountersAsync(ipAddress, community);
            return counters.PrintCounter;
        }
        public async Task<int> GetScanCounterAsync(string ipAddress, string community = "public")
        {
            var counters = await GetCountersAsync(ipAddress, community);
            return counters.ScanCounter;
        }
    }
    public interface IKyoceraSnmpService
    {
        Task<KyoceraCounters> GetCountersAsync(string ipAddress, string community = "public");
        Task<int> GetScanCounterAsync(string ipAddress, string community = "public");
        Task<int> GetPrintCounterAsync(string ipAddress, string community = "public");
    }
}