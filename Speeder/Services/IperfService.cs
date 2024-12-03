using System.Diagnostics;
using Prometheus;
using Speeder.Infra;

namespace Speeder.Services;

public class IperfService(ILogger<IperfService> log, ServerPool pool, IConfiguration config) : BackgroundService
{
    private readonly Gauge _iperfLatencyGauge = Metrics.CreateGauge("iperf_latency_ms", "latency in ms");
    private readonly Gauge _iperfBandwidthGauge = Metrics.CreateGauge("iperf_bandwidth_bps", "bandwidth in bps");
    private readonly Counter _iperfRunsCounter = Metrics.CreateCounter("iperf_runs", "total iperf runs");
    private readonly Counter _iperfFailureCounter = Metrics.CreateCounter("iperf_failures", "number of failures");

    private readonly string IperfExePath = config.GetRequiredSection("Iperf").GetValue<string>("ExePath")
        ?? throw new ApplicationException("missing configuration 'Iperf:ExePath'");

    private const int DelayMinutes = 1;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _iperfRunsCounter.Inc();
            var nextServer = pool.GetNextAvailable();
            if(nextServer is null)
            {
                log.LogError("no servers are available at the moment, retrying in 30 seconds");
                _iperfFailureCounter.Inc();
                await Task.Delay(TimeSpan.FromSeconds(30));
                continue;
            }

            log.LogInformation("trying server {Server}", nextServer.Hostname);
            var result = RunIperf3Test(nextServer);

            if(result is null)
            {
                _iperfFailureCounter.Inc();
                log.LogWarning("server {Server} did not work, trying next server", nextServer.Hostname);
                nextServer.UnavailableSince = DateTime.UtcNow;
                continue;
            }

            _iperfLatencyGauge.Set(CalculateAverageLatency(result));
            _iperfBandwidthGauge.Set(result.End.ReceivedSum.BitsPerSecond);
            log.LogInformation("latest result: bandwidth - {BW}; latency - {Lat}", result.End.ReceivedSum.BitsPerSecond, CalculateAverageLatency(result));

            log.LogInformation("speed test done, waiting for {Delay} minutes", DelayMinutes);
            await Task.Delay(TimeSpan.FromMinutes(DelayMinutes), stoppingToken);
        }
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