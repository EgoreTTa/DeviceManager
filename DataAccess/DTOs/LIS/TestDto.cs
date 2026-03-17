namespace DataAccess.DTOs.LIS
{
    using System.Xml.Serialization;

    [XmlType("tests")]
    public class TestDto
    {
        [XmlElement("resulttypedata")]
        public string _resultTypeData;

        [XmlElement("testid")]
        public string _testId;

        [XmlElement("muid")]
        public string _muId;

        [XmlElement("value")]
        public string _value;
    }
}