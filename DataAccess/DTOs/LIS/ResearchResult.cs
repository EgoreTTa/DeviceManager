namespace DataAccess.DTOs.LIS
{
    using System.Xml.Serialization;

    public class ResearchResult
    {
        [XmlElement("resulttypedata")]
        public string ResultTypeData { get; set; } = default!;

        [XmlElement("testid")]
        public string TestId { get; set; } = default!;

        [XmlElement("muid")]
        public string MUId { get; set; } = default!;

        [XmlElement("value")]
        public string Value { get; set; } = default!;
    }
}