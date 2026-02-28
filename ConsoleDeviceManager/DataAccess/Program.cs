namespace DataAccess
{
    using DTOs;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    public class Program
    {
        private static async Task Main()
        {
            var dataAccess = new DataAccess("http://10.31.6.59/inst/ws/lis" ,"SystemName_Device0001", "SystemName_Driver0001");
            await dataAccess.SetDeviceResults(new[]
            {
                new TestResult
                {
                    SampleCode = "260325-0001",
                    Results = new[]
                    {
                        new Result
                        {
                            TestCode = "Test1TestCode2",
                            Value = "20",
                            MuCode = "TestMUCode2,4"
                        },
                    },
                },
            });

            var testCollations = dataAccess.GetDriverTestCollations().Result;
            var maxLengthCode = testCollations.Select(x => x.code.Length).Max();
            var maxLengthId = testCollations.Select(x => x.id.Length).Max();

            const string testCollationCode = "Код показателя на устройстве";
            const string testCollationId = "ID показателя в системе";
            maxLengthCode = maxLengthCode > testCollationCode.Length ? maxLengthCode : testCollationCode.Length;
            maxLengthId = maxLengthId > testCollationId.Length ? maxLengthId : testCollationId.Length;
            Console.WriteLine($"{testCollationCode.PadLeft(maxLengthCode)}" + '\t' + $"{testCollationId.PadLeft(maxLengthId)}");

            foreach (var testCollation in testCollations)
            {
                Console.WriteLine($"{testCollation.code.PadLeft(maxLengthCode)}" + '\t' + $"{testCollation.id.PadLeft(maxLengthId)}");
            }
        }
    }
}
