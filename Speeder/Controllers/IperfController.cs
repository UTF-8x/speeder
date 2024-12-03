using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Speeder.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class IperfController : ControllerBase 
{
    private const string IperfServer = "ams.speedtest.clouvider.net";
    private const string IperfPort = "5200-5209";

    private const string IperfExePath = @"C:\Users\utf8x\Downloads\iperf3.17.1_64\iperf3.exe";

    [HttpGet]
    public IActionResult Run()
    {
        var args = $"-c {IperfServer} -p {IperfPort} -J";

        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = IperfExePath,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
                return StatusCode(500, new { Error = error });

            return Ok(new { Output = output });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = ex.Message });
        }
    }
}