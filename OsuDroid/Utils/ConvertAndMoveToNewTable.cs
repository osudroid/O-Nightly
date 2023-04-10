using System.Collections.Concurrent;
using System.Data;
using System.Runtime.CompilerServices;
using Npgsql;
using OsuDroid.Database.OldEntities;
using OsuDroidLib.Database.Entities;
using Dapper;
namespace OsuDroid.Utils;

public class ConvertAndMoveToNewTable {
    public async Task OpiRun() {
        BblUser[] allUserFromOldDb;
        
        using (var db = DbBuilder.BuildNpgsqlConnection()) {
            WriteLine($"Start Remove New Tables");
            RemoveAllRowsFromNewTables(db);
            WriteLine($"Fetch Old Users");
            WriteLine("Insert Users");
            allUserFromOldDb = await GetBblUser();
            InsertAllUsers(db, allUserFromOldDb);
            var com = db.CreateCommand();
            com.CommandText = "DELETE FROM public.bbl_user_stats;";
            com.ExecuteNonQuery();
        }

        await ParallelTransfer();
    }

    public async Task ParallelTransfer() {
        try {
            ConcurrentDictionary<long, ConcurrentBag<BblScore>> bblScores = new();
            
            using (var db = DbBuilder.BuildPostSqlAndOpen()) {
                foreach (var bblUser in db.Fetch<BblUser>("SELECT id FROM public.bbl_user").Ok()) {
                    bblScores[bblUser.Id] = new();
                }
                WriteLine($"User Count: {bblScores.Count}");
                List<bbl_score> oldScore = db.Fetch<bbl_score>("SELECT * FROM old_osu.bbl_score").Ok();
                WriteLine($"oldScore Count: {oldScore}");
                
                WriteLine("Bind Score To User Step 1");
                Parallel.ForEach(
                    oldScore, 
                    new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, 
                    MergeUserWithScore
                );
                
                void MergeUserWithScore(bbl_score score) {
                    score.Mode ??= "|";
                    
                    if (score.Mode.IndexOf("x", StringComparison.Ordinal) != -1
                        || score.Mode.IndexOf("AR", StringComparison.Ordinal) != -1
                        || IsNullOrEmptyOrNULLOrNullOrWhitespace(score.Mark)
                        || score.Score <= 0
                        || score.Accuracy <= 0
                        || IsNullOrEmptyOrNULLOrNullOrWhitespace(score.Hash)
                        || IsNullOrEmptyOrNULLOrNullOrWhitespace(score.Filename))
                        return;
                    
                    if (score.Mode is null)
                        score.Mode = "|";
                    if (score.Mode.Length == 0)
                        score.Mode = "|";
                    if (score.Mode.IndexOf("-", StringComparison.Ordinal) != -1)
                        score.Mode = "|";
                    if (score.Mode[^1] != '|')
                        score.Mode = "|";

                    var bblScore = new BblScore {
                        Id = score.Id,
                        Uid = score.Uid,
                        Filename = score.Filename,
                        Hash = score.Hash,
                        Mode = score.Mode,
                        Score = score.Score,
                        Combo = score.Combo,
                        Mark = score.Mark,
                        Geki = score.Geki,
                        Perfect = score.Perfect,
                        Katu = score.Katu,
                        Good = score.Good,
                        Bad = score.Bad,
                        Miss = score.Miss,
                        Date = DateTime.SpecifyKind(score.Date, DateTimeKind.Utc),
                        Accuracy = score.Accuracy
                    };

                    if (bblScores.TryGetValue(bblScore.Uid, out var concurrentBag) == false) {
                        return;
                    }
                    concurrentBag.Add(bblScore);
                }
                
                WriteLine("Bind Score To User Finish");
            }
            
            
            GC.Collect();

            {
                WriteLine("Pre Insert Scores");
                var bblScoresList = new List<BblScore>(10_000_000);
                foreach (KeyValuePair<long,ConcurrentBag<BblScore>> keyValuePair in bblScores) {
                    foreach (var bblScore in keyValuePair.Value) {
                        bblScoresList.Add(bblScore);
                    }
                }
                
                WriteLine($"Pre Insert Scores Count: {bblScoresList.Count}");
                WriteLine("Insert All Scores");
                
                using (var buildPostSqlAndOpen = DbBuilder.BuildPostSqlAndOpen()) {
                    long posi = 0;
                    var list = new List<BblScore>(10_000);
                    foreach (var i in bblScoresList) {
                        if (list.Count == 10_000) {
                            WriteLine($"{bblScoresList.Count}: {posi}");
                            buildPostSqlAndOpen.InsertBulk(list);
                            posi += 10_000;
                            list.Clear();
                        }
                        list.Add(i);
                    }
                    buildPostSqlAndOpen.InsertBulk(list);
                    WriteLine($"Insert End");
                }
            }

            ConcurrentBag<BblUserStats> stats = new ConcurrentBag<BblUserStats>();
            
            using (var db = DbBuilder.BuildPostSqlAndOpen()) {
                WriteLine("Calc Stats");
                Parallel.ForEach(
                    bblScores, 
                    new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                    pair => {
                        var userId = pair.Key;
                        var scores = pair.Value.ToArray();
                        var userStats = CreateUserStats(userId, scores);
                        WriteLine($"Calc Stats UserId: {userId}, Score: {userStats.OverallScore}, ScoresRows: {scores.Length}");
                        stats.Add(userStats);
                    }
                );

                db.InsertBulk(stats.ToArray());
            }
            
            WriteLine($"Finish");
        }
        catch (Exception e) {
            WriteLine(e);
            WriteLine("------------------------------------------");
            System.Environment.Exit(1);
            throw;
        }
    }


