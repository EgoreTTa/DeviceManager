namespace DataAccess.DTOs
{
    using System.Xml.Serialization;

    [XmlType("deviceorder")]
    public class DeviceOrderDTO
    {
        [XmlElement("id")]
        public string _id;

        [XmlElement("devicesystemname")]
        public string _deviceSystemName;

        [XmlElement("lines")]
        public DirectionLineDTO[] _directionLines;
    }
}