namespace DataAccess.DTOs
{
    using System.Xml.Serialization;

    [XmlType("SaveDeviceResults")]
    public class SaveDeviceResultsRequest
    {
        [XmlElement("devicesystemname")]
        public string DeviceSystemName { get; set; } = default!;

        [XmlElement("lines")]
        public DirectiveLine[] DirectiveLines { get; set; }

    }
}