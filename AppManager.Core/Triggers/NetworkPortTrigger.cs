using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AppManager.Core.Actions;
using System.Collections.Generic;

namespace AppManager.Core.Triggers
{
    internal class NetworkPortTrigger : BaseTrigger
    {
        public override TriggerTypeEnum TriggerType => TriggerTypeEnum.NetworkPort;

        private TcpListener _tcpListener;
        private CancellationTokenSource _cancellationTokenSource;

        public int? Port { get; set; }
        public string? IPAddress { get; set; }
        public int? TimeoutMs { get; set; }
        public Dictionary<string, object> CustomProperties { get; set; }

        public NetworkPortTrigger(TriggerModel model) : base(model)
        {
            Description = "Monitors network port for incoming connections or data";
            
            Port = model.Port;
            IPAddress = model.IPAddress;
            TimeoutMs = model.TimeoutMs;
            CustomProperties = model.CustomProperties ?? [];
        }

        public override bool CanStart()
        {
            return Port > 0 && Port <= 65535;
        }

        public override Task<bool> StartAsync()
        {
            return Task.Run<bool>(() =>
            {
                if (!IsActive) { return false; }

                try
                {
                    _cancellationTokenSource = new CancellationTokenSource();

                    var ipAddress = System.Net.IPAddress.Parse(IPAddress ?? "127.0.0.1");
                    _tcpListener = new TcpListener(ipAddress, Port.Value);
                    _tcpListener.Start();

                    // Start listening for connections in background
                    _ = Task.Run(async () => await ListenForConnectionsAsync(_cancellationTokenSource.Token));

                    System.Diagnostics.Debug.WriteLine($"Network port trigger '{Name}' started listening on {ipAddress}:{Port}");
                    return true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error starting network port trigger '{Name}': {ex.Message}");
                    return false;
                }
            });
        }

        public override void Stop()
        {
            try
            {
                _cancellationTokenSource?.Cancel();
                _tcpListener?.Stop();
                
                System.Diagnostics.Debug.WriteLine($"Network port trigger '{Name}' stopped");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error stopping network port trigger '{Name}': {ex.Message}");
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
                        OnTriggerActivated("network_target", AppActionTypeEnum.Launch, null, new { Data = data, Client = client.Client.RemoteEndPoint });
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
            Stop();
            _cancellationTokenSource?.Dispose();
            base.Dispose();
        }

        public override TriggerModel ToModel()
        {
            return new TriggerModel
            {
                TriggerType = TriggerType,
                IsActive = IsActive,
                Port = Port,
                IPAddress = IPAddress,
                TimeoutMs = TimeoutMs,
                CustomProperties = CustomProperties
            };
        }
    }
}