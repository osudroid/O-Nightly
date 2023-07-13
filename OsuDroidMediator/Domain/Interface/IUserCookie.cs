using LamLibAllOver;
using Npgsql;

namespace OsuDroidMediator.Domain.Interface; 

public interface IUserCookie {
    public Option<Guid> GetCookie();
    public ResultErr<string> SetCookie(Guid cookie);
    public Option<(Guid Cookie, long UserId)> GetCookieAndUserId(NpgsqlConnection db);
}