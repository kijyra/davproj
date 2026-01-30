using Microsoft.Win32;
using Serilog;
using System.IO.Pipes;
using System.Net.NetworkInformation;
using System.Security.AccessControl;
using System.Security.Principal;
using System.ServiceProcess;
namespace HardwareAgent
{
    internal class VNCServiceManager
    {
        private const string VncServiceName = "tvnserver";
        private const string VncRegistryPath = @"Software\TightVNC\Server";
        public bool IsVncActive()
        {
            try
            {
                var properties = IPGlobalProperties.GetIPGlobalProperties();
                var connections = properties.GetActiveTcpConnections();
                return connections.Any(c =>
                    c.LocalEndPoint.Port == 5900 &&
                    c.State == TcpState.Established);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при проверке активности сетевых соединений VNC");
                return false;
            }
        }
        public void SetupAndStart(bool fullControl)
        {
            Log.Information("Настройка TightVNC. Полный контроль: {FullControl}", fullControl);
            try
            {
                UpdateRegistrySettings(fullControl);
                using (var sc = new ServiceController(VncServiceName))
                {
                    if (sc.Status == ServiceControllerStatus.Running)
                    {
                        Log.Information("Перезапуск службы VNC для применения настроек...");
                        sc.Stop();
                        sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));
                    }
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(10));
                    Log.Information("Служба VNC успешно запущена.");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Критическая ошибка при настройке или запуске TightVNC.");
            }
        }
        private void UpdateRegistrySettings(bool fullControl)
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(VncRegistryPath))
                {
                    if (key != null)
                    {
                        int controlLevel = fullControl ? 0 : 1;
                        key.SetValue("ControlLevel", controlLevel, RegistryValueKind.DWord);
                        key.SetValue("QuerySetting", 0, RegistryValueKind.DWord);
                        key.SetValue("AuthRequired", 1, RegistryValueKind.DWord);
                        byte[] vncPasswordHash = new byte[] { 0x3f, 0x2f, 0x2e, 0xf6, 0x72, 0xae, 0x04, 0x36 };
                        key.SetValue("Password", vncPasswordHash, RegistryValueKind.Binary);
                        key.SetValue("PasswordViewOnly", vncPasswordHash, RegistryValueKind.Binary);

                        Log.Information("Реестр TightVNC обновлен: ControlLevel={Level}", controlLevel);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Не удалось записать настройки в реестр. Проверьте права службы (SYSTEM).");
            }
        }
        public void StopVncService()
        {
            try
            {
                using (var sc = new ServiceController(VncServiceName))
                {
                    if (sc.Status != ServiceControllerStatus.Stopped)
                    {
                        sc.Stop();
                        sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));
                        Log.Information("Служба VNC остановлена.");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при остановке службы VNC.");
            }
        }
        public async Task<string> RequestUserPermission(VncRequest request)
        {
            NamedPipeServerStream pipeServer = null;
            try
            {
                var pipeSecurity = new PipeSecurity();
                pipeSecurity.AddAccessRule(new PipeAccessRule(
                    new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null),
                    PipeAccessRights.ReadWrite,
                    AccessControlType.Allow));
                pipeServer = NamedPipeServerStreamAcl.Create(
                    "VncControlPipe",
                    PipeDirection.InOut,
                    NamedPipeServerStream.MaxAllowedServerInstances,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous,
                    0,
                    0,
                    pipeSecurity);
                Log.Information("Ожидание подключения Tray App...");
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
                await pipeServer.WaitForConnectionAsync(cts.Token);
                var writer = new StreamWriter(pipeServer, leaveOpen: true) { AutoFlush = true };
                var reader = new StreamReader(pipeServer, leaveOpen: true);
                await writer.WriteLineAsync($"{request.AdminName}|{request.IsFullControl}");
                var response = await reader.ReadLineAsync();
                if (response == "ALLOW")
                {
                    this.SetupAndStart(true);
                    await writer.WriteLineAsync("SESSION_STARTED");
                    _ = Task.Run(() => MonitorVncSession(pipeServer, writer, reader));
                    return "ALLOW";
                }
                writer.Dispose();
                reader.Dispose();
                pipeServer.Dispose();
                return response ?? "DENY";
            }
            catch (Exception ex)
            {
                Log.Error("Ошибка авторизации Pipe: {Msg}", ex.Message);
                pipeServer?.Dispose();
                return "ERROR";
            }
        }
        public async Task MonitorVncSession(NamedPipeServerStream pipe, StreamWriter writer, StreamReader reader)
        {
            try
            {
                int waitTimeout = 120;
                while (waitTimeout > 0 && !this.IsVncActive())
                {
                    await Task.Delay(1000);
                    waitTimeout--;
                }
                while (this.IsVncActive())
                {
                    await Task.Delay(5000);
                }
                Log.Information("Сессия завершена, уведомляем Tray App.");
                await writer.WriteLineAsync("SESSION_ENDED");
            }
            catch (Exception ex)
            {
                Log.Warning("Pipe мониторинг завершён с ошибкой: {Msg}", ex.Message);
            }
            finally
            {
                writer.Dispose();
                reader.Dispose();
                pipe.Dispose();
            }
        }
    }
}
