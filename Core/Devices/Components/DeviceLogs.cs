namespace Core.Devices.Components
{
    using Serilog.Core;
    using Serilog.Events;
    using Serilog.Formatting.Display;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public sealed class DeviceLogs : ILogEventSink
    {
        private readonly Queue<LogEvent> _queue;
        private readonly int _limit;
        private readonly MessageTemplateTextFormatter _formatter;

        public string[] Logs => _queue.Select(x =>
        {
            var sw = new StringWriter();
            _formatter.Format(x, sw);
            return sw.ToString();
        }).ToArray();

        public DeviceLogs(int limit, string outputTemplate = "{Timestamp:HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
        {
            _limit = limit;
            _formatter = new MessageTemplateTextFormatter(outputTemplate, null);
            _queue = new Queue<LogEvent>(_limit);
        }

        public void Emit(LogEvent logEvent)
        {
            if (_queue.Count == _limit) _queue.Dequeue();
            _queue.Enqueue(logEvent);
        }
    }
}