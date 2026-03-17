namespace DataAccess.DTOs.LIS
{
    using System.Xml.Serialization;

    [XmlType("status")]
    public class StatusDto
    {
        [XmlElement("id")]
        public string _id;

        [XmlElement("ishiglited")]
        public string _ishiglited;

        [XmlElement("isworktestdatavalid")]
        public string _isworktestdatavalid;

        [XmlElement("status")]
        public string _status;

        [XmlElement("tests")]
        public TestDto[] _tests;
    }
}