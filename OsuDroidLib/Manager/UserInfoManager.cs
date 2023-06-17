using Npgsql;

namespace OsuDroidLib.Manager; 

public static class UserInfoManager {
    public static async Task<ResultErr<string>> UpdatePasswordAsync(NpgsqlConnection db, long userId, string newPassword) {
        
    }
}