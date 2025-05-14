using System.Buffers;
using LamLibAllOver;
using Rimu.Repository.Security.Adapter.Interface;

namespace Rimu.Repository.Security.Domain;

public class PassSecurity : ISecurity, ISecurityPhp {
    public bool Api2HashValidate(string checkHash, string data, string token) {
        return true;
    }

    public bool DecodeString(string content, string hashStr) {
        return true;
    }

    public string EncryptString(string content) {
        return content;
    }

    public string SignRequest(params Span<string> arr) {
        var buff = new Object[arr.Length];
        for (int i = 0; i < arr.Length; i++) {
            buff[i] = arr[i];
        }
        return Merge.ObjectsToString(buff);
    }

    public bool CheckRequest(string sign, params Span<string> arr) {
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