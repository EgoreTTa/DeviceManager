namespace DataAccess.DTOs
{
    using System.Xml.Serialization;

    public class DirectiveLine
    {
        [XmlElement("id")]
        public string Id { get; set; } = default!;

        [XmlElement("tests")]
        public ResearchResult[] ResearchResults { get; set; }

        [XmlElement("creatorsharedid")]
        public string CreatorSharedId { get; set; } = default!;

    }
}