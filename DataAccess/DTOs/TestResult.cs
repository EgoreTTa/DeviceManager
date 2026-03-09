namespace DataAccess.DTOs
{
    public class TestResult
    {
        public int Id { get; set; }
        public int DeviceId { get; set; }
        public string SampleCode { get; set; }
        public Result[] Results { get; set; }
    }
}