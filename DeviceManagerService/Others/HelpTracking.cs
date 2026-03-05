using DataAccess;
using DriverBase;
using Newtonsoft.Json.Linq;
using System;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

namespace DeviceManagerService.Others
{
    public class HelpTracking
    {
        public Device Device { get; set; }
        // public Device Device { get; set; }
        public CancellationTokenSource TokenSource { get; set; }
        public Task Task { get; set; }
    }
}