namespace Core.Devices.Components.Connect
{
    using Core.Configurations.Device.Connection;
    using Serilog;
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public sealed class FileSystemConnect : IConnection
    {
        private readonly string _folderToRead;
        private readonly string _folderToReaded;
        private readonly string _folderToWrite;
        private readonly string _folderToWrited;
        private readonly int _intervalForReadInSecond;

        public ILogger Logger { get; set; }

        public FileSystemConnect(ILogger logger, FileSystemConnection configuration)
        {
            Logger = logger;

            _folderToRead = configuration.FolderToRead;
            _folderToReaded = Path.Combine(configuration.FolderToRead, "Readed");
            _folderToWrite = configuration.FolderToWrite;
            _folderToWrited = Path.Combine(configuration.FolderToWrite, "Writed");
            _intervalForReadInSecond = configuration.IntervalToReadInSecond;
            Logger.Information($"filesystem connect create.");
        }

        public Task StartAsync(CancellationToken token)
        {
            Directory.CreateDirectory(_folderToRead);
            Directory.CreateDirectory(_folderToWrite);
            Logger.Information($"filesystem connect start.");
            return Task.CompletedTask;
        }

        public void Stop() => Logger.Information($"filesystem connect stop.");

        public async Task<byte[]> ReadAsync(CancellationToken token)
        {
            try
            {
                var files = Directory.GetFiles(_folderToRead);
                var filesInfo = files.Select(x => new FileInfo(x))
                                     .OrderByDescending(x => x.CreationTime);
                
                if (files.Any() is false)
                {
                    await Task.Delay(TimeSpan.FromSeconds(_intervalForReadInSecond), token);
                    throw new Exception("No files for read.");
                }

                var nextFile = filesInfo.First();

                var bytes = await File.ReadAllBytesAsync(nextFile.FullName, token);
                Logger.Debug($"filesystem read: {nextFile.Name}:{string.Join(", ", bytes.Select(x => $"{x:X2}"))}");
                nextFile.MoveTo(_folderToReaded);
                return bytes;
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message);
                throw exception;
            }
        }

        public async Task WriteAsync(byte[] bytes, CancellationToken token)
        {
            try
            {
                var fileName = Path.Combine(_folderToWrite, "Send");
                await File.WriteAllBytesAsync(fileName, bytes, token);
                Logger.Debug($"filesystem send: {fileName}:{string.Join(", ", bytes.Select(x => $"{x:X2}"))}");
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message);
                throw new Exception("No data for write.");
            }
        }
    }
}