using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace DeviceManagerService.Others
{
    public class Connection
    {
        public bool IsActive { get; set; }
        public IDriver Driver { get; set; }
        public Socket Socket { get; set; }
        // public ICommunication Communication { get; set; }
        // public CommunicationTypes CommunicationType { get; set; }

        // public enum CommunicationTypes
        // {
        //     Network = 0,
        //     Serial = 1,
        //     File = 2
        // }

        public async Task StartAsync(CancellationToken token)
        {
            await Driver.StartAsync(token);
            await ListenTcpIpPort(token);
        }
        
        private async Task ListenTcpIpPort(CancellationToken token)
        {
            var ipEndPoint = new IPEndPoint(IPAddress.Loopback, 5050); //todo
            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            if (false)
            {
                socket.ReceiveBufferSize = 1024;
                socket.SendBufferSize = 1024;
                socket.ReceiveTimeout = 500;
                socket.SendTimeout = 500;

                Console.WriteLine($"{DateTime.Now}\t" + $"Клиент запущен. Попытка соединения с сервером {ipEndPoint}...");
                socket.Connect(ipEndPoint.Address, ipEndPoint.Port);
                Console.WriteLine($"{DateTime.Now}\t" + $"Соединение успешно! Клиент {socket.LocalEndPoint}.");

            } // client
            else
            {
                socket.Bind(ipEndPoint);
                socket.Listen(1);
                socket.ReceiveBufferSize = 1024;
                socket.SendBufferSize = 1024;

                Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} Сервер запущен {ipEndPoint}. Ожидание подключений...");
            }

            var buffer = new byte[1024];
            while (true) //todo isActive
            {
                try
                {
                    using var client = await socket.AcceptAsync();
                    Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} Адрес подключенного клиента: {client.RemoteEndPoint}");

                    while (client.Connected)
                    {
                        var count = await client.ReceiveAsync(buffer, SocketFlags.None, token);
                        Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} Connection receive: {string.Join(", ", buffer.Take(count).Select(x => $"{x:X2}"))}");

                        if (Driver is null) continue;
                        try
                        {
                            var bytes = await Driver.WriteAsync(buffer.Take(count).ToArray());

                            await Socket.SendAsync(bytes, SocketFlags.None);
                            Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} Connection send: {string.Join(", ", bytes)}");
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {exception.Message}");
                            Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {exception.StackTrace}");
                        }
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {exception.Message}");
                    Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {exception.StackTrace}");
                    //todo await Driver.Clear(); 
                }
            }
        }
    }
}