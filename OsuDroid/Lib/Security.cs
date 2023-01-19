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
            throw new NotImplementedException();
        }

        public bool DecodeString(string content, string hashStr) {
            throw new NotImplementedException(nameof(DecodeString));
        }

        public string EncryptString(string content) {
            throw new NotImplementedException(nameof(EncryptString));
        }

        public string SignRequest(params string[] arr) {
            throw new NotImplementedException(nameof(SignRequest));
        }

        public bool CheckRequest(string sign, params string[] arr) {
            throw new NotImplementedException(nameof(CheckRequest));
        }

        public bool CheckApiKey(string key) {
            throw new NotImplementedException(nameof(CheckApiKey));
        }

        public bool CheckIfApiKeyIsRoot(string key) {
            throw new NotImplementedException(nameof(CheckIfApiKeyIsRoot));
        }

        public bool CheckSudo(string key, string permissionName) {
            throw new NotImplementedException(nameof(CheckSudo));
        }
    }
}