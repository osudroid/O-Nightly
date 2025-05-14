namespace Rimu.Repository.Authentication.Adapter.Interface;

public interface IPasswordProvider {
    public string HashPassword(string password);
    public bool VerifyPassword(string password, string hash);
    
    public bool NeedRehash(string hash);
}