using Dapper;

namespace OsuDroid.Lib.DbTransfer;

internal static class CleanDbHandler {
    public static async Task Run() {
        await using (var db = await DbBuilder.BuildNpgsqlConnection()) {
            WriteLine("Start DELETE FROM public.Patron");
            await db.ExecuteAsync("DELETE FROM public.Patron");
            WriteLine("Start DELETE FROM public.PlayScore");
            await db.ExecuteAsync("DELETE FROM  public.PlayScore");

            WriteLine("Start DELETE FROM public.PlayscoreBanned");
            await db.ExecuteAsync("DELETE FROM  public.PlayscoreBanned");

            WriteLine("Start DELETE FROM public.PlayScorePreSubmit");

            await db.ExecuteAsync("DELETE FROM  public.PlayScorePreSubmit");
            WriteLine("Start DELETE FROM public.UserStats");

            await db.ExecuteAsync("DELETE FROM  public.UserStats");

            WriteLine("Start DELETE FROM public.UserInfo");
            await db.ExecuteAsync("DELETE FROM  public.UserInfo");

            WriteLine("Start DELETE FROM public.GlobalRankingTimeLine");
            await db.ExecuteAsync("DELETE FROM  public.GlobalRankingTimeLine");
        }
    }
}