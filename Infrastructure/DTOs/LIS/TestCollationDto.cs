namespace Infrastructure.DTOs.LIS
{
    using System.Xml.Serialization;

    [XmlType("row")]
    public class TestCollationDto
    {
        [XmlElement("code")]
        public string Code { get; set; }

        [XmlElement("deviceid")]
        public string DeviceId { get; set; }

        [XmlElement("systementityid")]
        public string SystemEntityId { get; set; }

        [XmlElement("driversystemname")]
        public string DriverSystemName { get; set; }

        [XmlElement("devicesystemname")]
        public string DeviceSystemName { get; set; }
    }
}