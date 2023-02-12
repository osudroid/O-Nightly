namespace OsuDroid.Database.TableFn; 

public static class Bbl {
    public static ResultErr<string> DeleteAccount(SavePoco db, long userId) {
        var resultBblUser = BblUser.GetUserById(db, userId);
        
        if (resultBblUser == EResult.Err)
            return resultBblUser; 
        if (resultBblUser.Ok().IsSet() == false)
            return ResultErr<string>.Err($"userId Not Found: {userId}");

        return BblUser.DeleteBblUser(db, userId);
    }
}