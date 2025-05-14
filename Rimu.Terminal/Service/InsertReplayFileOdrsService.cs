using Rimu.Terminal.Provider;

namespace Rimu.Terminal.Service;

public sealed class InsertReplayFileOdrsService {
    private readonly string _uploadOdrsDirectory;
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    private readonly InsertReplayFileOdrsProvider _insertReplayFileOdrsProvider;
    
    public InsertReplayFileOdrsService(string uploadOdrsDirectory) {
        _uploadOdrsDirectory = uploadOdrsDirectory;
        _insertReplayFileOdrsProvider = new InsertReplayFileOdrsProvider(uploadOdrsDirectory);
    }

    public async ValueTask RunAsync(CancellationToken cancellationToken) {
        await _insertReplayFileOdrsProvider.RunAsync(cancellationToken);
    }
}