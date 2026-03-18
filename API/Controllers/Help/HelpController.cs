namespace API.Controllers.Help
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Win32;

    [ApiController]
    [Route("[controller]/")]
    public class HelpController : ControllerBase
    {
        [HttpGet(nameof(GetPorts))]
        public string[] GetPorts()
        {
            var ports = new List<string>();
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                    foreach (var valueName in Registry.LocalMachine
                                                      .OpenSubKey("HARDWARE")
                                                      .OpenSubKey("DEVICEMAP")
                                                      .OpenSubKey("SERIALCOMM")
                                                      .GetValueNames())
                    {
                        ports.Add(Registry.LocalMachine
                                          .OpenSubKey("HARDWARE")
                                          .OpenSubKey("DEVICEMAP")
                                          .OpenSubKey("SERIALCOMM")
                                          .GetValue(valueName)
                                          .ToString());
                    }

                    break;
                case PlatformID.Unix:
                    ports.AddRange(Directory.GetFiles("/dev/", "ttyS*"));
                    ports.AddRange(Directory.GetFiles("/dev/", "ttyACM*"));
                    ports.AddRange(Directory.GetFiles("/dev/", "ttyUSB*"));
                    break;
            }

            return ports.OrderBy(x => x).ToArray();
            // ports.Where(x =>
            //      {
            //          try
            //          {
            //              var serial = new SerialPort(x);
            //              serial.Open();
            //              serial.Close();
            //              return true;
            //          }
            //          catch (Exception exception)
            //          {
            //              return false;
            //          }
            //      })
            //      .OrderBy(x => x)
            //      .ToArray();
        }

        [HttpGet(nameof(GetOSPlatform))]
        public string GetOSPlatform()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                    return "Windows";
                case PlatformID.Unix:
                    return "Linux";
                default:
                    return "Unknow";
            }
        }
    }
}