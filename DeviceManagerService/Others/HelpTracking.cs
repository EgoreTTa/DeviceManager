using System.Threading;
using System.Threading.Tasks;

namespace DeviceManagerService.Others
{
    public class HelpTracking
    {
        public Device Device { get; set; }
        public CancellationTokenSource TokenSource { get; set; }
        public Task Task { get; set; }
    }
}