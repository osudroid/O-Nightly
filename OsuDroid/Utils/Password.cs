namespace OsuDroid.Utils;

public static class Password {
    /// <summary> It Use Env.PasswdSeed </summary>
    /// <param name="passwd"></param>
    /// <returns></returns>
    public static Result<string, string> Hash(string passwd) {
        try {
            return Result<string, string>.Ok(MD5.Hash(passwd + Env.PasswdSeed).ToLower());
        }
        catch (Exception e) {
            return Result<string, string>.Err(e.ToString());
        }
    }
}
