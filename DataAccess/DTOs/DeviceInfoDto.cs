namespace DataAccess.DTOs
{
    using System.Xml.Serialization;

    [XmlType("row")]
    public class DeviceInfoDto
    {
        [XmlElement("driversystemname")]
        public string DriverSystemName { get; set; }

        [XmlElement("id")]
        public string Id { get; set; }

        [XmlElement("iddriver")]
        public string IdDriver { get; set; }

        [XmlElement("workmode")]
        public string WorkMode { get; set; }

        [XmlElement("lpu")]
        public string LpuId { get; set; }

        [XmlElement("Name")]
        public string Name { get; set; }

        [XmlElement("options")]
        public string Options { get; set; }

        [XmlElement("systemname")]
        public string SystemName { get; set; }

        [XmlElement("isactive")]
        public string IsActive { get; set; }
    }
}