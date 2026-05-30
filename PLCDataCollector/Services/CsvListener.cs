using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PLCDataCollector.Services
{
    public class CsvReceivedEventArgs : EventArgs
    {
        public string SourceIp { get; set; } = "";
        public string CsvText { get; set; } = "";
    }

    public class CsvListener : IDisposable
    {
        private readonly int _port;
        private TcpListener? _listener;
        private CancellationTokenSource? _cts;
        private readonly LoggingService? _log;

        public event EventHandler<CsvReceivedEventArgs>? CsvReceived;

        public CsvListener(int port, LoggingService? log = null)
        {
            _port = port;
            _log = log;
        }

        public void Start()
        {
            _cts = new CancellationTokenSource();
            _listener = new TcpListener(IPAddress.Any, _port);
            _listener.Start();
            _log?.Info($"CsvListener started on port {_port}");
            Task.Run(() => AcceptLoop(_cts.Token));
        }

        public void Stop()
        {
            _cts?.Cancel();
            _listener?.Stop();
        }

        private async Task AcceptLoop(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var client = await _listener!.AcceptTcpClientAsync(token).ConfigureAwait(false);
                    _ = Task.Run(() => HandleClient(client, token));
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception) { }
        }

        private async Task HandleClient(TcpClient client, CancellationToken token)
        {
            using var c = client;
            try
            {
                var ep = c.Client.RemoteEndPoint as IPEndPoint;
                var ip = ep?.Address.ToString() ?? "unknown";
                using var ns = c.GetStream();
                using var ms = new System.IO.MemoryStream();
                var buffer = new byte[4096];
                int read;
                while ((read = await ns.ReadAsync(buffer, 0, buffer.Length, token).ConfigureAwait(false)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                var text = Encoding.UTF8.GetString(ms.ToArray());
                _log?.Info($"CSV received from {ip}, {text.Length} bytes");
                CsvReceived?.Invoke(this, new CsvReceivedEventArgs { SourceIp = ip, CsvText = text });
            }
            catch (Exception) { }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
