using Microsoft.Extensions.DependencyInjection;
using Rimu.Repository.Postgres.Adapter;
using Rimu.Repository.Postgres.Adapter.Entities;
using Rimu.Repository.Postgres.Adapter.Query;

namespace Rimu.Terminal.Provider;

public sealed class InsertReplayFileOdrsProvider {
    private readonly string _uploadOdrsDirectory;
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    private long _maxChunks = 0;
    private long _chunksFinished = 0;
    public InsertReplayFileOdrsProvider(string uploadOdrsDirectory) {
        _uploadOdrsDirectory = uploadOdrsDirectory;
    }

    public async Task RunAsync(CancellationToken cancellationToken) {
        var files = Directory.GetFiles(_uploadOdrsDirectory).Chunk(128).ToArray();
        _maxChunks = files.Length;
        _chunksFinished = 0;
        
        await Parallel.ForEachAsync(
            files, new ParallelOptions() {
                CancellationToken = cancellationToken, 
                MaxDegreeOfParallelism = 16, 
                TaskScheduler = TaskScheduler.Default
            }, 
            LoadChunkToDbAsync
        );
    }
    
    private async ValueTask LoadChunkToDbAsync(string[] fileWithPaths, CancellationToken cancellationToken) {
        await using var serviceScope = Rimu.Repository.Dependency.Adapter.Injection.GlobalServiceProvider.CreateAsyncScope();
        var serviceProvider = serviceScope.ServiceProvider;

        await using var dbContext = serviceProvider.GetDbContext();
        var queryReplayFile = serviceProvider.GetQueryReplayFile();
        
        if (cancellationToken.IsCancellationRequested) {
            await Task.FromCanceled(cancellationToken);
        }

        var replayFiles = new List<ReplayFile>(fileWithPaths.Length);
        
        foreach (var fileWithPath in fileWithPaths) {
            if (!fileWithPath.EndsWith(".odr")) {
                continue;
            }
            ReadOnlySpan<char> fileName = Path.GetFileName(fileWithPath.AsSpan());
            ReadOnlySpan<char> replayFile = fileName[..fileName.IndexOf('.')];

            if (!long.TryParse(replayFile, out var id)) {
                Logger.Error($"Could not parse replay file id '{replayFile}'");
                System.Environment.Exit(1);
            }
            
            replayFiles.Add(new () {
                Id = id,
                Odr = await ReadAllBytesAsync(fileWithPath, cancellationToken)
            });
        }

        if (await queryReplayFile.BulkInsertWithIdsAsync(replayFiles.ToArray()) == EResult.Err) {
            Logger.Error($"Failed to insert ReplayFiles from {_uploadOdrsDirectory}");
            System.Environment.Exit(1);
            return;
        }

        Interlocked.Add(ref this._chunksFinished, 1);
        Logger.Info($"Chunks {this._maxChunks} | {this._chunksFinished} replay files.");
        
        static async ValueTask<byte[]> ReadAllBytesAsync(string path, CancellationToken cancellationToken) {
            if (cancellationToken.IsCancellationRequested) {
                await Task.FromCanceled(cancellationToken);
            }


            await using (var memoryStream = new MemoryStream(4096*2)) {
                var streamOptions = new FileStreamOptions() {
                    Access = FileAccess.Read,
                    Mode = FileMode.Open,
                    Options = FileOptions.SequentialScan,
                    Share = FileShare.None,
                    BufferSize = 4096 * 2,
                };
                await using var file = System.IO.File.Open(path, streamOptions);
                await file.CopyToAsync(memoryStream, cancellationToken);
                
                return memoryStream.ToArray();
            }
        }
    }
}