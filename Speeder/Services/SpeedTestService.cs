using Prometheus;
using Speeder.Infra;
using Speeder.Infra.Impl;

namespace Speeder.Services;

public class SpeedTestService(
    ILogger<SpeedTestService> log, 
    ServerPool pool, 
    IConfiguration config,
    Iperf3Adapter iperfAdapter,
    OoklaAdapter ooklaAdapter) : BackgroundService
{

    private readonly Gauge _LatencyGauge = Metrics.CreateGauge("speeder_download_latency", "latency from Ookla speedtest", ["source_name"]);
    private readonly Gauge _downBandwidthGauge = Metrics.CreateGauge("speeder_download_speed", "download bandwidth from Ookla speedtest", ["source_name"]);
    private readonly Gauge _upBandwidthGauge = Metrics.CreateGauge("speeder_upload_speed", "upload bandwidth from Ookla speedtest", ["source_name"]);
    private readonly Counter _runCounter = Metrics.CreateCounter("speeder_runs_counter", "total count of all Ookla speedtest runs", ["source_name"]);
    private readonly Counter _failCounter = Metrics.CreateCounter("speeder_fails_counter", "total count of failed Ookla speedtest runs", ["source_name"]);

    private const int DelayMinutes = 1;

    private bool _ready = false;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if(!_ready)
            {
                log.LogInformation("speed test service ready");
                _ready = true;
                continue;
            }

            _runCounter.WithLabels(["ookla"]).Inc();
            _runCounter.WithLabels(["iperf"]).Inc();

            var ooklaTask = ooklaAdapter.MeasureAsync(stoppingToken);
            var iperfTask = iperfAdapter.MeasureAsync(stoppingToken);

            var ooklaResult = await ooklaTask;
            var iperfResult = await iperfTask;

            if(ooklaResult is null) 
            {
                log.LogWarning("ookla test failed");
                _failCounter.WithLabels(["ookla"]).Inc();
            }
            else
            {
                _downBandwidthGauge.WithLabels(["ookla"]).Set(ooklaResult.DownloadSpeed);
                _upBandwidthGauge.WithLabels(["ookla"]).Set(ooklaResult.UploadSpeed);
                _LatencyGauge.WithLabels(["ookla"]).Set(ooklaResult.AverageLatency);
            }

            if(iperfResult is null)
            {
                log.LogWarning("iperf test failed");
                _failCounter.WithLabels(["iperf"]).Inc();
            }
            else
            {
                _downBandwidthGauge.WithLabels(["iperf"]).Set(iperfResult.DownloadSpeed);
                _upBandwidthGauge.WithLabels(["iperf"]).Set(iperfResult.UploadSpeed);
                _LatencyGauge.WithLabels(["iperf"]).Set(iperfResult.AverageLatency);
            }

            log.LogInformation("speed test done, waiting for {Delay} minutes", DelayMinutes);
            await Task.Delay(TimeSpan.FromMinutes(DelayMinutes), stoppingToken);
        }
    }
}