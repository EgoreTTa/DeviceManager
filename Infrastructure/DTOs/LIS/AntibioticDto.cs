namespace Infrastructure.DTOs.LIS
{
    using System.Xml.Serialization;

    [XmlType("row")]
    public class AntibioticDto
    {
        [XmlElement("code")]
        public string _code;

        [XmlElement("systementityid")]
        public string _systemEntityId;

        [XmlElement("driversystemname")]
        public string _driverSystemName;

        [XmlElement("antibioticid")]
        public string _antibioticId;
    }
}