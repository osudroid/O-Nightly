namespace Rimu.Repository.Pp.Adapter;

public interface IPpCalculatorContext {
    public Task<SResult<Option<double>>> CalculateReplayAsync(byte[] replayFileBytes, string filename);
}