    public void RunRecalcStats() {
        ConcurrentDictionary<long, ConcurrentBag<BblScore>> bblScores = new();
        ConcurrentBag<BblUserStats> stats = new ConcurrentBag<BblUserStats>();
        
        using (var db = DbBuilder.BuildPostSqlAndOpen()) {
            foreach (var bblUser in db.Fetch<BblUser>("SELECT id FROM public.bbl_user").Ok()) {
                bblScores[bblUser.Id] = new();
            }
            
            WriteLine($"User Count: {bblScores.Count}");
            List<BblScore> scores = db.Fetch<BblScore>("SELECT * FROM bbl_score").Ok();
            WriteLine($"Score Count: {scores.Count}");
                
            WriteLine("Bind Score To User");
            Parallel.ForEach(
                scores,
                new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                score => {
                    bblScores[score.Uid].Add(score);
                }
            );

            WriteLine("Bind Score To User Finish");
        }
        
        WriteLine("Calc Stats");
        Parallel.ForEach(
            bblScores, 
            new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
            pair => {
                var userId = pair.Key;
                var scores = pair.Value.ToArray();
                BblUserStats userStats = CreateUserStats(userId, scores);
                WriteLine($"Calc Stats UserId: {userId}, Score: {userStats.OverallScore}, ScoresRows: {scores.Length}");
                stats.Add(userStats);
            }
        );

        BblUserStats[] statsArray = stats.ToArray();
        WriteLine($"Stats Insert Count: {statsArray.Length}");
        using (var db = DbBuilder.BuildNpgsqlConnection()) {
            WriteLine($"Delete Old Stats");
            db.Execute("DELETE FROM bbl_user_stats");
            WriteLine($"Insert");
            db.Execute(@"
INSERT 
INTO bbl_user_stats (
uid,
overall_playcount,
overall_score,
overall_accuracy,
overall_combo,
overall_xss,
overall_ss,
overall_xs,
overall_s,
overall_a,
overall_b,
overall_c,
overall_d,
overall_hits,
overall_300,
overall_100,
overall_50,
overall_geki,
overall_katu,
overall_miss
) VALUES (
   @Uid,
   @OverallPlaycount,
   @OverallScore,
   @OverallAccuracy,
   @OverallCombo,
   @OverallXss,
   @OverallSs,
   @OverallXs,
   @OverallS,
   @OverallA,
   @OverallB,
   @OverallC,
   @OverallD,
   @OverallHits,
   @Overall300,
   @Overall100,
   @Overall50,
   @OverallGeki,
   @OverallKatu,
   @OverallMiss
)      
", statsArray);
        }
        
        WriteLine($"Finish");
    }
    
    public async Task Run() {
        BblUser[] allUserFromOldDb;
        
        using (var db = DbBuilder.BuildNpgsqlConnection()) {
            WriteLine($"Start Remove New Tables");
            RemoveAllRowsFromNewTables(db);
            WriteLine($"Fetch Old Users");
            WriteLine("Insert Users");
            allUserFromOldDb = await GetBblUser();
            InsertAllUsers(db, allUserFromOldDb);
            var com = db.CreateCommand();
            com.CommandText = "DELETE FROM public.bbl_user_stats;";
            com.ExecuteNonQuery();
        }


        await Parallel.ForEachAsync(
            allUserFromOldDb, 
            new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount + 2 }, 
            SingleTransfer
        );
        
        await RunFixUserStats();
    }

    private async Task RunFixUserStats() {
        BblUser[] allUserFromOldDb;
        using (var db = DbBuilder.BuildPostSqlAndOpen()) {
            allUserFromOldDb = db.Fetch<BblUser>("SELECT id FROM public.bbl_user").OkOr(new(0)).ToArray();
            db.Execute("DELETE FROM public.bbl_user_stats");
        }
        
        await Parallel.ForEachAsync(
            allUserFromOldDb, 
            new ParallelOptions { MaxDegreeOfParallelism = 18 }, 
            SingleFixStat
        );
        
        static async ValueTask SingleFixStat(BblUser user, CancellationToken token) {
            BblScore[] scores;
            var dbs = DbBuilder.BuildNpgsqlSavePoco();
            using var db = dbs.SavePoco;
            using var npsql = dbs.Npgsql;
            
            scores = db.Fetch<BblScore>($"SELECT * FROM public.bbl_score WHERE uid = {user.Id}").OkOr(new(0)).ToArray();
            var userStats = CreateUserStats(user.Id, scores);
            WriteLine($"SET Stats UserId: {user.Id}, Score: {userStats.OverallScore}, ScoresRows: {scores.Length}");
            
            
            var com = npsql.CreateCommand();
            com.CommandText = $@"
INSERT 
INTO public.bbl_user_stats (
                              uid, 
                              overall_playcount, 
                              overall_score, 
                              overall_accuracy, 
                              overall_combo, 
                              overall_xs, 
                              overall_ss, 
                              overall_s, 
                              overall_a, 
                              overall_b, 
                              overall_c, 
                              overall_d, 
                              overall_hits, 
                              overall_300, 
                              overall_100, 
                              overall_50, 
                              overall_geki, 
                              overall_katu, 
                              overall_miss, 
                              overall_xss) 
VALUES (
    {userStats.Uid},
    {userStats.OverallPlaycount},
    {userStats.OverallScore},
    {userStats.OverallAccuracy},
    {userStats.OverallCombo},
    {userStats.OverallXs},
    {userStats.OverallSs},
    {userStats.OverallS},
    {userStats.OverallA},
    {userStats.OverallB},
    {userStats.OverallC},
    {userStats.OverallD},
    {userStats.OverallHits},
    {userStats.Overall300},
    {userStats.Overall100},
    {userStats.Overall50},
    {userStats.OverallGeki},
    {userStats.OverallKatu},
    {userStats.OverallMiss},
    {userStats.OverallXss}      
)";
            await com.ExecuteNonQueryAsync(token);
        }
    }

    private static void InsertAllUsers(NpgsqlConnection db, BblUser[] users) {
        var batch = db.CreateBatch();
        foreach (var user in users) {
            batch.BatchCommands.Add(new NpgsqlBatchCommand {
                CommandText = @"
Insert Into public.bbl_user (id, username, password, email, deviceid, regist_time, last_login_time, latest_ip, region, active, banned, restrict_mode, username_last_change, patron_email, patron_email_accept) 
VALUES (
        @Id, 
        @Username, 
        @Password, 
        @Email, 
        @Deviceid, 
        @RegistTime, 
        @LastLoginTime, 
        @LatestIp, 
        @Region, 
        @Active, 
        @Banned, 
        @RestrictMode, 
        @UsernameLastChange, 
        @PatronEmail, 
        @PatronEmailAccept)
",
                Parameters = {
                    new NpgsqlParameter { Value = user.Id, DbType = DbType.Int64, ParameterName = "Id" },
                    new NpgsqlParameter { Value = user.Username, DbType = DbType.String, ParameterName = "Username" },
                    new NpgsqlParameter { Value = user.Password, DbType = DbType.String, ParameterName = "Password" },
                    new NpgsqlParameter { Value = user.Email, DbType = DbType.String, ParameterName = "Email" },
                    new NpgsqlParameter { Value = user.Deviceid, DbType = DbType.String, ParameterName = "Deviceid" },
                    new NpgsqlParameter
                        { Value = user.RegistTime, DbType = DbType.DateTime, ParameterName = "RegistTime" },
                    new NpgsqlParameter
                        { Value = user.LastLoginTime, DbType = DbType.DateTime, ParameterName = "LastLoginTime" },
                    new NpgsqlParameter { Value = user.LatestIp, DbType = DbType.String, ParameterName = "LatestIp" },
                    new NpgsqlParameter { Value = user.Region, DbType = DbType.String, ParameterName = "Region" },
                    new NpgsqlParameter { Value = user.Active, DbType = DbType.Boolean, ParameterName = "Active" },
                    new NpgsqlParameter { Value = user.Banned, DbType = DbType.Boolean, ParameterName = "Banned" },
                    new NpgsqlParameter
                        { Value = user.RestrictMode, DbType = DbType.Boolean, ParameterName = "RestrictMode" },
                    new NpgsqlParameter {
                        Value = user.UsernameLastChange, DbType = DbType.DateTime, ParameterName = "UsernameLastChange"
                    },
                    new NpgsqlParameter {
                        Value = DBNull.Value, DbType = DbType.String, ParameterName = "PatronEmail", IsNullable = true
                    },
                    new NpgsqlParameter
                        { Value = user.PatronEmailAccept, DbType = DbType.Boolean, ParameterName = "PatronEmailAccept" }
                }
            });
        }

        batch.ExecuteNonQuery();
    }
    
