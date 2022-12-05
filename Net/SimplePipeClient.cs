using System.IO.Pipes;
using System.Threading.Tasks;
using Terraria;

namespace MultiWorld.Net
{
    public class SimplePipeClient
    {
        public string Id { get; init; }
        public delegate void OnRecieveData(int length, byte[] data);
        public event OnRecieveData RecieveDataEvent;
        private readonly NamedPipeClientStream _pipeClient;
        private bool _shouldStop = false;

        public SimplePipeClient(string pipeId)
        {
            Id = pipeId;
            _pipeClient = new(".", pipeId, PipeDirection.InOut, PipeOptions.Asynchronous);
        }
        public async Task StartAsync()
        {
            if (_pipeClient.IsConnected)
                return;
            await _pipeClient.ConnectAsync();
#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
            Task.Factory.StartNew(RecieveData);
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
        }
        public void Start()
        {
            if (_pipeClient.IsConnected)
                return;
            _pipeClient.Connect();
#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
            Task.Factory.StartNew(RecieveData);
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
        }
        public async Task SendAsync(byte[] data)
        {
            await _pipeClient.WriteAsync(data);
        }
        public void Send(byte[] data)
        {
            SendAsync(data).Wait();
        }
        private void RecieveData()
        {
            var buf = new byte[MessageBuffer.readBufferMax];
            while (!_shouldStop)
            {
                RecieveDataEvent?.Invoke(_pipeClient.Read(buf), buf);
            }
        }
        public void Dispose()
        {
            _shouldStop = true;
            _pipeClient?.Close();
            _pipeClient?.Dispose();
        }
    }
}
