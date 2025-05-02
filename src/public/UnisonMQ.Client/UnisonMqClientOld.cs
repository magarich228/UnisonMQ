using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace UnisonMQ.Client
{
    internal class UnisonMqClientOld// : IUnisonMqClient
    {
        private readonly UnisonMqConfiguration _configuration;

        private readonly TcpClient _client;
        private NetworkStream? _networkStream;

        public UnisonMqClientOld(string hostname, int port) : this(new UnisonMqConfiguration()
        {
            Ip = hostname,
            Port = port
        })
        {
        }

        public UnisonMqClientOld(UnisonMqConfiguration configuration)
        {
            _configuration = configuration;

            _client = new TcpClient();
            _client.NoDelay = true;
        }

        public async Task ConnectAsync()
        {
            if (!_client.Connected)
            {
                await _client.ConnectAsync(
                    _configuration.Ip,
                    _configuration.Port);

                _networkStream = _client.GetStream();
            }
        }

        public Task CloseAsync()
        {
            _client.Close();
            _networkStream?.Close();

            return Task.CompletedTask;
        }

        public async Task<bool> PingAsync(CancellationToken token = default)
        {
            if (!IsConnected())
                return false;
            
            var message = "ping";
            var bytes = System.Text.Encoding.UTF8.GetBytes(message);
            
            await _networkStream!.WriteAsync(bytes.AsMemory(), token);

            throw new NotImplementedException();
        }

        protected virtual void CheckConnection()
        {
            var message = $"UnisonMQ: No connection established. Use {nameof(ConnectAsync)}() first.";
            
            if (!IsConnected())
            {
                throw new UnisonMqClientException(message);
            }
        }

        private bool IsConnected()
        {
            return _networkStream != null &&
                   _client.Connected;
        }

        #region IAsyncDisposable

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore();
            GC.SuppressFinalize(this);
        }

        protected virtual async ValueTask DisposeAsyncCore()
        {
            _client.Dispose();

            if (_networkStream is { } networkStream)
                await networkStream.DisposeAsync();
        }

        #endregion
    }
}