    private static async ValueTask SingleTransfer(BblUser user, CancellationToken token) {
        try {
            var box = new StrongBox<BblScore[]?>(default);
            box.Value = await GetOldScoresByUserId(user.Id);
            
            if (box.Value is null || box.Value.Length <= 0) {
                WriteLine($"User id: {user.Id} Finish");
                return;
            }
        
            using var db = DbBuilder.BuildPostSqlAndOpen();
            
            var resultErr = await InsertScores(box.Value);
            if (resultErr == EResult.Err)
                throw new Exception(resultErr.Err() + "\n\n");
            
            WriteLine($"User id: {user.Id} Finish");
        }
        catch (Exception e) {
            WriteLine(e);
            WriteLine("------------------------------------------");
            System.Environment.Exit(1);
            throw;
        }
    }

    private static async Task<ResultErr<string>> InsertScores(BblScore[] scores) {
        await using var db = DbBuilder.BuildNpgsqlConnection();

        await using var batchScore = db.CreateBatch();
        foreach (var score in scores) { 
            var ins = new NpgsqlBatchCommand { 
                CommandText = @"
Insert Into public.bbl_score (id, uid, filename, hash, mode, score, combo, mark, geki, perfect, katu, good, bad, miss, date, accuracy) 
VALUES (
        @id, 
        @uid, 
        @filename, 
        @hash,
        @mode,
        @score,
        @combo,
        @mark,
        @geki,
        @perfect,
        @katu,
        @good,
        @bad,
        @miss,
        @date,
        @accuracy)
",
                Parameters = {
                    new NpgsqlParameter { Value = score.Id, DbType = DbType.Int64, ParameterName = "id" },
                    new NpgsqlParameter { Value = score.Uid, DbType = DbType.Int64, ParameterName = "uid" },
                    new NpgsqlParameter
                        { Value = score.Filename, DbType = DbType.String, ParameterName = "filename" },
                    new NpgsqlParameter { Value = score.Hash, DbType = DbType.String, ParameterName = "hash" },
                    new NpgsqlParameter { Value = score.Mode, DbType = DbType.String, ParameterName = "mode" },
                    new NpgsqlParameter { Value = score.Score, DbType = DbType.Int64, ParameterName = "score" },
                    new NpgsqlParameter { Value = score.Combo, DbType = DbType.Int64, ParameterName = "combo" },
                    new NpgsqlParameter { Value = score.Mark, DbType = DbType.String, ParameterName = "mark" },
                    new NpgsqlParameter { Value = score.Geki, DbType = DbType.Int64, ParameterName = "geki" },
                    new NpgsqlParameter
                        { Value = score.Perfect, DbType = DbType.Int64, ParameterName = "perfect" },
                    new NpgsqlParameter { Value = score.Katu, DbType = DbType.Int64, ParameterName = "katu" },
                    new NpgsqlParameter { Value = score.Good, DbType = DbType.Int64, ParameterName = "good" },
                    new NpgsqlParameter { Value = score.Bad, DbType = DbType.Int64, ParameterName = "bad" },
                    new NpgsqlParameter { Value = score.Miss, DbType = DbType.Int64, ParameterName = "miss" },
                    new NpgsqlParameter
                        { Value = score.Date, DbType = DbType.DateTime, ParameterName = "date" },
                    new NpgsqlParameter
                        { Value = score.Accuracy, DbType = DbType.Int64, ParameterName = "accuracy" }
                   }
            };
            batchScore.BatchCommands.Add(ins);
        }
        
        try {
            await batchScore.ExecuteNonQueryAsync(); 
            return ResultErr<string>.Ok();
        }
        
        catch (Exception e) {
            return ResultErr<string>.Err(e.ToString());
        }
    }

    private static bool IsNullOrEmptyOrNULLOrNullOrWhitespace(string? value)
        => value is null || value.Trim() is "" or " " or "NULL" or "null";
    
    private async Task<BblUser[]> GetBblUser() {
        using var db = DbBuilder.BuildPostSqlAndOpen();
        var bblUsersOld = (await db.FetchAsync<bbl_user>($"SELECT * FROM {Env.OldDatabase}.bbl_user")).OkOr(new());

         
        
        for (var i = bblUsersOld.Count - 1; i >= 0; i--) {
            var singleUser = bblUsersOld[i];

            if (IsNullOrEmptyOrNULLOrNullOrWhitespace(singleUser.Email) 
                || IsNullOrEmptyOrNULLOrNullOrWhitespace(singleUser.Password)
                || IsNullOrEmptyOrNULLOrNullOrWhitespace(singleUser.Username)
                || singleUser.Id < 0) {
                bblUsersOld.RemoveAt(i);
                continue;
            }

            singleUser.Password = singleUser.Password!.Trim();
            singleUser.Username = singleUser.Username!.Trim();
            singleUser.Email = singleUser.Email!.Trim();
        }

        IEnumerable<BblUser> RemoveAllEqEmails(IEnumerable<BblUser> enumerable) {
            
            var dictionary = new Dictionary<string, BblUser>(128000);
            foreach (var bblUser in enumerable.OrderBy(x => x.Id)) {
                if (dictionary.ContainsKey(bblUser.Email!))
                    continue;
                dictionary[bblUser.Email!] = bblUser;
            }

            return dictionary.Select(x => x.Value);
        }
        
        IEnumerable<BblUser> RemoveAllEqUsername(IEnumerable<BblUser> enumerable) {
            var dictionary = new Dictionary<string, BblUser>(128000);
            foreach (var bblUser in enumerable.OrderBy(x => x.Id)) {
                if (dictionary.ContainsKey(bblUser.Username!))
                    continue;
                dictionary[bblUser.Email!] = bblUser;
            }

            return dictionary.Select(x => x.Value);
        }
        
        var bblUsers = bblUsersOld.Select(bblUser => new BblUser {
            Active = bblUser.Active == 1,
            Banned = bblUser.Banned == 1,
            Deviceid = "",
            Email = bblUser.Email,
            Id = bblUser.Id,
            Password = bblUser.Password,
            Region = (bblUser.Region ?? "").ToUpper(),
            Username = bblUser.Username,
            PatronEmail = null,
            LatestIp = bblUser.RegistIp ?? "",
            RegistTime = DateTime.SpecifyKind(bblUser.RegistTime, DateTimeKind.Utc),
            RestrictMode = false,
            LastLoginTime = DateTime.SpecifyKind(bblUser.LastLoginTime, DateTimeKind.Utc),
            PatronEmailAccept = false,
            UsernameLastChange = DateTime.SpecifyKind(bblUser.RegistTime, DateTimeKind.Utc)
        });
        
        return RemoveAllEqUsername(RemoveAllEqEmails(bblUsers)).ToArray();
    }

