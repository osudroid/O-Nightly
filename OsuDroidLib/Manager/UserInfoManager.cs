using Npgsql;
using OsuDroidLib.Database.Entities;
using OsuDroidLib.Query;

namespace OsuDroidLib.Manager;

public static class UserInfoManager {
    public static async Task<Result<Option<UserInfo>, string>> GetByUserIdAsync(NpgsqlConnection db, long userId) {
        return await QueryUserInfo.GetByUserIdAsync(db, userId);
    }

    public static async Task<ResultErr<string>> UpdatePasswordAsync(NpgsqlConnection db, long userId,
        string newPassword) {
        var result = Lib.PasswordHash.HashWithBCryptPassword(newPassword);
        if (result == EResult.Err)
            return result;

        return await Query.QueryUserInfo.UpdatePasswordByUserIdAsync(db, userId, result.Ok());
    }

    public struct ResultValidatePasswordAndIfMd5UpdateIt {
        public bool PasswordIsValid { get; set; }
        public bool RehashPassword { get; set; }
        public bool ChangeToBCrypt { get; set; }

        public ResultValidatePasswordAndIfMd5UpdateIt(bool passwordIsValid = false, bool rehashPassword = false,
            bool changeToBCrypt = false) {
            PasswordIsValid = passwordIsValid;
            RehashPassword = rehashPassword;
            ChangeToBCrypt = changeToBCrypt;
        }
    }

    public static async Task<Result<ResultValidatePasswordAndIfMd5UpdateIt, string>> ValidatePasswordAndIfMd5UpdateIt(
        NpgsqlConnection db, long userId, string checkPassword, string dbPassword) {
        var passwordIsRight = OsuDroidLib.Lib.PasswordHash.IsRightPassword(checkPassword, dbPassword);
        if (passwordIsRight == EResult.Err)
            return passwordIsRight.ChangeOkType<ResultValidatePasswordAndIfMd5UpdateIt>();


        if (!passwordIsRight.Ok()) {
            return Result<ResultValidatePasswordAndIfMd5UpdateIt, string>.Ok(new(passwordIsValid: false));
        }

        var returnValue = new ResultValidatePasswordAndIfMd5UpdateIt(passwordIsValid: true);

        switch (Lib.PasswordHash.IsBCryptHash(dbPassword)) {
            case true:
                if (!OsuDroidLib.Lib.PasswordHash.BCryptNeedRehash(dbPassword).Ok()) {
                    break;
                }

                returnValue.RehashPassword = true;
                var newBcryptRehash = OsuDroidLib.Lib.PasswordHash.HashWithBCryptPassword(checkPassword);
                if (newBcryptRehash == EResult.Err)
                    return newBcryptRehash.ChangeOkType<ResultValidatePasswordAndIfMd5UpdateIt>();

                var resultErr = await UpdatePasswordAsync(db, userId, newBcryptRehash.Ok());
                if (resultErr == EResult.Err)
                    return resultErr.ConvertTo<ResultValidatePasswordAndIfMd5UpdateIt>();
                break;
            default:
                returnValue.ChangeToBCrypt = true;
                var newBcrypt = Lib.PasswordHash.HashWithBCryptPassword(checkPassword);
                if (newBcrypt == EResult.Err)
                    return newBcrypt.ChangeOkType<ResultValidatePasswordAndIfMd5UpdateIt>();

                await UpdatePasswordAsync(db, userId, newBcrypt.Ok());
                break;
        }

        return Result<ResultValidatePasswordAndIfMd5UpdateIt, string>.Ok(returnValue);
    }

    public static async Task<Result<bool, string>> UsernameIsInUse(
        NpgsqlConnection db, string username) {
        var result = await QueryUserInfo.GetUserIdByUsernameAsync(db, username.ToLower());
        if (result == EResult.Err)
            return result.ChangeOkType<bool>();
        return Result<bool, string>.Ok(result.Ok().IsSet());
    }

    public static async Task<ResultErr<string>> UpdateUsernameAsync(
        NpgsqlConnection db, long userId, string username) {
        return await QueryUserInfo.UpdateUsernameByUserIdAsync(db, userId, username);
    }
}