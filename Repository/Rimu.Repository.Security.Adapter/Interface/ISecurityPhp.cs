namespace Rimu.Repository.Security.Adapter.Interface;

public interface ISecurityPhp {
    public string EncryptString(string content);

    public bool DecodeString(string content, string hashStr);

    public bool CheckRequest(string sign, Span<string> arr);

    public string SignRequest(Span<string> arr);
}