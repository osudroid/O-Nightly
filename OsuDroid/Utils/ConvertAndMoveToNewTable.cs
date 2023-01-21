using System.Collections.Concurrent;
using System.Data;
using System.Runtime.InteropServices;
using Npgsql;
using OsuDroid.Database.OldEntities;
using OsuDroidLib.Database.Entities;

namespace OsuDroid.Utils;

public class ConvertAndMoveToNewTable {
#pragma warning disable CS8618
    private SavePoco oldDb = null!;
#pragma warning restore CS8618

    public void Run(bool onlyUserStats = false) {
        oldDb = DbBuilder.BuildPostSqlAndOpen();
        if (onlyUserStats) {
            FullUpdateUserStats();
            return;
        }
        RemoveAllRowsFromNewTable();
        ConvertScore();
        ConvertUser();
        MoveUser();
        InsertScore();
        RemoveScoresWithNoUser();
        FullUpdateUserStats();
        oldDb.Dispose();
    }

    private void RemoveAllRowsFromNewTable() {
        oldDb.Execute("DELETE FROM public.bbl_global_ranking_timeline");
        oldDb.Execute("DELETE FROM public.bbl_patron");
        oldDb.Execute("DELETE FROM public.bbl_score");
        oldDb.Execute("DELETE FROM public.bbl_score_banned");
        oldDb.Execute("DELETE FROM public.bbl_score_pre_submit");
        oldDb.Execute("DELETE FROM public.bbl_user_stats");
        oldDb.Execute("DELETE FROM public.bbl_user");
    }

