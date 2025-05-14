using Rimu.Repository.Pp.Adapter;

namespace Rimu.Repository.Pp.Domain;

public class PpCalculatorContext: IPpCalculatorContext {
    private readonly PpCalculatorProvider _provider;

    public PpCalculatorContext() {
        _provider = PpCalculatorProvider.Self;
    }

    public async Task<SResult<Option<double>>> CalculateReplayAsync(byte[] replayFileBytes, string filename) {
        return await _provider.CalculateReplayAsync(replayFileBytes, filename);
    }
}