namespace DriverBase.DTOs
{
    public class OptionDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public object Value { get; set; }
        public string[] Examples { get; set; }
    }
}