namespace Infrastructure.DTOs.LIS
{
    using System.Xml.Serialization;

    [XmlType("row")]
    public class EnumValueDto
    {
        [XmlElement("code")]
        public string Code;

        [XmlElement("systementityid")]
        public string SystemEntityId;

        [XmlElement("driversystemname")]
        public string DriverSystemName;
    }
}