
using System.Diagnostics;
using System.Text.Json;

namespace Speeder.Infra.Impl;

public class OoklaAdapter(ILogger<OoklaAdapter> log, IConfiguration config) : ISpeedTestAdapter
{
    private readonly string _speedtestExe = config.GetRequiredSection("Ookla")["ExePath"]
        ?? throw new ApplicationException("missing required configuration section Ookla:ExePath");

    public Task<SpeedTestResult?> MeasureAsync(CancellationToken cancel)
    {
        while(!cancel.IsCancellationRequested)
        {
            var result = RunOoklaTest();
            if(result is null || result.Error is not null)
            {
                log.LogError("could not run Ookla speed test");
                if (result is not null && result.Error is not null) log.LogError(result.Error);
                return Task.FromResult<SpeedTestResult?>(null);
            }

            return Task.FromResult<SpeedTestResult?>(new SpeedTestResult
            {
                DownloadSpeed = result.Download.Bandwidth,
                UploadSpeed = result.Upload.Bandwidth,
                AverageLatency = (int)result.Download.Latency.Iqm,
                AverageJitter = (int)result.Download.Latency.Jitter,
                Latencies = [(int)result.Download.Latency.Iqm],
                Jitters = [(int)result.Download.Latency.Jitter]
            });
        }

        log.LogWarning("task cancelled");
        return Task.FromResult<SpeedTestResult?>(null);
    }

    private OoklaResult? RunOoklaTest()
    {
        var args = "--format json";

        log.LogInformation("starting an Ookla speed test");

        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _speedtestExe,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();

            var output = process.StandardOutput.ReadToEnd();
            log.LogDebug(output);
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                log.LogError("test failed with exit code {Code}: {Output}", process.ExitCode, process.StandardError.ReadToEnd());
                return null;
            }

            return ParseOutput(output);
        }
        catch (Exception ex) 
        {
            log.LogError("could not run speed test: {Message}", ex.Message);
            return null;
        }
    }

    private OoklaResult? ParseOutput(string output)
    {
        try
        {
            return JsonSerializer.Deserialize<OoklaResult>(output);
        }
        catch (Exception ex)
        {
            log.LogError("could not parse speedtest output: {Message}", ex.Message);
            return null;
        }
    }
}