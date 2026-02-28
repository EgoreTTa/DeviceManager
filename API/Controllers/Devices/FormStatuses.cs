namespace API.Controllers.Devices
{
    public class FormStatus
    {
        public string DeviceName { get; set; }
        public string CountMessagesQuery { get; set; }
        public string CountMessagesOrder { get; set; }
        public string CountMessagesResult { get; set; }
        public string CountMessagesError { get; set; }
    }
}