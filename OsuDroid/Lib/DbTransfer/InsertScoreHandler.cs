using OsuDroid.Database.OldEntities;
using System.Collections.Concurrent;
using OsuDroidLib.Database.Entities;
using Dapper;

namespace OsuDroid.Lib.DbTransfer; 

internal static class InsertScoreHandler {
    public static async Task Run() {
        var playScoreArr = await GetAllOldScoresAsPlayScore();
        await InsertPlayScore(playScoreArr);
    }

    private static async Task<PlayScore[]> GetAllOldScoresAsPlayScore() {
        HashSet<long> userIds = new HashSet<long>(100_000);
        List<bbl_score> oldScore = new List<bbl_score>(30_000_000);
        ConcurrentBag<PlayScore> newScore = new ConcurrentBag<PlayScore>();
        GC.Collect();
        await using (var db = await DbBuilder.BuildNpgsqlConnection()) {
            foreach (var bblUser in await db.QueryAsync<UserInfo>("SELECT UserId FROM public.UserInfo")) {
                userIds.Add(bblUser.UserId);
            }
            
            WriteLine($"User Count: {userIds.Count}");
            
            foreach (var bblScore in await db.QueryAsync<bbl_score>("SELECT * FROM old_osu.bbl_score")) {
                oldScore.Add(bblScore);
            }

            WriteLine($"oldScore Count: {oldScore.Count}");
        }
        
        GC.Collect();
        void ConvertToNewPlayScore(bbl_score score) {
            if (userIds.Contains(score.uid) == false)
                return;
            
            score.mode ??= "|";
            score.mark = IsNullOrEmptyOrNULLOrNullOrWhitespace(score.mark) ? "-" : score.mark;

            if (score.mode.IndexOf("AR", StringComparison.Ordinal) != -1
                || score.score <= 0
                || score.accuracy <= 0
                || IsNullOrEmptyOrNULLOrNullOrWhitespace(score.hash)
                || IsNullOrEmptyOrNULLOrNullOrWhitespace(score.filename)) {
                WriteLine($"Not Valid Score Row {score.id}");
            }

            if (score.mode is null)
                score.mode = "|";
            if (score.mode.Length == 0)
                score.mode = "|";
            if (score.mode.IndexOf("-", StringComparison.Ordinal) != -1)
                score.mode = "|";

            var playScore = new PlayScore {
                PlayScoreId = score.id,
                UserId = score.id,
                Filename = score.filename ??
                           throw new NullReferenceException($"score.filename score.id: {score.id}"),
                Hash = score.hash ?? throw new NullReferenceException($"score.hash score.id: {score.id}"),
                Mode = Utils.Mode.ModeAsSingleStringToModeArray(score.mode),
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
                Accuracy = score.accuracy
            };

            newScore.Add(playScore);
        }
        
        Parallel.ForEach(
            oldScore,
            new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
            ConvertToNewPlayScore
        );

        return newScore.ToArray();
    }
    
    private static async Task InsertPlayScore(PlayScore[] playScoreArr) {
        var chunks = SplitIntoChunks(playScoreArr, 100_000);
     
        await Parallel.ForEachAsync(
            chunks,
            new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, 
            InsertPlayScoreChunk
        );
    }

    private static async ValueTask InsertPlayScoreChunk(PlayScore[] playScoreArr, CancellationToken cancellationToken) {
        await using var db = await DbBuilder.BuildNpgsqlConnection();
        
        var query = @"
INSERT INTO PlayScore (PlayScoreId, UserId, Filename, Hash, Mode, Score, Combo, Mark, Geki, Perfect, Katu, Good, Bad, Miss, Date, Accuracy) 
VALUES                (@PlayScoreId, @UserId, @Filename, @Hash, @Mode, @Score, @Combo, @Mark, @Geki, @Perfect, @Katu, @Good, @Bad, @Miss, @Date, @Accuracy) 
";
        await db.ExecuteAsync(query, playScoreArr);
    }
    
    private static bool IsNullOrEmptyOrNULLOrNullOrWhitespace(string? value)
        => value is null || value.Trim() is "" or " " or "NULL" or "null";
    
    private static List<T[]> SplitIntoChunks<T>(T[] array, int chunkSize) {
        if (chunkSize <= 0)
            throw new Exception($"chunkSize <= 0: {chunkSize}");
        List<T[]> listOfChunks = new List<T[]>((1 + array.Length) / chunkSize);
        
        for (int i = 0; i < array.Length; i += chunkSize)
        {
            T[] chunk = new T[Math.Min(chunkSize, array.Length - i)];
            Array.Copy(array, i, chunk, 0, chunk.Length);
            listOfChunks.Add(chunk);
        }
        return listOfChunks;
    }
}