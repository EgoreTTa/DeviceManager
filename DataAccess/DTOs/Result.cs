namespace DataAccess.DTOs
{
    public class Result
    {
        public int Id { get; set; }
        public int TestResultId { get; set; }
        public string DateTime { get; set; } = $"{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        public string TestCode { get; set; }
        public string Value { get; set; }
        public string MuCode { get; set; }
    }
}