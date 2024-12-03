using System.Diagnostics;
using System.Text.Json.Serialization;
using Prometheus;

namespace Speeder.Services;

public class IperfService(ILogger<IperfService> log) : BackgroundService
{
    private readonly Gauge _iperfLatencyGauge = Metrics.CreateGauge("iperf_latency_ms", "latency in ms");
    private readonly Gauge _iperfBandwidthGauge = Metrics.CreateGauge("iperf_bandwidth_bps", "bandwidth in bps");
    private const string IperfServer = "iperf.worldstream.nl";
    private const string IperfPort = "5201";
    private const string IperfExePath = @"C:\Users\utf8x\Downloads\iperf3.17.1_64\iperf3.exe";

    private const int DelayMinutes = 1;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {   
            var result = RunIperf3Test();
            if (result != null)
            {
                _iperfLatencyGauge.Set(CalculateAverageLatency(result));
                _iperfBandwidthGauge.Set(result.End.ReceivedSum.BitsPerSecond);
                log.LogInformation("latest result: bandwidth - {BW}; latency - {Lat}", result.End.ReceivedSum.BitsPerSecond, CalculateAverageLatency(result));
            }

            log.LogInformation("speed test done, waiting for {Delay} minutes", DelayMinutes);
            await Task.Delay(TimeSpan.FromMinutes(DelayMinutes), stoppingToken);
        }
    }

    private Iperf3Result? RunIperf3Test()
    {
        var args = $"-c {IperfServer} -p {IperfPort} -J"; // Use JSON output for easier parsing
        
        log.LogInformation("starting a speed test (with: {Args})", args);
        
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
            log.LogInformation(output);
            process.WaitForExit();

            if (process.ExitCode == 0)
                return ParseIperfOutput(output);

            log.LogError($"iPerf3 failed: ({process.ExitCode}) {process.StandardError.ReadToEnd()}");
        }
        catch (Exception ex)
        {
            log.LogError($"Error running iPerf3: {ex.Message}");
        }

        return null;
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