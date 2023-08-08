using Newtonsoft.Json;

namespace OsuDroid.Utils;

public interface ILogRequestJsonPrint {
    public void LogRequestJsonPrint() {
        if (Setting.Log_RequestJsonPrint!.Value == false) return;

        try {
            WriteLine(JsonConvert.SerializeObject(this));
        }
        catch (Exception e) {
            WriteLine("{e}");
        }
    }
}