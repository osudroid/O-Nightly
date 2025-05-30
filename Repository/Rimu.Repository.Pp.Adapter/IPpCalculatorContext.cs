namespace Rimu.Repository.Pp.Adapter;

public interface IPpCalculatorContext {
    /// <summary>
    /// Calculates performance points (PP).
    /// </summary>
    /// <param name="replayFileBytes">The byte array representing the replay file.</param>
    /// <param name="filename">The name of the replay file.</param>
    /// <returns>
    /// A success result with an optional double value representing the calculated PP, or an error result.
    /// </returns>
    public Task<SResult<Option<double>>> CalculateReplayAsync(byte[] replayFileBytes, string filename);
}