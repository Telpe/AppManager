using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AppManager.Core.Actions;

namespace AppManager.Core.Triggers
{
    internal class NetworkPortTrigger : BaseTrigger
    {
        public override TriggerTypeEnum TriggerType => TriggerTypeEnum.NetworkPort;
        public override string Description => "Monitors network port for incoming connections or data";

        private TriggerModel _parameters;
        private TcpListener _tcpListener;
        private CancellationTokenSource _cancellationTokenSource;

        public NetworkPortTrigger(string name = null) : base(name)
        {
        }

        public override bool CanStart(TriggerModel parameters = null)
        {
            return parameters?.Port > 0 && parameters.Port <= 65535;
        }

        public override async Task<bool> StartAsync(TriggerModel parameters = null)
        {
            if (IsActive || parameters == null)
                return false;

            try
            {
                _parameters = parameters;
                _cancellationTokenSource = new CancellationTokenSource();

                var ipAddress = IPAddress.Parse(parameters.IPAddress ?? "127.0.0.1");
                _tcpListener = new TcpListener(ipAddress, parameters.Port);
                _tcpListener.Start();

                IsActive = true;
                
                // Start listening for connections in background
                _ = Task.Run(async () => await ListenForConnectionsAsync(_cancellationTokenSource.Token));

                System.Diagnostics.Debug.WriteLine($"Network port trigger '{Name}' started listening on {ipAddress}:{parameters.Port}");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error starting network port trigger '{Name}': {ex.Message}");
                return false;
            }
        }

        public override async Task<bool> StopAsync()
        {
            if (!IsActive)
                return true;

            try
            {
                _cancellationTokenSource?.Cancel();
                _tcpListener?.Stop();
                
                IsActive = false;
                System.Diagnostics.Debug.WriteLine($"Network port trigger '{Name}' stopped");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error stopping network port trigger '{Name}': {ex.Message}");
                return false;
            }
        }

        private async Task ListenForConnectionsAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested && IsActive)
                {
                    var tcpClient = await _tcpListener.AcceptTcpClientAsync();
                    
                    // Handle connection in background
                    _ = Task.Run(async () => await HandleClientAsync(tcpClient, cancellationToken), cancellationToken);
                }
            }
            catch (ObjectDisposedException)
            {
                // Expected when stopping
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in network port trigger '{Name}' listener: {ex.Message}");
            }
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
        {
            try
            {
                using (client)
                {
                    var buffer = new byte[4096];
                    var stream = client.GetStream();
                    
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                    
                    if (bytesRead > 0)
                    {
                        string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        
                        System.Diagnostics.Debug.WriteLine($"Network port trigger '{Name}' received data: {data}");
                        
                        // Trigger the configured action
                        OnTriggerActivated("network_target", AppActionEnum.Launch, null, new { Data = data, Client = client.Client.RemoteEndPoint });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error handling client in network port trigger '{Name}': {ex.Message}");
            }
        }

        public override void Dispose()
        {
            _ = StopAsync();
            _cancellationTokenSource?.Dispose();
            base.Dispose();
        }
    }
}