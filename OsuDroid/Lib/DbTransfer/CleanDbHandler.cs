using System.Collections.Concurrent;
using System.Data;
using System.Runtime.CompilerServices;
using Npgsql;
using OsuDroid.Database.OldEntities;
using OsuDroidLib.Database.Entities;
using OsuDroidLib.Extension;
using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;
using Dapper;
using Dapper.Contrib.Extensions;
using OsuDroidLib.Dto;

namespace OsuDroid.Lib.DbTransfer; 

internal static class CleanDbHandler {
    public static async Task Run() {
        await using (var db = await DbBuilder.BuildNpgsqlConnection()) {
            Console.WriteLine("Start DELETE FROM public.Patron");
            await db.ExecuteAsync("TRUNCATE public.Patron", null, null, commandTimeout: 4000);
            Console.WriteLine("Start DELETE FROM public.PlayScore");
            await db.ExecuteAsync("TRUNCATE public.PlayScore", null, null, commandTimeout: 4000);
            Console.WriteLine("Start DELETE FROM public.PlayscoreBanned");
            await db.ExecuteAsync("TRUNCATE public.PlayscoreBanned", null, null, commandTimeout: 4000);
            Console.WriteLine("Start DELETE FROM public.PlayScorePreSubmit");
            await db.ExecuteAsync("TRUNCATE public.PlayScorePreSubmit", null, null, commandTimeout: 4000);
            Console.WriteLine("Start DELETE FROM public.UserStats");
            await db.ExecuteAsync("TRUNCATE public.UserStats", null, null, commandTimeout: 4000);
            Console.WriteLine("Start DELETE FROM public.UserInfo");
            await db.ExecuteAsync("TRUNCATE public.UserInfo", null, null, commandTimeout: 4000);
            Console.WriteLine("Start DELETE FROM public.GlobalRankingTimeLine");
            await db.ExecuteAsync("TRUNCATE public.GlobalRankingTimeLine", null, null, commandTimeout: 4000);
        }
    }
}