    private static async Task<BblScore[]?> GetOldScoresByUserId(long id) {
        using var db = DbBuilder.BuildPostSqlAndOpen();
        List<bbl_score> scores =
            (await db.FetchAsync<bbl_score>($"SELECT * FROM {Env.OldDatabase}.bbl_score WHERE uid = {id}")).OkOr(new());

        for (var i = scores.Count - 1; i >= 0; i--) {
            var score = scores[i];
            if (score.Mode is null
                || score.Mode.IndexOf("x", StringComparison.Ordinal) != -1
                || score.Mode.IndexOf("AR", StringComparison.Ordinal) != -1
                || IsNullOrEmptyOrNULLOrNullOrWhitespace(score.Mark)
                || score.Score <= 0
                || score.Accuracy <= 0
                || IsNullOrEmptyOrNULLOrNullOrWhitespace(score.Hash)
                || IsNullOrEmptyOrNULLOrNullOrWhitespace(score.Filename)
               ) {

                scores.RemoveAt(i);
            }
        }

        foreach (var bblScore in scores) {
            if (bblScore.Mode is null)
                bblScore.Mode = "|";
            if (bblScore.Mode.Length == 0)
                bblScore.Mode = "|";
            if (bblScore.Mode.IndexOf("-", StringComparison.Ordinal) != -1)
                bblScore.Mode = "|";
            if (bblScore.Mode[^1] != '|')
                bblScore.Mode = "|";
        }

        var res = new BblScore[scores.Count];
        for (var i = 0; i < scores.Count; i++) {
            var bblScore = scores[i];

            res[i] = new BblScore {
                Id = bblScore.Id,
                Uid = bblScore.Uid,
                Filename = bblScore.Filename,
                Hash = bblScore.Hash,
                Mode = bblScore.Mode,
                Score = bblScore.Score,
                Combo = bblScore.Combo,
                Mark = bblScore.Mark,
                Geki = bblScore.Geki,
                Perfect = bblScore.Perfect,
                Katu = bblScore.Katu,
                Good = bblScore.Good,
                Bad = bblScore.Bad,
                Miss = bblScore.Miss,
                Date = DateTime.SpecifyKind(bblScore.Date, DateTimeKind.Utc),
                Accuracy = bblScore.Accuracy
            };
        }
        
        return res;
    }
    
    private static void RemoveAllRowsFromNewTables(NpgsqlConnection db) {
        using var batch = db.CreateBatch();
        NpgsqlCommand s;
        // s = db.CreateCommand();
        // s.CommandText = "DELETE FROM public.bbl_global_ranking_timeline";
        // s.ExecuteNonQuery();
        s = db.CreateCommand();
        s.CommandText = "DELETE FROM public.bbl_patron";
        s.ExecuteNonQuery();
        s = db.CreateCommand();
        s.CommandText = "DELETE FROM public.bbl_score";
        s.ExecuteNonQuery();
        s = db.CreateCommand();
        s.CommandText = "DELETE FROM public.bbl_score_banned";
        s.ExecuteNonQuery();
        s = db.CreateCommand();
        s.CommandText = "DELETE FROM public.bbl_score_pre_submit";
        s.ExecuteNonQuery();
        s = db.CreateCommand();
        s.CommandText = "DELETE FROM public.bbl_user_stats";
        s.ExecuteNonQuery();
        s = db.CreateCommand();
        s.CommandText = "DELETE FROM public.bbl_user";
        s.CommandTimeout = 1024;
        s.ExecuteNonQuery();
        
        batch.ExecuteNonQuery();
    }

    private static BblUserStats CreateUserStats(long userId, BblScore[] listBblScores) {
        var bblUserStats = new BblUserStats() {
                Uid = userId,
                OverallPlaycount = 0,
                OverallScore = 0,
                OverallAccuracy = 0,
                OverallCombo = 0,
                OverallXss = 0,
                OverallSs = 0,
                OverallXs = 0,
                OverallS = 0,
                OverallA = 0,
                OverallB = 0,
                OverallC = 0,
                OverallD = 0,
                OverallHits = 0,
                Overall300 = 0,
                Overall100 = 0,
                Overall50 = 0,
                OverallGeki = 0,
                OverallKatu = 0,
                OverallMiss = 0,
            };

        if (listBblScores.Length == 0) {
            Console.WriteLine($"UserId: {userId} Has Not Plays");
            return bblUserStats;
        }
            
                
        var dictionary = new Dictionary<string, BblScore>((listBblScores.Length + 1) / 2);
                
        foreach (var bblScore in listBblScores) {
            if (bblScore.Hash is null) continue;
                
            if (dictionary.TryGetValue(bblScore.Hash, out var inDic) == false) {
                dictionary[bblScore.Hash] = bblScore;
                continue;
            }
                    
            if (inDic.Score > bblScore.Score)
                continue;

            dictionary[bblScore.Hash] = bblScore;
        }
            
        foreach (var (key, bblScore) in dictionary) {
            bblUserStats.OverallScore += bblScore.Score;
            bblUserStats.OverallAccuracy += bblScore.Accuracy;
            bblUserStats.OverallCombo += bblScore.Combo;
            bblUserStats.OverallXss += bblScore.EqAsInt(BblScore.EMark.XSS);
            bblUserStats.OverallSs += bblScore.EqAsInt(BblScore.EMark.SS);
            bblUserStats.OverallXs += bblScore.EqAsInt(BblScore.EMark.XS);
            bblUserStats.OverallS += bblScore.EqAsInt(BblScore.EMark.S);
            bblUserStats.OverallA += bblScore.EqAsInt(BblScore.EMark.A);
            bblUserStats.OverallB += bblScore.EqAsInt(BblScore.EMark.B);
            bblUserStats.OverallC += bblScore.EqAsInt(BblScore.EMark.C);
            bblUserStats.OverallD += bblScore.EqAsInt(BblScore.EMark.D);
            bblUserStats.OverallHits += bblScore.GetValue(BblScore.EBblScore.Hits);
            bblUserStats.Overall300 += bblScore.GetValue(BblScore.EBblScore.N300);
            bblUserStats.Overall100 += bblScore.GetValue(BblScore.EBblScore.N100);
            bblUserStats.Overall50 += bblScore.GetValue(BblScore.EBblScore.N50);
            bblUserStats.OverallGeki += bblScore.Geki;
            bblUserStats.OverallKatu += bblScore.Katu;
            bblUserStats.OverallMiss += bblScore.Miss;
            bblUserStats.OverallPlaycount = dictionary.Count;
        }

        return bblUserStats;
    }
    
