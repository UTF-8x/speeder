using System.Diagnostics;

namespace Speeder.Infra.Impl;

public class Iperf3Adapter(ILogger<Iperf3Adapter> log, ServerPool pool, IConfiguration config) : ISpeedTestAdapter
{
    private readonly string _iperfExePath = config.GetRequiredSection("Iperf")["ExePath"]
        ?? throw new ApplicationException("missing required configuration value 'Iperf:ExePath'");

    public Task<SpeedTestResult?> MeasureAsync(CancellationToken cancel)
    {
        while(!cancel.IsCancellationRequested)
        {
            var nextServer = pool.GetNextAvailable();
            if (nextServer is null) {
                log.LogWarning("no servers available");
                return Task.FromResult<SpeedTestResult?>(null);
            }

            log.LogDebug("trying server {Server}", nextServer.Hostname);
            var result = RunIperf3Test(nextServer);

            if(result is null || result.End is null || result.Error is not null)
            {
                log.LogWarning("server {Server} did not work", nextServer.Hostname);
                return Task.FromResult<SpeedTestResult?>(null);
            }

            log.LogDebug("speed test finished with bandwidth: {BW}; latency: {Lat}", result.End.ReceivedSum.BitsPerSecond, CalculateAverageLatency(result));

            return Task.FromResult<SpeedTestResult?>(new SpeedTestResult
            {
                DownloadSpeed = result.End.ReceivedSum.BitsPerSecond,
                UploadSpeed = result.End.SentSum.BitsPerSecond,
                UpLatency = -1,
                DownLatency = -1,
                UpJitter = -1,
                DownJitter = -1
            });
        }

        log.LogWarning("task cancelled");
        return Task.FromResult<SpeedTestResult?>(null);
    }

    private Iperf3Result? RunIperf3Test(ServerPool.Iperf3Server server)
    {
        var args = $"-c {server.Hostname} -p {server.PortRange} -J"; // Use JSON output for easier parsing

        log.LogInformation("starting a speed test against {Server}", server.Hostname);

        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _iperfExePath,
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
                log.LogError($"iPerf3 failed: ({process.ExitCode}) {process.StandardError.ReadToEnd()}");
                return null;
            }

            return ParseIperfOutput(output);
        }
        catch (Exception ex)
        {
            log.LogError($"Error running iPerf3: {ex.Message}");
            return null;
        }
    }

    private Iperf3Result? ParseIperfOutput(string output)
    {
        try
        {
            var result = System.Text.Json.JsonSerializer.Deserialize<Iperf3Result>(output);
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing iPerf3 output: {ex.Message}");
            return null;
        }
    }

    public int CalculateAverageLatency(Iperf3Result result)
    {
        var latencies = new List<double>();

        foreach (var interval in result.Intervals)
        {
            if (interval.Sum.Bytes > 0)
            {
                double latencyPerByte = interval.Sum.Seconds / interval.Sum.Bytes;
                latencies.Add(latencyPerByte);
            }
        }

        if (latencies.Any())
        {
            double averageLatencyInSeconds = latencies.Average();
            int averageLatencyInMillis = (int)(averageLatencyInSeconds * 1000); // Convert to ms
            return averageLatencyInMillis;
        }

        return -1;
    }
}