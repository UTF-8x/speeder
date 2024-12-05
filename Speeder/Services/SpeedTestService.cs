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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {        
        while (!stoppingToken.IsCancellationRequested)
        {
            _intervalMinutes.WithLabels(["ookla"]).Set(DelayMinutes);

            _runCounter.WithLabels(["ookla"]).Inc();

            var ooklaTask = ooklaAdapter.MeasureAsync(stoppingToken);

            var ooklaResult = await ooklaTask;

            if(ooklaResult is null) 
            {
                log.LogWarning("ookla test failed");
                _failCounter.WithLabels(["ookla"]).Inc();
            }
            else
            {
                _upLatencyGauge.WithLabels(["ookla"]).Set(ooklaResult.UpLatency);
                _downLatencyGauge.WithLabels(["ookla"]).Set(ooklaResult.DownLatency);

                _upJitterGauge.WithLabels(["ookla"]).Set(ooklaResult.UpJitter);
                _downJitterGauge.WithLabels(["ookla"]).Set(ooklaResult.DownJitter);

                _upBandwidthGauge.WithLabels(["ookla"]).Set(ooklaResult.UploadSpeed);
                _downBandwidthGauge.WithLabels(["ookla"]).Set(ooklaResult.DownloadSpeed);
            }

            log.LogInformation("speed test done, waiting for {Delay} minutes", DelayMinutes);
            await Task.Delay(TimeSpan.FromMinutes(DelayMinutes), stoppingToken);
        }
    }
}