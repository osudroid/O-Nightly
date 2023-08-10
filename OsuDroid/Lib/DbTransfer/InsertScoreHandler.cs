using System.Collections.Concurrent;
using Dapper;
using OsuDroid.Database.OldEntities;
using OsuDroid.Utils;
using OsuDroidLib.Dto;

namespace OsuDroid.Lib.DbTransfer;

internal static class InsertScoreHandler {
    public static async Task Run() {
        var playScoreArr = await GetAllOldScoresAsPlayScore();
        WriteLine("Fix PlayScore playScoreArr Count: " + playScoreArr.Length);
        await InsertPlayScore(playScoreArr);
    }

    private static async Task<Entities.PlayScore[]> GetAllOldScoresAsPlayScore() {
        var oldScore = new List<bbl_score>(30_000_000);
        var newScore = new ConcurrentBag<Entities.PlayScore>();
        GC.Collect();
        await using (var db = await DbBuilder.BuildNpgsqlConnection()) {
            oldScore = (await db.QueryAsync<bbl_score>(@"
SELECT * 
FROM old_osu.bbl_score
JOIN userinfo ON userinfo.userid = old_osu.bbl_score.uid
WHERE score > 0
AND hash != ''
AND hash is not null
AND false = (mode Like '%|AR%')
"
            )).ToList();

            WriteLine($"oldScore Count: {oldScore.Count}");
        }

        GC.Collect();

        void ConvertToNewPlayScore(bbl_score score) {
            score.mode ??= "|";
            score.mode = score.mode == "-" ? "|" : score.mode;

            if (score.mode.IndexOf("AR", StringComparison.Ordinal) != -1
                || score.score <= 0
                || score.accuracy <= 0
                || IsNullOrEmptyOrNULLOrNullOrWhitespace(score.hash)
                || IsNullOrEmptyOrNULLOrNullOrWhitespace(score.filename))
                WriteLine($"Not Valid Score Row {score.id}");

            if (score.mode is null)
                score.mode = "|";
            if (score.mode.Length == 0)
                score.mode = "|";
            if (score.mode.IndexOf("-", StringComparison.Ordinal) != -1)
                score.mode = "|";

            var playScore = new Entities.PlayScore {
                PlayScoreId = score.id,
                UserId = score.uid,
                Filename = score.filename ?? throw new NullReferenceException($"score.filename score.id: {score.id}"),
                Hash = score.hash ?? throw new NullReferenceException($"score.hash score.id: {score.id}"),
                Mode = Mode.ModeAsSingleStringToModeArray(score.mode),
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

            // Test Convert
            if (PlayScoreDto.ToPlayScoreDto(playScore).IsNotSet())
                throw new Exception($"Can Not Convert To PlayScoreDto PlayScoreId {playScore.PlayScoreId}");

            newScore.Add(playScore);
        }

        Parallel.ForEach(
            oldScore,
            new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
            ConvertToNewPlayScore
        );

        return newScore.ToArray();
    }

    private static async Task InsertPlayScore(Entities.PlayScore[] playScoreArr) {
        var chunks = SplitIntoChunks(playScoreArr, 10_000);
        var count = chunks.Count;
        var icChunks = new List<(int i, int count, Entities.PlayScore[] arr)>(chunks.Count);

        for (var i = 0; i < chunks.Count; i++) icChunks.Add((i, count, chunks[i]));

        await Parallel.ForEachAsync(
            icChunks,
            new ParallelOptions { MaxDegreeOfParallelism = 16 },
            InsertPlayScoreChunk
        );
    }

    private static async ValueTask InsertPlayScoreChunk(
        (int i, int count, Entities.PlayScore[] arr) val,
        CancellationToken cancellationToken) {
        await using var db = await DbBuilder.BuildNpgsqlConnection();

        var id = Guid.NewGuid();
        try {
            WriteLine($"Inserting PlayScore Chunk from {val.i} of {val.count} id: {id}");
            try {
                var query = @"
INSERT INTO PlayScore (PlayScoreId, UserId, Filename, Hash, Mode, Score, Combo, Mark, Geki, Perfect, Katu, Good, Bad, Miss, Date, Accuracy) 
VALUES                (@PlayScoreId, @UserId, @Filename, @Hash, @Mode, @Score, @Combo, @Mark, @Geki, @Perfect, @Katu, @Good, @Bad, @Miss, @Date, @Accuracy) 
";
                await db.ExecuteAsync(query, val.arr);
            }
            catch (Exception e) {
                WriteLine(e);
                throw;
            }
        }
        catch (Exception e) {
            WriteLine($"Inserting PlayScore Chunk Error from {val.i} of {val.count} id: {id} \n Error: {e}");
            throw;
        }
    }

    private static bool IsNullOrEmptyOrNULLOrNullOrWhitespace(string? value) {
        return value is null || value.Trim() is "" or " " or "NULL" or "null";
    }

    private static List<T[]> SplitIntoChunks<T>(T[] array, int chunkSize) {
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
}