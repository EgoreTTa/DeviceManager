namespace Infrastructure.DTOs.LIS
{
    using System.Xml.Serialization;

    [XmlType("row")]
    public class EnumValueDto
    {
        [XmlElement("code")]
        public string _code;

        [XmlElement("systementityid")]
        public string _systemEntityId;

        [XmlElement("driversystemname")]
        public string _driverSystemName;
    }
}