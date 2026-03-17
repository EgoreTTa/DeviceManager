namespace DataAccess.DTOs.LIS
{
    using System.Xml.Serialization;

    [XmlType("lines")]
    [XmlSchemaProvider("XmlSchemaProvider")]
    public class DirectionLineDTO
    {
        [XmlElement("id")]
        public string _id;

        [XmlElement("idmethod")]
        public string _idmethod;

        [XmlElement("idsample")]
        public string _idsample;

        [XmlElement("iddevice")]
        public string _iddevice;

        [XmlElement("devicesystemname")]
        public string _devicesystemname;

        [XmlElement("samplebarcode")]
        public string _samplebarcode;

        [XmlElement("patientage")]
        public string _patientage;

        [XmlElement("patientid")]
        public string _patientid;

        [XmlElement("patientextid")]
        public string _patientextid;

        [XmlElement("patientname1")]
        public string _patientname1;

        [XmlElement("patientname2")]
        public string _patientname2;

        [XmlElement("patientname3")]
        public string _patientname3;

        [XmlElement("patientsex")]
        public string _patientsex;

        [XmlElement("patientdepartment")]
        public string _patientdepartment;

        [XmlElement("tests")]
        public TestDto[] _testDTOs;

        [XmlElement("requestedbarcode")]
        public string _requestedbarcode;

        [XmlElement("patientbirthdate")]
        public string _patientbirthdate;

        [XmlElement("idbiomaterialtype")]
        public string _idbiomaterialtype;

        [XmlElement("samplingdatetime")]
        public string _samplingdatetime;

        [XmlElement("ordereddate")]
        public string _ordereddate;

        [XmlElement("senderinfo")]
        public string _senderinfo;

        [XmlElement("senderorganization")]
        public string _senderorganization;

        [XmlElement("creatorsharedid")]
        public string _creatorsharedid;

        [XmlElement("alternativebarcode")]
        public string _alternativebarcode;
    }
}