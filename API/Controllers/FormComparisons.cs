namespace API.Controllers
{
    public class FormComparisons
    {
        public Antibiotic[] Antibiotics { get; set; }
        public Biomaterial[] Biomaterials { get; set; }
        public Locus[] Loci { get; set; }
        public UnitMeasurement[] UnitsMeasurement { get; set; }
        public Microorganism[] MicroOrganisms { get; set; }
        public EnumerableTestValue[] EnumerableTestValues { get; set; }
        public ResultsTest[] ResultsTests { get; set; }

        public class Antibiotic
        {
            public int Id { get; set; }
            public int Value { get; set; }
        }
        public class Biomaterial
        {
            public int Id { get; set; }
            public int Value { get; set; }
        }
        public class Locus
        {
            public int Id { get; set; }
            public int Value { get; set; }
        }
        public class UnitMeasurement
        {
            public int Id { get; set; }
            public int Value { get; set; }
        }
        public class Microorganism
        {
            public int Id { get; set; }
            public int Value { get; set; }
        }
        public class EnumerableTestValue
        {
            public int Id { get; set; }
            public int Value { get; set; }
        }
        public class ResultsTest
        {
            public int Id { get; set; }
            public int Value { get; set; }
        }
    }
}