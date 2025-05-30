using Rimu.Repository.Pp.Adapter;

namespace Rimu.Repository.Pp.Domain;

/// <summary>
/// Provides a context for calculating performance points (PP) using a replay file.
/// </summary>
public class PpCalculatorContext: IPpCalculatorContext {
    private readonly PpCalculatorProvider _provider;

    public PpCalculatorContext() {
        _provider = PpCalculatorProvider.Self;
    }

    /// <summary>
    /// Asynchronously calculates the performance points (PP) from a replay file.
    /// </summary>
    /// <param name="replayFileBytes">The byte array representing the replay file.</param>
    /// <param name="filename">The name of the replay file.</param>
    /// <returns>
    /// A success result with an optional double value representing the calculated PP, or an error result.
    /// </returns>
    public async Task<SResult<Option<double>>> CalculateReplayAsync(byte[] replayFileBytes, string filename) {
        return await _provider.CalculateReplayAsync(replayFileBytes, filename);
    }
}