namespace OsuDroidLib.Lib; 

public static class PasswordHash {
    public static bool IsBCryptHash(string hash) {
        return Lib.HashParser.IsValidHash(hash);
    }

    public static Result<string, string> HashWithBCryptPassword(string password) {
        try {
            return Result<string, string>
                .Ok(BCrypt.Net.BCrypt.HashPassword(password, Setting.Password_BCryptSalt!.Value));
        }
        catch (Exception e) {
            return Result<string, string>.Err(e.ToString());
        }
    }

    public static string HashWithOldPassword(string password) {
        return MD5.Hash(password + Setting.Password_Seed).ToLower();
    }

    public static Result<bool, string> IsRightPassword(string password, string hashFromDb) {
        if (IsBCryptHash(hashFromDb)) { 
            return Result<bool, string>.Ok(BCrypt.Net.BCrypt.Verify(password, hashFromDb));
        }

        return Result<bool, string>.Ok(HashWithOldPassword(password) == hashFromDb);
    }

    public static Result<bool, string> BCryptNeedRehash(string hashFromDb) {
        try {
            return Result<bool, string>.Ok(BCrypt.Net.BCrypt.PasswordNeedsRehash(
                hashFromDb,
                Setting.Password_BCryptSalt!.Value
            ));
        }
        catch (Exception e) {
            return Result<bool, string>.Err(e.ToString());
        }
    }
}