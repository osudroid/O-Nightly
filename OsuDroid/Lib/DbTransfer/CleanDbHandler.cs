namespace OsuDroid.Lib.DbTransfer;

internal static class CleanDbHandler {
    public static async Task Run() {
        await using (var db = await DbBuilder.BuildNpgsqlConnection()) {
            WriteLine("Start DELETE FROM public.Patron");
            await db.ExecuteAsync("DELETE FROM public.Patron", null, 4000);

            WriteLine("Start DELETE FROM public.PlayScore");
            await db.ExecuteAsync("DELETE FROM  public.PlayScore", null, 4000);

            WriteLine("Start DELETE FROM public.PlayscoreBanned");
            await db.ExecuteAsync("DELETE FROM  public.PlayscoreBanned", null, 4000);

            WriteLine("Start DELETE FROM public.PlayScorePreSubmit");

            await db.ExecuteAsync("DELETE FROM  public.PlayScorePreSubmit", null, 4000);
            WriteLine("Start DELETE FROM public.UserStats");

            await db.ExecuteAsync("DELETE FROM  public.UserStats", null, 4000);

            WriteLine("Start DELETE FROM public.UserInfo");
            await db.ExecuteAsync("DELETE FROM  public.UserInfo", null, 4000);

            WriteLine("Start DELETE FROM public.GlobalRankingTimeLine");
            await db.ExecuteAsync("DELETE FROM  public.GlobalRankingTimeLine", null, 4000);
        }
    }
}