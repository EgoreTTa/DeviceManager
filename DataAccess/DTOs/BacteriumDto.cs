namespace DataAccess.DTOs
{
    using System.Xml.Serialization;

    [XmlType("row")]
    public class BacteriumDto
    {
        [XmlElement("code")]
        public string _code;

        [XmlElement("systementityid")]
        public string _systemEntityId;

        [XmlElement("driversystemname")]
        public string _driverSystemName;

        [XmlElement("microorgid")]
        public string _microorgId;
    }
}