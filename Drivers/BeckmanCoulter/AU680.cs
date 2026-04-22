namespace BeckmanCoulter
{
    using DriverBase;
    using DriverBase.DTOs;
    using Infrastructure.DTOs.LIS;
    using Serilog;
    using System;
    using System.Text;
    using System.Threading.Tasks;

    public class AU680 : IParser
    {
        public ILogger Logger { get; set; }
        public Encoding Encoding { get; set; }

        public bool IsRackNoCupPos { get; set; } = true;
        public string RackNoDigit { get; set; } = "4";
        public bool IsType { get; set; } = true;
        public int SampleIDDigit { get; set; } = 20;
        public bool IsSex { get; set; } = false;
        public bool IsAge { get; set; } = false;
        public int LengthPatientInformationBlock { get; set; } = 20;
        public int OnlineTestNo { get; set; } = 3;
        public int ResultDigit { get; set; } = 9;
        public int DataFlag { get; set; } = 2;
        public string Class { get; set; } = "B";
        public bool IsBCCCheck { get; set; } = true;

        public void Clear()
        {
            throw new NotImplementedException();
        }


        public OptionDTO[] GetOptions()
        {
            return new[]
            {
                new OptionDTO()
                {
                    Name = nameof(IsRackNoCupPos),
                    Description = "System -> Online -> Format Configuration -> Rack No./Cup pos.",
                    Value = IsRackNoCupPos,
                    Examples = new[] { "true", "false" },
                },
                new OptionDTO()
                {
                    Name = nameof(RackNoDigit),
                    Description = "System -> Online -> Format Configuration -> Rack No. Digit",
                    Value = RackNoDigit,
                    Examples = new[] { "4", "5" },
                },
                new OptionDTO()
                {
                    Name = nameof(IsType),
                    Description = "System -> Online -> Format Configuration -> Type",
                    Value = IsType,
                    Examples = new[] { "true", "false" },
                },
                new OptionDTO()
                {
                    Name = nameof(SampleIDDigit),
                    Description = "System -> Format -> Requisition Format -> Sample ID",
                    Value = SampleIDDigit,
                    Examples = new[] { "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26" },
                },
                new OptionDTO()
                {
                    Name = nameof(IsSex),
                    Description = "System -> Format -> Requisition Format -> Sex",
                    Value = IsSex,
                    Examples = new[] { "true", "false" },
                },
                new OptionDTO()
                {
                    Name = nameof(IsAge),
                    Description = "System -> Format -> Requisition Format -> Age",
                    Value = IsAge,
                    Examples = new[] { "true", "false" },
                },
                new OptionDTO()
                {
                    Name = nameof(LengthPatientInformationBlock),
                    Description = "System -> Format -> Requisition Format -> Patient Information (Необходима сумма Digit включенных блоков)",
                    Value = LengthPatientInformationBlock,
                    Examples = new[] { "0", "20", "40", "60", "80", "100", "120" },
                },
                new OptionDTO()
                {
                    Name = nameof(OnlineTestNo),
                    Description = "System -> Online -> Format Configuration -> Online Test No. Digit",
                    Value = OnlineTestNo,
                    Examples = new[] { "2", "3" },
                },
                new OptionDTO()
                {
                    Name = nameof(ResultDigit),
                    Description = "System -> Online -> Format Configuration -> Result Digit",
                    Value = ResultDigit,
                    Examples = new[] { "6", "9" },
                },
                new OptionDTO()
                {
                    Name = nameof(DataFlag),
                    Description = "System -> Online -> Format Configuration -> No. of Data Flags",
                    Value = DataFlag,
                    Examples = new[] { "2", "4" },
                },
                new OptionDTO()
                {
                    Name = nameof(IsBCCCheck),
                    Description = "System -> Online -> Protocol -> BCC Check",
                    Value = IsBCCCheck,
                    Examples = new[] { "true", "false" },
                },
                new OptionDTO()
                {
                    Name = nameof(Class),
                    Description = "System -> Online -> Protocol -> Class",
                    Value = Class,
                    Examples = new[] { "A", "B" },
                },
            };
        }

        public void SetOptions(OptionDTO[] options)
        {
            foreach (var option in options)
            {
                switch (option.Name)
                {
                    case nameof(IsRackNoCupPos):
                        IsRackNoCupPos = bool.Parse($"{option.Value}");
                        break;
                    case nameof(RackNoDigit):
                        RackNoDigit = $"{option.Value}";
                        break;
                    case nameof(IsType):
                        IsType = bool.Parse($"{option.Value}");
                        break;
                    case nameof(SampleIDDigit):
                        SampleIDDigit = int.Parse($"{option.Value}");
                        break;
                    case nameof(IsSex):
                        IsSex = bool.Parse($"{option.Value}");
                        break;
                    case nameof(IsAge):
                        IsAge = bool.Parse($"{option.Value}");
                        break;
                    case nameof(LengthPatientInformationBlock):
                        LengthPatientInformationBlock = int.Parse($"{option.Value}");
                        break;
                    case nameof(OnlineTestNo):
                        OnlineTestNo = int.Parse($"{option.Value}");
                        break;
                    case nameof(ResultDigit):
                        ResultDigit = int.Parse($"{option.Value}");
                        break;
                    case nameof(DataFlag):
                        DataFlag = int.Parse($"{option.Value}");
                        break;
                    case nameof(Class):
                        Class = $"{option.Value}";
                        break;
                    case nameof(IsBCCCheck):
                        IsBCCCheck = bool.Parse($"{option.Value}");
                        break;
                }
            }
        }


        public Task<ParserMessage> WriteAsync(byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public Task<ParserMessage> ReadAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ParserMessage> WriteAsync(DeviceOrderDTO[] orders)
        {
            throw new NotImplementedException();
        }
    }
}