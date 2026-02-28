namespace DataAccess.DTOs
{

    using System.Xml.Serialization;

    [XmlType("row")]
    public class TestCollationDto
    {
        [XmlElement("code")]
        public string _code;

        [XmlElement("deviceid")]
        public string _deviceId;

        [XmlElement("systementityid")]
        public string _systemEntityId;

        [XmlElement("driversystemname")]
        public string _driverSystemName;

        [XmlElement("devicesystemname")]
        public string _devicesystemname;
    }
}