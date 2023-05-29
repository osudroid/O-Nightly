namespace OsuDroidLib.Extension; 

public static class ExceptionMethods {
    public static ResultErr<string> ToResultErr(this Exception self) {
        return ResultErr<string>.Err(self.ToString());
    }
}