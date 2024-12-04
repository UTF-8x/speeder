namespace Speeder.Infra;

public class SpeedTestResult
{
    public double DownloadSpeed { get; set; }

    public double UploadSpeed { get; set; }

    public int AverageLatency { get; set; }

    public int AverageJitter { get; set; }

    public int[]? Latencies { get; set; }

    public int[]? Jitters { get; set; }
}