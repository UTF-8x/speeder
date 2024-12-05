namespace Speeder.Infra;

public class SpeedTestResult
{
    public double DownloadSpeed { get; set; }

    public double UploadSpeed { get; set; }

    public double DownLatency { get; set; }

    public double DownJitter { get; set; }

    public double UpLatency { get; set; }

    public double UpJitter { get; set; }
}