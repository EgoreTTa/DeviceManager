namespace DataAccess.DTOs.LIS
{
    using System.Xml.Serialization;

    [XmlType("row")]
    public class BiomaterialDto
    {
        [XmlElement("code")]
        public string _code;

        [XmlElement("systementityid")]
        public string _systemEntityId;

        [XmlElement("driversystemname")]
        public string _driverSystemName;
    }
}