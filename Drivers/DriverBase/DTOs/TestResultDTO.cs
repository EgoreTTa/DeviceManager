namespace DriverBase.DTOs
{
    public class TestResultDTO
    {
        public int Id { get; set; }
        public string DeviceSystemName { get; set; }
        public string DriverSystemName { get; set; }
        public string DateTime { get; set; } = $"{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        public string Status { get; set; } // ok, waiting, error
        public string SampleCode { get; set; }
        public string TestCode { get; set; }
        public string Value { get; set; }
        public string MuCode { get; set; }
    }
}