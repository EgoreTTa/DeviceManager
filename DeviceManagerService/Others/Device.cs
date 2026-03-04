namespace DeviceManagerService.Others
{
    using System.Threading;
    using System.Threading.Tasks;

    public class Device
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SystemName { get; set; } = string.Empty;
        public IDriver Driver { get; set; }
        public Connection Connection { get; set; } = null;
        // public DataAccess DataAccess { get; set; } = null;
        public bool IsActive { get; set; } = false;

        public async Task StartAsync(CancellationToken token)
        {
            // if (Driver is null) return;
            // if (DataAccess is null) return;
            // if (Connection is null) return;
            Driver = new Driver
            {
                // DataAccess = new DataAccess("http://192.168.241.141/med2des/ws/lis", "SystemName_Device0001", "SystemName_Driver0001")
            };
            Connection = new Connection
            {
                Driver = Driver
            };
            Driver.Connection = Connection;

            // Connection.SetDriver(Driver);
            // Driver.SetConnection(Connection);
            // Driver.SetDataAccess(DataAccess);

            var connectionTask = Connection.StartAsync(token);

            await connectionTask;
        }
    }
}