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

    private readonly Gauge _upLatencyGauge = Metrics.CreateGauge("speeder_upload_latency", "upload latency", ["source_name"]);
    private readonly Gauge _downLatencyGauge = Metrics.CreateGauge("speeder_download_latency", "download latency", ["source_name"]);

    private readonly Gauge _upJitterGauge = Metrics.CreateGauge("speeder_upload_jitter", "upload jitter", ["source_name"]);
    private readonly Gauge _downJitterGauge = Metrics.CreateGauge("speeder_download_jitter", "download jitter", ["source_name"]);

    private readonly Gauge _downBandwidthGauge = Metrics.CreateGauge("speeder_download_speed", "download speed", ["source_name"]);
    private readonly Gauge _upBandwidthGauge = Metrics.CreateGauge("speeder_upload_speed", "upload speed", ["source_name"]);
    
    private readonly Counter _runCounter = Metrics.CreateCounter("speeder_runs_counter", "total run count", ["source_name"]);
    private readonly Counter _failCounter = Metrics.CreateCounter("speeder_fails_counter", "total failed run count", ["source_name"]);

    private readonly Gauge _intervalMinutes = Metrics.CreateGauge("speeder_interval_minutes", "measurement interval in minutes", ["source_name"]);

    private readonly int DelayMinutes = config.GetRequiredSection("Speeder").GetValue<int>("MeasurementIntervalSeconds");

    private readonly bool _useOokla = config.GetRequiredSection("Speeder").GetValue<bool>("UseOokla");
    private readonly bool _useIperf = config.GetRequiredSection("Speeder").GetValue<bool>("UseIperf");

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {        
        while (!stoppingToken.IsCancellationRequested)
        {
            _intervalMinutes.WithLabels(["ookla"]).Set(DelayMinutes);
            _intervalMinutes.WithLabels(["iperf"]).Set(DelayMinutes);

            Task? ooklaTask = null;
            Task? iperfTask = null;

            if (_useOokla) ooklaTask = DoTest("ookla", ooklaAdapter, stoppingToken);
            if (_useIperf) iperfTask = DoTest("iperf", iperfAdapter, stoppingToken);

            if (ooklaTask is not null) await ooklaTask;
            if (iperfTask is not null) await iperfTask;

            log.LogInformation("speed test done, waiting for {Delay} minutes", DelayMinutes);
            await Task.Delay(TimeSpan.FromMinutes(DelayMinutes), stoppingToken);
        }
    }

    private async Task DoTest(string label, ISpeedTestAdapter adapter, CancellationToken stoppingToken)
    {
        _runCounter.WithLabels([label]).Inc();
        var result = await adapter.MeasureAsync(stoppingToken);

        if (result is null)
        {
            log.LogWarning($"{label} test failed");
            _failCounter.WithLabels([label]).Inc();
        }
        else
        {
            _upLatencyGauge.WithLabels([label]).Set(result.UpLatency);
            _downLatencyGauge.WithLabels([label]).Set(result.DownLatency);

            _upJitterGauge.WithLabels([label]).Set(result.UpJitter);
            _downJitterGauge.WithLabels([label]).Set(result.DownJitter);

            _upBandwidthGauge.WithLabels([label]).Set(result.UploadSpeed);
            _downBandwidthGauge.WithLabels([label]).Set(result.DownloadSpeed);
        }
    }
}