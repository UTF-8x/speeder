namespace Speeder.Infra;

public interface ISpeedTestAdapter
{
    /// <summary>
    /// Perform a measurement asynchronously
    /// </summary>
    /// <param name="cancel"></param>
    /// <returns></returns>
    Task<SpeedTestResult?> MeasureAsync(CancellationToken cancel);
}