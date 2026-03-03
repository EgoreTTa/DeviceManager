using System.Threading;
using System.Threading.Tasks;

namespace DeviceManagerWorker
{
    public class HelpTracking
    {
        public Device Device { get; set; }
        public CancellationTokenSource TokenSource { get; set; }
        public Task Task { get; set; }
    }
}