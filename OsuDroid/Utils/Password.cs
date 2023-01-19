namespace OsuDroid.Utils;

public static class Password {
    /// <summary> It Use Env.PasswdSeed </summary>
    /// <param name="passwd"></param>
    /// <returns></returns>
    public static Response<string, string> Hash(string passwd) {
        try {
            WriteLine("Beee:: " + passwd + Env.PasswdSeed);

            return Response<string, string>.Ok(MD5.Hash(passwd + Env.PasswdSeed).ToLower());
        }
        catch (Exception e) {
            return Response<string, string>.Err(e.ToString());
        }
    }
}