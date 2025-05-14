using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Globalization;
using CsvHelper;
using Microsoft.Extensions.DependencyInjection;
using Rimu.Repository.Environment.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter;
using Rimu.Repository.Postgres.Adapter.Dto;
using Rimu.Repository.Postgres.Adapter.Entities;
using Rimu.Repository.Postgres.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;
using Rimu.Terminal.ImportCSVFile;
using Rimu.Terminal.TypeConverter;

namespace Rimu.Terminal.Provider;

internal class InsertOldScoresToNewDbProvider {
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    private readonly string _pathBblScoreCsv;
    private readonly IQueryUserInfo _queryUserInfo;
    private readonly IQueryPlayStatsHistory _queryPlayStatsHistory;
    private readonly IQueryReplayFile _queryReplayFile;
    
    internal InsertOldScoresToNewDbProvider(
        string pathBblScoreCsv,
        IQueryUserInfo queryUserInfo,
        IQueryPlayStatsHistory queryPlayStatsHistory,
        IQueryReplayFile queryReplayFile
        ) {
        
        _pathBblScoreCsv = pathBblScoreCsv;
        _queryUserInfo = queryUserInfo;
        _queryPlayStatsHistory = queryPlayStatsHistory;
        _queryReplayFile = queryReplayFile;
    }
    
    /// <exception></exception>>
    public async Task RunAsync() {
        FrozenSet<long> userIds = (await _queryUserInfo.GetAllIdsAsync()).Ok().ToFrozenSet();
        View_Play_PlayStats[] playScoreArr = await GetAllOldScoresAsPlayScore(userIds);
        
        Logger.Debug("Fix PlayScore playScoreArr Count: {}", playScoreArr.Length);
        playScoreArr = await FilterPlayScoresWithUserIdsAsync(playScoreArr);
        
        Logger.Debug("Fix ReplayFileIds playScoreArr Count: {}", playScoreArr.Length);
        await SetReplayFileIdToNullIfNotExist(playScoreArr);
        
        Logger.Debug("Insert Play PlayStats Count: {}", playScoreArr.Length);
        await InsertPlay_PlayStatsAsync(playScoreArr);
        
        Logger.Debug("Start Insert PlayHistory");
        await InsertBulkPlayHistoryAsync(playScoreArr);
        Logger.Debug("End   Insert PlayHistory");
    }

    private async Task<View_Play_PlayStats[]> FilterPlayScoresWithUserIdsAsync(View_Play_PlayStats[] playScoreArr) {
        HashSet<long> userIdsHashSet = new HashSet<long>(playScoreArr.Length);
        
        Logger.Debug("Start FilterPlayScoresWithUserIdsAsync Count: {}", userIdsHashSet.Count);
        
        var userInfosResult = (await _queryUserInfo.GetAllIdsAsync());
        if (userInfosResult == EResult.Err) {
            throw new Exception("userInfosResult has a error");
        }
        foreach (var ids in userInfosResult.Ok()) {
            userIdsHashSet.Add(ids);
        }
        
        var returnAfterFilter = playScoreArr.Where(p => userIdsHashSet.Contains(p.Id)).ToArray();
        
        Logger.Debug("End FilterPlayScoresWithUserIdsAsync Count: {}", returnAfterFilter.Length);

        return returnAfterFilter;
    }

    private async Task SetReplayFileIdToNullIfNotExist(View_Play_PlayStats[] playScoreArr) {
        FrozenSet<long> replayFileIdSet;
        {
            var resultOk = await this._queryReplayFile.GetAllIdsAsync();
            if (resultOk == EResult.Err) {
                Logger.Error("this._queryReplayFile.GetAllIdsAsync() Error");
                System.Environment.Exit(1);
            }

            replayFileIdSet = new HashSet<long>(resultOk.Ok()).ToFrozenSet();
        }
        
        foreach (var viewPlayPlayStats in playScoreArr) {
            if (!viewPlayPlayStats.ReplayFileId.HasValue) {
                continue;
            }

            if (replayFileIdSet.Contains(viewPlayPlayStats.ReplayFileId.Value)) {
                continue;
            }
            viewPlayPlayStats.ReplayFileId = null;
        }
    }
    
