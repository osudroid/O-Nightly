using System.Reflection;
using OsuDroidExtraInterface;
using Path = OsuDroid.Utils.Path;

namespace OsuDroid.Lib;

public static class Security {
    private static ISecurity? _security;

    public static ISecurity GetSecurity() {
        if (_security is not null) return _security;
        try {
            WriteLine("TryLoad Load Extra Security DLL");
            var path = Path.Process() + Env.OSUDROID_SECURITY_DLL;
            var assembly = Assembly.LoadFile(path);
            _security = (ISecurity)Activator.CreateInstance(assembly.ExportedTypes.FirstOrDefault()!)!;
            WriteLine("Extra Security DLL Loaded");
            return _security;
        }
        catch (Exception) {
            WriteLine("Extra Security DLL Not Found Or Error");
            WriteLine("Use Default Security");
            _security = new PassSecurity();
            return _security;
        }
    }

    public class PassSecurity : ISecurity {
        public bool Api2HashValidate(string checkHash, string data, string token) {
            return true;
        }

        public bool DecodeString(string content, string hashStr) {
            return true;
        }

        public string EncryptString(string content) {
            return content;
        }

        public string SignRequest(params string[] arr) {
            return Merge.ObjectsToString(arr);
        }

        public bool CheckRequest(string sign, params string[] arr) {
            return true;
        }

        public bool CheckApiKey(string key) {
            return true;
        }

        public bool CheckIfApiKeyIsRoot(string key) {
            return true;
        }

        public bool CheckSudo(string key, string permissionName) {
            return true;
        }
    }
}