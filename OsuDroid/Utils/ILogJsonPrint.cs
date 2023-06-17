using Newtonsoft.Json;

namespace OsuDroid.Utils; 

public interface ILogRequestJsonPrint {
    public void LogRequestJsonPrint() {
        if (Setting.Log_RequestJsonPrint!.Value == false) return;
            
        try {
            Console.WriteLine(JsonConvert.SerializeObject(this));
        }
        catch (Exception e) {
            Console.WriteLine("{e}");
        }
    }
}