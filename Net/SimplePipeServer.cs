namespace MultiWorldLib.Net
{
    using System;
    using System.IO.Pipes;
    using System.Threading;
    using System.Threading.Tasks;
    using Terraria;

    namespace MultiWorld.Net
    {
        public sealed class SimplePipeServer : IDisposable
        {
            public string Id { get; init; }
            private readonly NamedPipeServerStream _pipeServer;
            private bool _shouldStop = false;
            public delegate void OnRecieveData(int length, byte[] data);
            public event OnRecieveData RecieveDataEvent;

            public SimplePipeServer() : this($"{ModMultiWorld.PIPE_PREFIX}.{Guid.NewGuid()}") { }
            public SimplePipeServer(string pipeId)
            {
                Id = pipeId;
                _pipeServer = new NamedPipeServerStream(pipeId, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            }
            public async Task StartAndWaitAsync(CancellationToken? cancel = default)
            {
                if (_pipeServer.IsConnected)
                    return;
                cancel ??= CancellationToken.None;
                await _pipeServer.WaitForConnectionAsync(cancel.Value);
#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
                Task.Factory.StartNew(RecieveData);
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
            }
            public void StartWithoutWait()
            {
                if (_pipeServer.IsConnected)
                    return;
                Task.Factory.StartNew(() =>
                {
                    _pipeServer.WaitForConnection();
                    RecieveData();
                });
            }
            public async Task SendAsync(byte[] data)
            {
                if (!_pipeServer.IsConnected)
                    return;
                await _pipeServer.WriteAsync(data);
            }
            public void Send(byte[] data)
            {
                if (!_pipeServer.IsConnected)
                    return;
                _pipeServer.Write(data);
            }

            private void RecieveData()
            {
                var buf = new byte[MessageBuffer.readBufferMax];
                while (!_shouldStop)
                {
                    RecieveDataEvent?.Invoke(_pipeServer.Read(buf), buf);
                }
            }
            public void Dispose()
            {
                _shouldStop = true;
                _pipeServer?.Close();
                _pipeServer?.Dispose();
            }
        }
    }
}