    private void RemoveScoresWithNoUser() {
        oldDb.Execute(@"
DELETE
FROM bbl_score
WHERE id in (SELECT bbl_score.id
             FROM bbl_score
                 LEFT JOIN bbl_user f ON f.id = bbl_score.uid
             WHERE f.id IS NULL
             );
");
    }

    private void ConvertUser() {
        oldDb.Execute(
            $"ALTER TABLE {Env.OldDatabase}.bbl_user ALTER COLUMN active TYPE bool USING CASE WHEN active=1 THEN TRUE ELSE FALSE END;");
        oldDb.Execute(
            $"ALTER TABLE {Env.OldDatabase}.bbl_user ALTER COLUMN supporter TYPE bool USING CASE WHEN supporter=1 THEN TRUE ELSE FALSE END;");
        oldDb.Execute(
            $"ALTER TABLE {Env.OldDatabase}.bbl_user ALTER COLUMN banned TYPE bool USING CASE WHEN banned=1 THEN TRUE ELSE FALSE END;");
        oldDb.Execute(
            $"ALTER TABLE {Env.OldDatabase}.bbl_user ALTER COLUMN restrict_mode TYPE bool USING CASE WHEN restrict_mode=1 THEN TRUE ELSE FALSE END;");
    }

    private void ConvertScore() {
        var database = Env.CrDbDatabase;
        WriteLine("Start DELETE ROW AT NULL");
        oldDb.Execute($@"
DELETE
FROM {Env.OldDatabase}.bbl_score
WHERE mark IS NULL
   or mark = 'NULL'
   or mode IS NULL
   or mode = 'NULL'
   or mode = ''
   or mark = ''
   or mode like '%x%'
   or mode like '%AR%';
");
        oldDb.Execute($@"
UPDATE {Env.OldDatabase}.bbl_score
SET mode = '|'
WHERE mode = '-';
");
        oldDb.Execute($@"
UPDATE {Env.OldDatabase}.bbl_score
SET mode = concat(bbl_score.mode, '|')
WHERE mode not like '%|';
");
    }

    private void MoveUser() {
        WriteLine("Move User");
        var userList = oldDb.Fetch<bbl_user>($@"SELECT * FROM {Env.OldDatabase}.bbl_user").OkOrDefault();
        if (userList is null) throw new NullReferenceException(nameof(userList));

        for (var i = userList.Count - 1; i >= 0; i--) {
            var user = userList[i];
            if (user.Email is null || user.Email == "NULL") {
                userList.RemoveAt(i);
                continue;
            }

            if (user.Banned == 0 && user.Playcount == 0) {
                userList.RemoveAt(i);
                continue;
            }


            if (user.Password is null || user.Password == "NULL") user.Password = string.Empty;

            if (user.RegistIp is null || user.RegistIp == "NULL") user.RegistIp = "";
        }

        var emailMap = new Dictionary<string, bbl_user>();

        for (var i = userList.Count - 1; i >= 0; i--) {
            var user = userList[i];
            if (emailMap.ContainsKey(user.Email!)) {
                if (user.LastLoginTime < emailMap[user.Email!].LastLoginTime) continue;
                emailMap[user.Email!] = user;
                continue;
            }

            emailMap.Add(user.Email!, user);
        }

        userList = emailMap.Select(x => x.Value).ToList();


        WriteLine($"Fetch User: {userList.Count}");
        using var db = DbBuilder.BuildNpgsqlConnection();
        ;
        var batchUser = db.CreateBatch();
        var batchUserStats = db.CreateBatch();

        foreach (var bblUser in userList) {
            var user = new BblUser {
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
            };

            var userStats = new BblUserStats {
                Uid = user.Id,
                Playcount = 0,
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
                OverallMiss = 0
            };

            batchUser.BatchCommands.Add(new NpgsqlBatchCommand {
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

            batchUserStats.BatchCommands.Add(new NpgsqlBatchCommand {
                CommandText = @"
INSERT INTO public.bbl_user_stats (uid, playcount, overall_score, overall_accuracy, overall_combo, overall_xs, overall_ss, overall_s, overall_a, overall_b, overall_c, overall_d, overall_hits, overall_300, overall_100, overall_50, overall_geki, overall_katu, overall_miss, overall_xss) 
VALUES (@Uid, 
        @Playcount, 
        @OverallScore, 
        @OverallAccuracy, 
        @OverallCombo, 
        @OverallXs, 
        @OverallSs, 
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
        @OverallMiss, 
        @OverallXss
        ) 
",
                Parameters = {
                    new NpgsqlParameter { Value = userStats.Uid, DbType = DbType.Int64, ParameterName = "Uid" },
                    new NpgsqlParameter
                        { Value = userStats.Playcount, DbType = DbType.Int64, ParameterName = "Playcount" },
                    new NpgsqlParameter
                        { Value = userStats.OverallScore, DbType = DbType.Int64, ParameterName = "OverallScore" },
                    new NpgsqlParameter
                        { Value = userStats.OverallAccuracy, DbType = DbType.Int64, ParameterName = "OverallAccuracy" },
                    new NpgsqlParameter
                        { Value = userStats.OverallCombo, DbType = DbType.Int64, ParameterName = "OverallCombo" },
                    new NpgsqlParameter
                        { Value = userStats.OverallXs, DbType = DbType.Int64, ParameterName = "OverallXs" },
                    new NpgsqlParameter
                        { Value = userStats.OverallSs, DbType = DbType.Int64, ParameterName = "OverallSs" },
                    new NpgsqlParameter
                        { Value = userStats.OverallS, DbType = DbType.Int64, ParameterName = "OverallS" },
                    new NpgsqlParameter
                        { Value = userStats.OverallA, DbType = DbType.Int64, ParameterName = "OverallA" },
                    new NpgsqlParameter
                        { Value = userStats.OverallB, DbType = DbType.Int64, ParameterName = "OverallB" },
                    new NpgsqlParameter
                        { Value = userStats.OverallC, DbType = DbType.Int64, ParameterName = "OverallC" },
                    new NpgsqlParameter
                        { Value = userStats.OverallD, DbType = DbType.Int64, ParameterName = "OverallD" },
                    new NpgsqlParameter
                        { Value = userStats.OverallHits, DbType = DbType.Int64, ParameterName = "OverallHits" },
                    new NpgsqlParameter
                        { Value = userStats.Overall300, DbType = DbType.Int64, ParameterName = "Overall300" },
                    new NpgsqlParameter
                        { Value = userStats.Overall100, DbType = DbType.Int64, ParameterName = "Overall100" },
                    new NpgsqlParameter
                        { Value = userStats.Overall50, DbType = DbType.Int64, ParameterName = "Overall50" },
                    new NpgsqlParameter
                        { Value = userStats.OverallGeki, DbType = DbType.Int64, ParameterName = "OverallGeki" },
                    new NpgsqlParameter
                        { Value = userStats.OverallKatu, DbType = DbType.Int64, ParameterName = "OverallKatu" },
                    new NpgsqlParameter
                        { Value = userStats.OverallMiss, DbType = DbType.Int64, ParameterName = "OverallMiss" },
                    new NpgsqlParameter
                        { Value = userStats.OverallXss, DbType = DbType.Int64, ParameterName = "OverallXss" }
                }
            });
        }

        WriteLine($"Insert User: {userList.Count}");
        batchUser.ExecuteNonQuery();
        batchUserStats.ExecuteNonQuery();
        WriteLine("Insert Done");
    }

    private void InsertScore() {
        static Task<int> InsertSpan(NpgsqlConnection db, Task<int>? taskOld, Span<bbl_score?> span) {
            Task<int> task;
            {
                var batchScore = db.CreateBatch();
                foreach (var bblScore in span) {
                    if (bblScore is null) continue;

                    var score = new BblScore {
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

                    batchScore.BatchCommands.Add(new NpgsqlBatchCommand {
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
                    });
                }

                WriteLine($"Score Insert Count: {span.Length}");
                if (taskOld is not null) taskOld.Wait();
                task = batchScore.ExecuteNonQueryAsync();
                WriteLine("Score Insert End");
            }
            GC.Collect();
            return task;
        }

        using var db = DbBuilder.BuildNpgsqlConnection();

        WriteLine("Move Score");
        var scoreSpan = CollectionsMarshal.AsSpan(
            oldDb.Fetch<bbl_score?>($@"SELECT * FROM {Env.OldDatabase}.bbl_score").OkOrDefault() ??
            new List<bbl_score?>(0));
        WriteLine("Score Fetch Count: " + scoreSpan.Length);
        for (var i = scoreSpan.Length - 1; i >= 0; i--) {
            var score = scoreSpan[i];
            if (score!.Score == 0 || string.IsNullOrWhiteSpace(score.Hash) || score.Hash is null) scoreSpan[i] = null;
        }

        WriteLine("Score Fetch After Filter Count: " + scoreSpan.Length);

        var posi = 0;
        var maxPosi = scoreSpan.Length;
        const int sliceSize = 10_000;
        Task<int>? task = null;
        while (true) {
            WriteLine($"Posi: {posi} FROM {scoreSpan.Length}");
            if (posi + sliceSize > maxPosi) {
                task = InsertSpan(db, task, scoreSpan.Slice(posi, scoreSpan.Length - posi));
                break;
            }

            task = InsertSpan(db, task, scoreSpan.Slice(posi, sliceSize));
            posi += sliceSize;
        }

        scoreSpan = Span<bbl_score?>.Empty;
        GC.Collect();
    }

    private void FullUpdateUserStats() {
        GC.Collect();
        WriteLine("FullUpdateUserScore Start");
        WriteLine("Get All User");

        var listBblUser = oldDb.Fetch<BblUser>(@"SELECT id FROM bbl_user").Ok();
        WriteLine("Get All BblScore");
        var userScores = new Dictionary<long, List<BblScore>>(listBblUser.Count);
        foreach (var bblUser in listBblUser) 
            userScores[bblUser.Id] = new List<BblScore>();
        
        foreach (var bblScore in oldDb.Fetch<BblScore>(@$"SELECT * FROM bbl_score").Ok()) {
            List<BblScore> list; 
            if (userScores.TryGetValue(bblScore.Uid, out list) == false) {
                list = new List<BblScore>(64);
                userScores[bblScore.Uid] = list;
            }
            list.Add(bblScore);
        }

        WriteLine($"Users  Count: {listBblUser.Count}");
        WriteLine($"Scores Count: {userScores.Count}");
        WriteLine("Get All Calc Stats");
        GC.Collect();
        var queue = new ConcurrentQueue<BblUserStats>();
        Parallel.ForEach(userScores, new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount * 2 },
            pair => {
                pair.Deconstruct(out var userId, out var listBblScores);
                
                if (listBblScores.Count == 0)
                    return;
                
                var bblUserStats = new BblUserStats() {
                    Uid = userId,
                    Playcount = 0,
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
                
                var dictionary = new Dictionary<string, BblScore>((listBblScores.Count + 1) / 2);
                
                foreach (var bblScore in listBblScores) {
                    if (bblScore.Hash is null) continue;

                    bblUserStats.Playcount++;
                    
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
                }
            });
        
        listBblUser.Clear();
        userScores.Clear();
        GC.Collect();
        

        WriteLine("Update Stats In Db");
        WriteLine($"Queue Start Size: {queue.Count}");
        
        Parallel.For(0, queue.Count, new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount * 2 }, (int _) => {
            using var db = DbBuilder.BuildPostSqlAndOpen();
            BblUserStats? userStats; 
            if (queue.TryDequeue(out userStats) == false) 
                return;
            
            WriteLine($"(Queue: {queue.Count}) Update Stats User: {userStats.Uid}");
            
            db.Execute(@$"
Update public.bbl_user_stats 
SET
    playcount = {userStats.Playcount}, 
    overall_score = {userStats.OverallScore}, 
    overall_accuracy = {userStats.OverallAccuracy}, 
    overall_combo = {userStats.OverallCombo}, 
    overall_xss = {userStats.OverallXss}, 
    overall_ss = {userStats.OverallSs}, 
    overall_xs = {userStats.OverallXs}, 
    overall_s = {userStats.OverallS}, 
    overall_a = {userStats.OverallA}, 
    overall_b = {userStats.OverallB}, 
    overall_c = {userStats.OverallC}, 
    overall_d = {userStats.OverallD}, 
    overall_hits = {userStats.OverallHits},  
    overall_300 = {userStats.Overall300}, 
    overall_100 = {userStats.Overall100}, 
    overall_50 = {userStats.Overall50}, 
    overall_geki = {userStats.OverallGeki}, 
    overall_katu = {userStats.OverallKatu}, 
    overall_miss = {userStats.OverallMiss}
WHERE uid = {userStats.Uid}   
");
        });
        
        
//         
//         
//         Parallel.ForEach(oldDb.Fetch<BblUser>("SELECT id FROM public.bbl_user").Ok(), new ParallelOptions() {
//             MaxDegreeOfParallelism = Environment.ProcessorCount * 2
//         }, bblUser => {
//             using var db = DbBuilder.BuildPostSqlAndOpen();
//             
//             
//             var scoreMap = new Dictionary<string, BblScore>(102_400);
//
//             var scoreList =
//                 db.Fetch<BblScore>($"SELECT * FROM public.bbl_score WHERE uid = {bblUser.Id}").OkOrDefault() ??
//                 new List<BblScore>(0);
//             
//             
//             
//             
//             
//             
//             foreach (var bblScore in scoreList) {
//                 if (!scoreMap.ContainsKey(bblScore.Hash!)) {
//                     scoreMap.Add(bblScore.Hash!, bblScore);
//                     continue;
//                 }
//
//                 var fromMap = scoreMap[bblScore.Hash!];
//                 if (fromMap.Score >= bblScore.Score) continue;
//
//                 scoreMap[bblScore.Hash!] = bblScore;
//             }
//
//             
//             
//             
//             
//             
//             
//             
//             
//             
//             
//             
//             
//             
//             
//             
//             
//             
//             
//             
//             
//             
//             
//             
//             
//             
//             
//             
//             
//             
//             
//             
//             
//             
//             scoreList.Clear();
//             var userStats = Flat.FlatToSingle<BblScore, BblUserStats>(scoreMap.Select(x => x.Value),
//                 (bblScore, bblUserStats) => {
//                     bblUserStats ??= new BblUserStats { Uid = bblScore.Uid };
//
//                     bblUserStats.Playcount++;
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
//
//                     return bblUserStats;
//                 });
//             scoreMap.Clear();
//             if (userStats is null) return;
//
//             WriteLine("Update Stats User: " + bblUser.Id);
//             db.Execute(@$"
// Update public.bbl_user_stats 
// SET
//     playcount = {userStats.Playcount}, 
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
        // });
    }
}