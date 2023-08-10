using Npgsql;
using OsuDroidLib.Database.Entities;
using OsuDroidLib.Lib;
using OsuDroidLib.Query;

namespace OsuDroidLib.Manager;

public static class UserInfoManager {
    public static async Task<Result<Option<UserInfo>, string>> GetByUserIdAsync(NpgsqlConnection db, long userId) {
        return await QueryUserInfo.GetByUserIdAsync(db, userId);
    }

    public static async Task<Result<Option<UserInfo>, string>> GetIdUsernamePasswordByLowerUsernameAsync(
        NpgsqlConnection db,
        string username) {
        return await QueryUserInfo.GetIdUsernamePasswordByLowerUsernameAsync(db, username.ToLower());
    }

    public static async Task<ResultErr<string>> UpdatePasswordAsync(
        NpgsqlConnection db,
        long userId,
        string newPassword) {
        var result = PasswordHash.HashWithBCryptPassword(newPassword);
        if (result == EResult.Err)
            return result;

        return await QueryUserInfo.UpdatePasswordByUserIdAsync(db, userId, result.Ok());
    }

    public static async Task<Result<ResultValidatePasswordAndIfMd5UpdateIt, string>> ValidatePasswordAndIfMd5UpdateIt(
        NpgsqlConnection db,
        long userId,
        string checkPassword,
        string dbPassword) {
        var passwordIsRight = PasswordHash.IsRightPassword(checkPassword, dbPassword);
        if (passwordIsRight == EResult.Err)
            return passwordIsRight.ChangeOkType<ResultValidatePasswordAndIfMd5UpdateIt>();


        if (!passwordIsRight.Ok())
            return Result<ResultValidatePasswordAndIfMd5UpdateIt, string>.Ok(
                new ResultValidatePasswordAndIfMd5UpdateIt(false)
            );

        var returnValue = new ResultValidatePasswordAndIfMd5UpdateIt(true);

        switch (PasswordHash.IsBCryptHash(dbPassword)) {
            case true:
                if (!PasswordHash.BCryptNeedRehash(dbPassword).Ok()) break;

                returnValue.RehashPassword = true;
                var newBcryptRehash = PasswordHash.HashWithBCryptPassword(checkPassword);
                if (newBcryptRehash == EResult.Err)
                    return newBcryptRehash.ChangeOkType<ResultValidatePasswordAndIfMd5UpdateIt>();

                var resultErr = await UpdatePasswordAsync(db, userId, newBcryptRehash.Ok());
                if (resultErr == EResult.Err)
                    return resultErr.ConvertTo<ResultValidatePasswordAndIfMd5UpdateIt>();
                break;
            default:
                returnValue.ChangeToBCrypt = true;
                var newBcrypt = PasswordHash.HashWithBCryptPassword(checkPassword);
                if (newBcrypt == EResult.Err)
                    return newBcrypt.ChangeOkType<ResultValidatePasswordAndIfMd5UpdateIt>();

                await UpdatePasswordAsync(db, userId, newBcrypt.Ok());
                break;
        }

        return Result<ResultValidatePasswordAndIfMd5UpdateIt, string>.Ok(returnValue);
    }

    public static async Task<Result<bool, string>> UsernameIsInUse(
        NpgsqlConnection db,
        string username) {
        var result = await QueryUserInfo.GetUserIdByUsernameAsync(db, username.ToLower());
        if (result == EResult.Err)
            return result.ChangeOkType<bool>();
        return Result<bool, string>.Ok(result.Ok().IsSet());
    }

    public static async Task<ResultErr<string>> UpdateUsernameAsync(
        NpgsqlConnection db,
        long userId,
        string username) {
        return await QueryUserInfo.UpdateUsernameByUserIdAsync(db, userId, username);
    }

    public static async ValueTask<Result<Option<UserInfo>, string>> GetByEmailAsync(NpgsqlConnection db, string email) {
        return await QueryUserInfo.GetByEmailAsync(db, email);
    }

    public struct ResultValidatePasswordAndIfMd5UpdateIt {
        public bool PasswordIsValid { get; set; }
        public bool RehashPassword { get; set; }
        public bool ChangeToBCrypt { get; set; }

        public ResultValidatePasswordAndIfMd5UpdateIt(
            bool passwordIsValid = false,
            bool rehashPassword = false,
            bool changeToBCrypt = false) {
            PasswordIsValid = passwordIsValid;
            RehashPassword = rehashPassword;
            ChangeToBCrypt = changeToBCrypt;
        }
    }
}