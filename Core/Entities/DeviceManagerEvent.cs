namespace Core.Entities
{
    public class DeviceManagerEvent
    {
        public int Id { get; set; }
        public string DateTime { get; set; } = $"{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        public string Description { get; set; }
        public string Details { get; set; }

        public DeviceManagerEvent(string description, string details = null)
        {
            Description = description;
            Details = details;
        }
    }
}