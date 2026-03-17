namespace DeviceManagerHubService
{
    using Microsoft.AspNetCore.SignalR;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    public class DeviceManagerHubService : IDeviceManagerHubService
    {
        private readonly ConcurrentQueue<string> _logs = new ConcurrentQueue<string>();
        private readonly IHubContext<DataHub> _hub;
        private const int MaxLines = 10;

        public DeviceManagerHubService(IHubContext<DataHub> hub) => _hub = hub;

        public void AddLog(string message)
        {
            // Добавляем
            _logs.Enqueue(message);
            // Удаляем старое, если > 10
            while (_logs.Count > MaxLines) _logs.TryDequeue(out _);

            // Отправляем только новую строку (эффективнее)
            _hub.Clients.All.SendAsync("ReceiveLog", message);
        }

        public IEnumerable<string> GetHistory() => _logs.ToList();
    }
}