    private async Task<View_Play_PlayStats[]> GetAllOldScoresAsPlayScore(FrozenSet<long> userIds) {
        IList<bbl_score> oldScores = Array.Empty<bbl_score>();

        await using (var stream = new FileStream(this._pathBblScoreCsv, FileMode.Open, FileAccess.Read, FileShare.None, 4096 * 16, FileOptions.SequentialScan))
        using (var reader = new StreamReader(stream))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture)) {
            csv.Context.TypeConverterCache.AddConverter<string>(new NullStringType());
            csv.Context.TypeConverterCache.AddConverter<long?>(new NullLongType());
            csv.Context.TypeConverterCache.AddConverter<double?>(new NullDecimalType());
            csv.Context.TypeConverterOptionsCache.GetOptions<string>().NullValues.Add("NULL");
            csv.Context.TypeConverterOptionsCache.GetOptions<long?>().NullValues.Add("NULL");
            csv.Context.TypeConverterOptionsCache.GetOptions<double?>().NullValues.Add("NULL");
            
            oldScores = csv
                         .GetRecords<bbl_score>()
                         .Where(x => userIds.Contains(x.uid))
                         .Where(x => x.score > 0)
                         .Where(x => x.hash is not null && x.hash.Length > 20)
                         .Where(x => x.mode is null 
                                     || (!x.mode.Contains("|AR")))
                         .ToList()
                ;
            
        }
        
        var newScore = new ConcurrentBag<View_Play_PlayStats>();
        GC.Collect();
        
        void ConvertToNewPlayScore(bbl_score score) {
            score.mode ??= "|";
            score.mode = score.mode == "-" ? "|" : score.mode;

            if (score.mode.IndexOf("AR", StringComparison.Ordinal) != -1
                || score.score <= 0
                || score.accuracy <= 0) {
                Logger.Warn($"Not Valid Score or accuracy Row {score.id} (score {score.score}, accuracy {score.accuracy})");
                return;
            }

            if (IsNullOrEmptyOrNULLOrNullOrWhitespace(score.hash) || IsNullOrEmptyOrNULLOrNullOrWhitespace(score.filename)) {
                Logger.Warn($"Not Valid hash or filename Row {score.id}");
            }

            if (score.mode is null)
                score.mode = "|";
            if (score.mode.Length == 0)
                score.mode = "|";
            if (score.mode.IndexOf("-", StringComparison.Ordinal) != -1)
                score.mode = "|";

            try {
                PlayMode.ModeAsSingleStringToModeArray(score.mode);
            }
            catch (Exception) {
                Logger.Warn("Invalid Mode: {}, id {}", score.mode, score.id);
                return;
            }
            
            var playScore = new View_Play_PlayStats() {
                Id = score.id,
                UserId = score.uid,
                Filename = score.filename ?? throw new NullReferenceException($"score.filename score.id: {score.id}"),
                FileHash = score.hash ?? throw new NullReferenceException($"score.hash score.id: {score.id}"),
                Mode = PlayMode.ModeAsSingleStringToModeArray(score.mode),
                Score = score.score,
                Combo = score.combo,
                Mark = score.mark ?? throw new NullReferenceException($"score.mark score.id: {score.id}"),
                Geki = score.geki,
                Perfect = score.perfect,
                Katu = score.katu,
                Good = score.good,
                Bad = score.bad,
                Miss = score.miss,
                Date = DateTime.SpecifyKind(score.date, DateTimeKind.Utc),
                Accuracy = score.accuracy,
                Pp = score.pp.HasValue ? score.pp.Value : -1,
                ReplayFileId = score.id,
            };

            // Test Convert
            PlayStatsDto.Create(playScore);
            newScore.Add(playScore);
        }

        Parallel.ForEach(
            oldScores,
            new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
            ConvertToNewPlayScore
        );

        return newScore.ToArray();
    }

    private async Task InsertPlay_PlayStatsAsync(View_Play_PlayStats[] playScoreArr) {
        Array.Sort(playScoreArr, (x, y) => x.Pp.CompareTo(y.Pp));
        var chunks = SplitIntoChunks(playScoreArr, 10_000);
        var count = chunks.Count;
        var icChunks = new List<(int i, int count, View_Play_PlayStats[] arr)>(chunks.Count);

        for (var i = 0; i < chunks.Count; i++) icChunks.Add((i, count, chunks[i]));

        await Parallel.ForEachAsync(
            icChunks,
            new ParallelOptions { MaxDegreeOfParallelism = 16 },
            InsertPlayScoreChunk
        );
    }

    private async ValueTask InsertPlayScoreChunk(
        (int i, int count, View_Play_PlayStats[] arr) val,
        CancellationToken cancellationToken) {

        await using var insertPlayScoreChunkScope = new InsertPlayScoreChunkScope();
        await insertPlayScoreChunkScope.RunAsync(val, cancellationToken);
    }

    private bool IsNullOrEmptyOrNULLOrNullOrWhitespace(string? value) {
        return value is null || value.Trim() is "" or " " or "NULL" or "null";
    }

    private List<T[]> SplitIntoChunks<T>(T[] array, int chunkSize) {
        if (chunkSize <= 0)
            throw new Exception($"chunkSize <= 0: {chunkSize}");
        var listOfChunks = new List<T[]>((1 + array.Length) / chunkSize);

        for (var i = 0; i < array.Length; i += chunkSize) {
            var chunk = new T[Math.Min(chunkSize, array.Length - i)];
            Array.Copy(array, i, chunk, 0, chunk.Length);
            listOfChunks.Add(chunk);
        }

        return listOfChunks;
    }

    /// <exception></exception>
    private async Task InsertBulkPlayHistoryAsync(View_Play_PlayStats[] view_Play_PlayStatsArr) {
        List<PlayStatsHistory> playStatsHistories = new(view_Play_PlayStatsArr.Length);
        foreach (var allPlayStat in view_Play_PlayStatsArr) {
            playStatsHistories.Add(PlayStatsHistory.From(allPlayStat));
        }

        int count = 0;
        foreach (var statsHistoriese in playStatsHistories.Chunk(10_000)) {
            Logger.Debug("Insert PlayStatsHistory Chunk: {}", count);
            count++;
            var err = await _queryPlayStatsHistory.InsertBulkWithNewIdAsync(statsHistoriese);
            if (err == EResult.Err) {
                Logger.Error("Can Not Insert Play History");
                throw new Exception("Can Not Insert Play History");
            }
        }
    }

    public class InsertPlayScoreChunkScope: IAsyncDisposable {
        private readonly IQueryPlay _queryPlay;
        private readonly IQueryPlayStats _queryPlayStats;
        private readonly IServiceScope _serviceScope;
        private readonly IDbContext _dbContext;
        private bool _isDisposed = false;
        public InsertPlayScoreChunkScope() {
            _serviceScope  = Repository.Dependency.Adapter.Injection.GlobalServiceProvider.CreateScope();
            _queryPlay = _serviceScope.ServiceProvider.GetQueryPlay();
            _queryPlayStats = _serviceScope.ServiceProvider.GetQueryPlayStats();
            _dbContext = _serviceScope.ServiceProvider.GetDbContext();
        }

        
        public async Task RunAsync((int i, int count, View_Play_PlayStats[] arr) val, CancellationToken cancellationToken) {

            if (cancellationToken.IsCancellationRequested) {
                await Task.FromCanceled(cancellationToken);
                return;
            }
            
            var plays = View_Play_PlayStats.ToPlays(val.arr); 
            var playStatss = View_Play_PlayStats.ToPlayStatsArray(val.arr);
        
            var id = Guid.NewGuid();
            Logger.Debug("Inserting Chunk from {} of {} id: {}", val.i, val.count, id);


            (await _queryPlay.InsertBulkAsync(plays)).ThrowIfErr("Can Not Insert Play");
            (await _queryPlayStats.InsertBulkAsync(playStatss)).ThrowIfErr("Can Not Insert PlayStats");
        }


        ~InsertPlayScoreChunkScope() {
            DisposeAsync().GetAwaiter().GetResult();
        }
        
        public async ValueTask DisposeAsync() {
            if (_isDisposed) {
                return;
            }
            _isDisposed = true;
            await _dbContext.DisposeAsync();
            GC.SuppressFinalize(this);
        }
    }
}