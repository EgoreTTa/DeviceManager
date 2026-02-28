namespace API.Controllers.Drivers
{
    public class FormDriver
    {
        public string FileName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string DateTimeBuild { get; set; } = string.Empty;
        public string[] WorkModes { get; set; }
    }
}