    //----old

//     public void Run(bool onlyUserStats = false) {
//         oldDb = DbBuilder.BuildPostSqlAndOpen();
//         if (onlyUserStats) {
//             FullUpdateUserStats();
//             return;
//         }
//         RemoveAllRowsFromNewTable();
//         ConvertScore();
//         ConvertUser();
//         MoveUser();
//         InsertScore();
//         RemoveScoresWithNoUser();
//         FullUpdateUserStats();
//         oldDb.Dispose();
//     }
//
//     
//
//     private void RemoveScoresWithNoUser() {
//         oldDb.Execute(@"
// DELETE
// FROM bbl_score
// WHERE id in (SELECT bbl_score.id
//              FROM bbl_score
//                  LEFT JOIN bbl_user f ON f.id = bbl_score.uid
//              WHERE f.id IS NULL
//              );
// ");
//     }
//
//     private void ConvertUser() {
//         oldDb.Execute(
//             $"ALTER TABLE {Env.OldDatabase}.bbl_user ALTER COLUMN active TYPE bool USING CASE WHEN active=1 THEN TRUE ELSE FALSE END;");
//         oldDb.Execute(
//             $"ALTER TABLE {Env.OldDatabase}.bbl_user ALTER COLUMN supporter TYPE bool USING CASE WHEN supporter=1 THEN TRUE ELSE FALSE END;");
//         oldDb.Execute(
//             $"ALTER TABLE {Env.OldDatabase}.bbl_user ALTER COLUMN banned TYPE bool USING CASE WHEN banned=1 THEN TRUE ELSE FALSE END;");
//         oldDb.Execute(
//             $"ALTER TABLE {Env.OldDatabase}.bbl_user ALTER COLUMN restrict_mode TYPE bool USING CASE WHEN restrict_mode=1 THEN TRUE ELSE FALSE END;");
//     }
//
//     private void ConvertScore() {
//         var database = Env.CrDbDatabase;
//         WriteLine("Start DELETE ROW AT NULL");
//         oldDb.Execute($@"
// DELETE
// FROM {Env.OldDatabase}.bbl_score
// WHERE mark IS NULL
//    or mark = 'NULL'
//    or mode IS NULL
//    or mode = 'NULL'
//    or mode = ''
//    or mark = ''
//    or mode like '%x%'
//    or mode like '%AR%';
// ");
//         oldDb.Execute($@"
// UPDATE {Env.OldDatabase}.bbl_score
// SET mode = '|'
// WHERE mode = '-';
// ");
//         oldDb.Execute($@"
// UPDATE {Env.OldDatabase}.bbl_score
// SET mode = concat(bbl_score.mode, '|')
// WHERE mode not like '%|';
// ");
//     }
//
//     private void MoveUser() {
//         WriteLine("Move User");
//         var userList = oldDb.Fetch<bbl_user>($@"SELECT * FROM {Env.OldDatabase}.bbl_user").OkOrDefault();
//         if (userList is null) throw new NullReferenceException(nameof(userList));
//
//         for (var i = userList.Count - 1; i >= 0; i--) {
//             var user = userList[i];
//             if (user.Email is null || user.Email == "NULL") {
//                 userList.RemoveAt(i);
//                 continue;
//             }
//
//             if (user.Banned == 0 && user.Playcount == 0) {
//                 userList.RemoveAt(i);
//                 continue;
//             }
//
//
//             if (user.Password is null || user.Password == "NULL") user.Password = string.Empty;
//
//             if (user.RegistIp is null || user.RegistIp == "NULL") user.RegistIp = "";
//         }
//
//         var emailMap = new Dictionary<string, bbl_user>();
//
//         for (var i = userList.Count - 1; i >= 0; i--) {
//             var user = userList[i];
//             if (emailMap.ContainsKey(user.Email!)) {
//                 if (user.LastLoginTime < emailMap[user.Email!].LastLoginTime) continue;
//                 emailMap[user.Email!] = user;
//                 continue;
//             }
//
//             emailMap.Add(user.Email!, user);
//         }
//
//         userList = emailMap.Select(x => x.Value).ToList();
//
//
//         WriteLine($"Fetch User: {userList.Count}");
//         using var db = DbBuilder.BuildNpgsqlConnection();
//         ;
//         var batchUser = db.CreateBatch();
//         var batchUserStats = db.CreateBatch();
//
//         foreach (var bblUser in userList) {
//             var user = new BblUser {
//                 Active = bblUser.Active == 1,
//                 Banned = bblUser.Banned == 1,
//                 Deviceid = "",
//                 Email = bblUser.Email,
//                 Id = bblUser.Id,
//                 Password = bblUser.Password,
//                 Region = (bblUser.Region ?? "").ToUpper(),
//                 Username = bblUser.Username,
//                 PatronEmail = null,
//                 LatestIp = bblUser.RegistIp ?? "",
//                 RegistTime = DateTime.SpecifyKind(bblUser.RegistTime, DateTimeKind.Utc),
//                 RestrictMode = false,
//                 LastLoginTime = DateTime.SpecifyKind(bblUser.LastLoginTime, DateTimeKind.Utc),
//                 PatronEmailAccept = false,
//                 UsernameLastChange = DateTime.SpecifyKind(bblUser.RegistTime, DateTimeKind.Utc)
//             };
//
//             var userStats = new BblUserStats {
//                 Uid = user.Id,
//                 OverallPlaycount = 0,
//                 OverallScore = 0,
//                 OverallAccuracy = 0,
//                 OverallCombo = 0,
//                 OverallXss = 0,
//                 OverallSs = 0,
//                 OverallXs = 0,
//                 OverallS = 0,
//                 OverallA = 0,
//                 OverallB = 0,
//                 OverallC = 0,
//                 OverallD = 0,
//                 OverallHits = 0,
//                 Overall300 = 0,
//                 Overall100 = 0,
//                 Overall50 = 0,
//                 OverallGeki = 0,
//                 OverallKatu = 0,
//                 OverallMiss = 0
//             };
//
//             batchUser.BatchCommands.Add(new NpgsqlBatchCommand {
//                 CommandText = @"
// Insert Into public.bbl_user (id, username, password, email, deviceid, regist_time, last_login_time, latest_ip, region, active, banned, restrict_mode, username_last_change, patron_email, patron_email_accept) 
// VALUES (
//         @Id, 
//         @Username, 
//         @Password, 
//         @Email, 
//         @Deviceid, 
//         @RegistTime, 
//         @LastLoginTime, 
//         @LatestIp, 
//         @Region, 
//         @Active, 
//         @Banned, 
//         @RestrictMode, 
//         @UsernameLastChange, 
//         @PatronEmail, 
//         @PatronEmailAccept)
// ",
//                 Parameters = {
//                     new NpgsqlParameter { Value = user.Id, DbType = DbType.Int64, ParameterName = "Id" },
//                     new NpgsqlParameter { Value = user.Username, DbType = DbType.String, ParameterName = "Username" },
//                     new NpgsqlParameter { Value = user.Password, DbType = DbType.String, ParameterName = "Password" },
//                     new NpgsqlParameter { Value = user.Email, DbType = DbType.String, ParameterName = "Email" },
//                     new NpgsqlParameter { Value = user.Deviceid, DbType = DbType.String, ParameterName = "Deviceid" },
//                     new NpgsqlParameter
//                         { Value = user.RegistTime, DbType = DbType.DateTime, ParameterName = "RegistTime" },
//                     new NpgsqlParameter
//                         { Value = user.LastLoginTime, DbType = DbType.DateTime, ParameterName = "LastLoginTime" },
//                     new NpgsqlParameter { Value = user.LatestIp, DbType = DbType.String, ParameterName = "LatestIp" },
//                     new NpgsqlParameter { Value = user.Region, DbType = DbType.String, ParameterName = "Region" },
//                     new NpgsqlParameter { Value = user.Active, DbType = DbType.Boolean, ParameterName = "Active" },
//                     new NpgsqlParameter { Value = user.Banned, DbType = DbType.Boolean, ParameterName = "Banned" },
//                     new NpgsqlParameter
//                         { Value = user.RestrictMode, DbType = DbType.Boolean, ParameterName = "RestrictMode" },
//                     new NpgsqlParameter {
//                         Value = user.UsernameLastChange, DbType = DbType.DateTime, ParameterName = "UsernameLastChange"
//                     },
//                     new NpgsqlParameter {
//                         Value = DBNull.Value, DbType = DbType.String, ParameterName = "PatronEmail", IsNullable = true
//                     },
//                     new NpgsqlParameter
//                         { Value = user.PatronEmailAccept, DbType = DbType.Boolean, ParameterName = "PatronEmailAccept" }
//                 }
//             });
//
//             batchUserStats.BatchCommands.Add(new NpgsqlBatchCommand {
//                 CommandText = @"
// INSERT INTO public.bbl_user_stats (uid, overall_playcount, overall_score, overall_accuracy, overall_combo, overall_xs, overall_ss, overall_s, overall_a, overall_b, overall_c, overall_d, overall_hits, overall_300, overall_100, overall_50, overall_geki, overall_katu, overall_miss, overall_xss) 
// VALUES (@Uid, 
//         @Playcount, 
//         @OverallScore, 
//         @OverallAccuracy, 
//         @OverallCombo, 
//         @OverallXs, 
//         @OverallSs, 
//         @OverallS, 
//         @OverallA, 
//         @OverallB, 
//         @OverallC, 
//         @OverallD, 
//         @OverallHits, 
//         @Overall300, 
//         @Overall100, 
//         @Overall50, 
//         @OverallGeki, 
//         @OverallKatu, 
//         @OverallMiss, 
//         @OverallXss
//         )
// ",
//                 Parameters = {
//                     new NpgsqlParameter { Value = userStats.Uid, DbType = DbType.Int64, ParameterName = "Uid" },
//                     new NpgsqlParameter
//                         { Value = userStats.OverallPlaycount, DbType = DbType.Int64, ParameterName = "Playcount" },
//                     new NpgsqlParameter
//                         { Value = userStats.OverallScore, DbType = DbType.Int64, ParameterName = "OverallScore" },
//                     new NpgsqlParameter
//                         { Value = userStats.OverallAccuracy, DbType = DbType.Int64, ParameterName = "OverallAccuracy" },
//                     new NpgsqlParameter
//                         { Value = userStats.OverallCombo, DbType = DbType.Int64, ParameterName = "OverallCombo" },
//                     new NpgsqlParameter
//                         { Value = userStats.OverallXs, DbType = DbType.Int64, ParameterName = "OverallXs" },
//                     new NpgsqlParameter
//                         { Value = userStats.OverallSs, DbType = DbType.Int64, ParameterName = "OverallSs" },
//                     new NpgsqlParameter
//                         { Value = userStats.OverallS, DbType = DbType.Int64, ParameterName = "OverallS" },
//                     new NpgsqlParameter
//                         { Value = userStats.OverallA, DbType = DbType.Int64, ParameterName = "OverallA" },
//                     new NpgsqlParameter
//                         { Value = userStats.OverallB, DbType = DbType.Int64, ParameterName = "OverallB" },
//                     new NpgsqlParameter
//                         { Value = userStats.OverallC, DbType = DbType.Int64, ParameterName = "OverallC" },
//                     new NpgsqlParameter
//                         { Value = userStats.OverallD, DbType = DbType.Int64, ParameterName = "OverallD" },
//                     new NpgsqlParameter
//                         { Value = userStats.OverallHits, DbType = DbType.Int64, ParameterName = "OverallHits" },
//                     new NpgsqlParameter
//                         { Value = userStats.Overall300, DbType = DbType.Int64, ParameterName = "Overall300" },
//                     new NpgsqlParameter
//                         { Value = userStats.Overall100, DbType = DbType.Int64, ParameterName = "Overall100" },
//                     new NpgsqlParameter
//                         { Value = userStats.Overall50, DbType = DbType.Int64, ParameterName = "Overall50" },
//                     new NpgsqlParameter
//                         { Value = userStats.OverallGeki, DbType = DbType.Int64, ParameterName = "OverallGeki" },
//                     new NpgsqlParameter
//                         { Value = userStats.OverallKatu, DbType = DbType.Int64, ParameterName = "OverallKatu" },
//                     new NpgsqlParameter
//                         { Value = userStats.OverallMiss, DbType = DbType.Int64, ParameterName = "OverallMiss" },
//                     new NpgsqlParameter
//                         { Value = userStats.OverallXss, DbType = DbType.Int64, ParameterName = "OverallXss" }
//                 }
//             });
//         }
//
//         WriteLine($"Insert User: {userList.Count}");
//         batchUser.ExecuteNonQuery();
//         WriteLine($"Insert User Stats: {userList.Count}");
//         batchUserStats.ExecuteNonQuery();
//         WriteLine("Insert Done");
//     }
//
//     private void InsertScore() {
//         static Task<int> InsertSpan(NpgsqlConnection db, Task<int>? taskOld, Span<bbl_score?> span) {
//             Task<int> task;
//             {
//                 var batchScore = db.CreateBatch();
//                 foreach (var bblScore in span) {
//                     if (bblScore is null) continue;
//
//                     var score = new BblScore {
//                         Id = bblScore.Id,
//                         Uid = bblScore.Uid,
//                         Filename = bblScore.Filename,
//                         Hash = bblScore.Hash,
//                         Mode = bblScore.Mode,
//                         Score = bblScore.Score,
//                         Combo = bblScore.Combo,
//                         Mark = bblScore.Mark,
//                         Geki = bblScore.Geki,
//                         Perfect = bblScore.Perfect,
//                         Katu = bblScore.Katu,
//                         Good = bblScore.Good,
//                         Bad = bblScore.Bad,
//                         Miss = bblScore.Miss,
//                         Date = DateTime.SpecifyKind(bblScore.Date, DateTimeKind.Utc),
//                         Accuracy = bblScore.Accuracy
//                     };
//                 }
//
//                 WriteLine($"Score Insert Count: {span.Length}");
//                 if (taskOld is not null) taskOld.Wait();
//                 task = batchScore.ExecuteNonQueryAsync();
//                 WriteLine("Score Insert End");
//             }
//             GC.Collect();
//             return task;
//         }
//
//         static HashSet<long> GetUserIds(SavePoco db) {
//             var dbRes = db.Fetch<BblUser>("SELECT id FROM public.bbl_user").Ok();
//             var res = new HashSet<long>(dbRes.Count);
//             foreach (var bblUser in dbRes) {
//                 res.Add(bblUser.Id);
//             }
//
//             return res;
//         }
//
//         using var db = DbBuilder.BuildNpgsqlConnection();
//
//         static List<bbl_score> GetScoreWithFixUserIds(SavePoco db) {
//             var userIds = GetUserIds(db);
//
//             var scores = db.Fetch<bbl_score>($@"SELECT * FROM {Env.OldDatabase}.bbl_score").OkOrDefault() ??
//                          new List<bbl_score>(0);
//             for (var i = scores.Count - 1; i >= 0; i--) {
//                 if (userIds.Contains(scores[i].Uid))
//                     continue;
//                 scores.RemoveAt(i);
//             }
//
//             return scores;
//         }
//
//         WriteLine("Move Score");
//         Span<bbl_score> scoreSpan = CollectionsMarshal.AsSpan(GetScoreWithFixUserIds(this.oldDb));
//         WriteLine($"Score Fetch Count: {scoreSpan.Length}");
//         for (var i = scoreSpan.Length - 1; i >= 0; i--) {
//             var score = scoreSpan[i];
//             if (score!.Score == 0 || string.IsNullOrWhiteSpace(score.Hash) || score.Hash is null) scoreSpan[i] = null;
//         }
//
//         WriteLine($"Score Fetch After Filter Count: {scoreSpan.Length}");
//
//         var posi = 0;
//         var maxPosi = scoreSpan.Length;
//         const int sliceSize = 10_000;
//         Task<int>? task = null;
//         while (true) {
//             WriteLine($"Posi: {posi} FROM {scoreSpan.Length}");
//             if (posi + sliceSize > maxPosi) {
//                 task = InsertSpan(db, task, scoreSpan.Slice(posi, scoreSpan.Length - posi));
//                 break;
//             }
//
//             task = InsertSpan(db, task, scoreSpan.Slice(posi, sliceSize));
//             posi += sliceSize;
//         }
//         
//         GC.Collect();
//     }
//
//     private void FullUpdateUserStats() {
//         GC.Collect();
//         WriteLine("FullUpdateUserScore Start");
//         WriteLine("Get All User");
//
//         var listBblUser = oldDb.Fetch<BblUser>(@"SELECT id FROM bbl_user").Ok();
//         WriteLine("Get All BblScore");
//         var userScores = new Dictionary<long, List<BblScore>>(listBblUser.Count);
//         foreach (var bblUser in listBblUser) 
//             userScores[bblUser.Id] = new List<BblScore>();
//         
//         foreach (var bblScore in oldDb.Fetch<BblScore>(@$"SELECT * FROM bbl_score").Ok()) {
//             List<BblScore>? list; 
//             if (userScores.TryGetValue(bblScore.Uid, out list) == false) {
//                 list = new List<BblScore>(64);
//                 userScores[bblScore.Uid] = list;
//             }
//             list.Add(bblScore);
//         }
//
//         WriteLine($"Users  Count: {listBblUser.Count}");
//         WriteLine($"Scores Count: {userScores.Count}");
//         WriteLine("Get All Calc Stats");
//         GC.Collect();
//         var queue = new ConcurrentQueue<BblUserStats>();
//         Parallel.ForEach(userScores, new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount * 2 },
//             pair => {
//                 pair.Deconstruct(out var userId, out var listBblScores);
//                 
//                 if (listBblScores.Count == 0)
//                     return;
//                 
//                 var bblUserStats = new BblUserStats() {
//                     Uid = userId,
//                     OverallPlaycount = 0,
//                     OverallScore = 0,
//                     OverallAccuracy = 0,
//                     OverallCombo = 0,
//                     OverallXss = 0,
//                     OverallSs = 0,
//                     OverallXs = 0,
//                     OverallS = 0,
//                     OverallA = 0,
//                     OverallB = 0,
//                     OverallC = 0,
//                     OverallD = 0,
//                     OverallHits = 0,
//                     Overall300 = 0,
//                     Overall100 = 0,
//                     Overall50 = 0,
//                     OverallGeki = 0,
//                     OverallKatu = 0,
//                     OverallMiss = 0,
//                 };
//                 
//                 var dictionary = new Dictionary<string, BblScore>((listBblScores.Count + 1) / 2);
//                 
//                 foreach (var bblScore in listBblScores) {
//                     if (bblScore.Hash is null) continue;
//
//                     bblUserStats.OverallPlaycount++;
//                     
//                     if (dictionary.TryGetValue(bblScore.Hash, out var inDic) == false) {
//                         dictionary[bblScore.Hash] = bblScore;
//                         continue;
//                     }
//                     
//                     if (inDic.Score > bblScore.Score)
//                         continue;
//
//                     dictionary[bblScore.Hash] = bblScore;
//                 }
//                 
//                 foreach (var (key, bblScore) in dictionary) {
//                     bblUserStats.OverallScore += bblScore.Score;
//                     bblUserStats.OverallAccuracy += bblScore.Accuracy;
//                     bblUserStats.OverallCombo += bblScore.Combo;
//                     bblUserStats.OverallXss += bblScore.EqAsInt(BblScore.EMark.XSS);
//                     bblUserStats.OverallSs += bblScore.EqAsInt(BblScore.EMark.SS);
//                     bblUserStats.OverallXs += bblScore.EqAsInt(BblScore.EMark.XS);
//                     bblUserStats.OverallS += bblScore.EqAsInt(BblScore.EMark.S);
//                     bblUserStats.OverallA += bblScore.EqAsInt(BblScore.EMark.A);
//                     bblUserStats.OverallB += bblScore.EqAsInt(BblScore.EMark.B);
//                     bblUserStats.OverallC += bblScore.EqAsInt(BblScore.EMark.C);
//                     bblUserStats.OverallD += bblScore.EqAsInt(BblScore.EMark.D);
//                     bblUserStats.OverallHits += bblScore.GetValue(BblScore.EBblScore.Hits);
//                     bblUserStats.Overall300 += bblScore.GetValue(BblScore.EBblScore.N300);
//                     bblUserStats.Overall100 += bblScore.GetValue(BblScore.EBblScore.N100);
//                     bblUserStats.Overall50 += bblScore.GetValue(BblScore.EBblScore.N50);
//                     bblUserStats.OverallGeki += bblScore.Geki;
//                     bblUserStats.OverallKatu += bblScore.Katu;
//                     bblUserStats.OverallMiss += bblScore.Miss;
//                 }
//                 
//                 queue.Enqueue(bblUserStats);
//             });
//         
//         listBblUser.Clear();
//         userScores.Clear();
//         GC.Collect();
//         
//
//         WriteLine("Update Stats In Db");
//         WriteLine($"Queue Start Size: {queue.Count}");
//         
//         Parallel.For(0, queue.Count, new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount * 2 }, (int _) => {
//             using var db = DbBuilder.BuildPostSqlAndOpen();
//             BblUserStats? userStats; 
//             if (queue.TryDequeue(out userStats) == false) 
//                 return;
//             
//             WriteLine($"(Queue: {queue.Count}) Update Stats User: {userStats.Uid}");
//             
//             db.Execute(@$"
// Update public.bbl_user_stats 
// SET
//     overall_playcount = {userStats.OverallPlaycount}, 
//     overall_score = {userStats.OverallScore}, 
//     overall_accuracy = {userStats.OverallAccuracy}, 
//     overall_combo = {userStats.OverallCombo}, 
//     overall_xss = {userStats.OverallXss}, 
//     overall_ss = {userStats.OverallSs}, 
//     overall_xs = {userStats.OverallXs}, 
//     overall_s = {userStats.OverallS}, 
//     overall_a = {userStats.OverallA}, 
//     overall_b = {userStats.OverallB}, 
//     overall_c = {userStats.OverallC}, 
//     overall_d = {userStats.OverallD}, 
//     overall_hits = {userStats.OverallHits},  
//     overall_300 = {userStats.Overall300}, 
//     overall_100 = {userStats.Overall100}, 
//     overall_50 = {userStats.Overall50}, 
//     overall_geki = {userStats.OverallGeki}, 
//     overall_katu = {userStats.OverallKatu}, 
//     overall_miss = {userStats.OverallMiss}
// WHERE uid = {userStats.Uid}
// ");
//         });
//         
//         
// //         
// //         
// //         Parallel.ForEach(oldDb.Fetch<BblUser>("SELECT id FROM public.bbl_user").Ok(), new ParallelOptions() {
// //             MaxDegreeOfParallelism = Environment.ProcessorCount * 2
// //         }, bblUser => {
// //             using var db = DbBuilder.BuildPostSqlAndOpen();
// //             
// //             
// //             var scoreMap = new Dictionary<string, BblScore>(102_400);
// //
// //             var scoreList =
// //                 db.Fetch<BblScore>($"SELECT * FROM public.bbl_score WHERE uid = {bblUser.Id}").OkOrDefault() ??
// //                 new List<BblScore>(0);
// //             
// //             
// //             
// //             
// //             
// //             
// //             foreach (var bblScore in scoreList) {
// //                 if (!scoreMap.ContainsKey(bblScore.Hash!)) {
// //                     scoreMap.Add(bblScore.Hash!, bblScore);
// //                     continue;
// //                 }
// //
// //                 var fromMap = scoreMap[bblScore.Hash!];
// //                 if (fromMap.Score >= bblScore.Score) continue;
// //
// //                 scoreMap[bblScore.Hash!] = bblScore;
// //             }
// //
// //             
// //             
// //             
// //             
// //             
// //             
// //             
// //             
// //             
// //             
// //             
// //             
// //             
// //             
// //             
// //             
// //             
// //             
// //             
// //             
// //             
// //             
// //             
// //             
// //             
// //             
// //             
// //             
// //             
// //             
// //             
// //             
// //             
// //             scoreList.Clear();
// //             var userStats = Flat.FlatToSingle<BblScore, BblUserStats>(scoreMap.Select(x => x.Value),
// //                 (bblScore, bblUserStats) => {
// //                     bblUserStats ??= new BblUserStats { Uid = bblScore.Uid };
// //
// //                     bblUserStats.Playcount++;
// //                     bblUserStats.OverallScore += bblScore.Score;
// //                     bblUserStats.OverallAccuracy += bblScore.Accuracy;
// //                     bblUserStats.OverallCombo += bblScore.Combo;
// //                     bblUserStats.OverallXss += bblScore.EqAsInt(BblScore.EMark.XSS);
// //                     bblUserStats.OverallSs += bblScore.EqAsInt(BblScore.EMark.SS);
// //                     bblUserStats.OverallXs += bblScore.EqAsInt(BblScore.EMark.XS);
// //                     bblUserStats.OverallS += bblScore.EqAsInt(BblScore.EMark.S);
// //                     bblUserStats.OverallA += bblScore.EqAsInt(BblScore.EMark.A);
// //                     bblUserStats.OverallB += bblScore.EqAsInt(BblScore.EMark.B);
// //                     bblUserStats.OverallC += bblScore.EqAsInt(BblScore.EMark.C);
// //                     bblUserStats.OverallD += bblScore.EqAsInt(BblScore.EMark.D);
// //                     bblUserStats.OverallHits += bblScore.GetValue(BblScore.EBblScore.Hits);
// //                     bblUserStats.Overall300 += bblScore.GetValue(BblScore.EBblScore.N300);
// //                     bblUserStats.Overall100 += bblScore.GetValue(BblScore.EBblScore.N100);
// //                     bblUserStats.Overall50 += bblScore.GetValue(BblScore.EBblScore.N50);
// //                     bblUserStats.OverallGeki += bblScore.Geki;
// //                     bblUserStats.OverallKatu += bblScore.Katu;
// //                     bblUserStats.OverallMiss += bblScore.Miss;
// //
// //                     return bblUserStats;
// //                 });
// //             scoreMap.Clear();
// //             if (userStats is null) return;
// //
// //             WriteLine("Update Stats User: " + bblUser.Id);
// //             db.Execute(@$"
// // Update public.bbl_user_stats 
// // SET
// //     playcount = {userStats.Playcount}, 
// //     overall_score = {userStats.OverallScore}, 
// //     overall_accuracy = {userStats.OverallAccuracy}, 
// //     overall_combo = {userStats.OverallCombo}, 
// //     overall_xss = {userStats.OverallXss}, 
// //     overall_ss = {userStats.OverallSs}, 
// //     overall_xs = {userStats.OverallXs}, 
// //     overall_s = {userStats.OverallS}, 
// //     overall_a = {userStats.OverallA}, 
// //     overall_b = {userStats.OverallB}, 
// //     overall_c = {userStats.OverallC}, 
// //     overall_d = {userStats.OverallD}, 
// //     overall_hits = {userStats.OverallHits},  
// //     overall_300 = {userStats.Overall300}, 
// //     overall_100 = {userStats.Overall100}, 
// //     overall_50 = {userStats.Overall50}, 
// //     overall_geki = {userStats.OverallGeki}, 
// //     overall_katu = {userStats.OverallKatu}, 
// //     overall_miss = {userStats.OverallMiss}
// // WHERE uid = {userStats.Uid}   
// // ");
//         // });
//     }
}