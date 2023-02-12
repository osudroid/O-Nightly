using OsuDroidLib.Database.Entities;

namespace OsuDroid.Model;

public static class Upload {
    public static Result<ApiTypes.Work, string> UploadReplay(
        string mapHash,
        long replayId,
        long userId,
        IFormFile odrApiStream
    ) {
        using var db = DbBuilder.BuildPostSqlAndOpen();

        var resultMap = SqlFunc.GetBblScoreByIdAndUserId(db, replayId, userId).Map(x => Option<BblScore>.NullSplit(x));

        if (resultMap == EResult.Err)
            return Result<ApiTypes.Work, string>.Err(resultMap.Err());

        var optionMap = resultMap.Ok();
        
        if (optionMap.IsSet() == false)
            return Result<ApiTypes.Work, string>.Err("Map Not Found");

        var map = optionMap.Unwrap();
        
        var resultOldesMap = SqlFunc
            .GetBblScoreOldesByUserIdAndHash(db, userId, mapHash)
            .Map(x => Option<BblScore>.NullSplit(x));

        if (resultOldesMap == EResult.Err)
            return Result<ApiTypes.Work, string>.Err(resultOldesMap.Err());

        var optionOldesMap = resultOldesMap.Ok();
        
        if (optionOldesMap.IsSet() == false)
            return Result<ApiTypes.Work, string>.Err("Map Not Found");

        var oldesMap = optionOldesMap.Unwrap();
        
        if (oldesMap.Id != map.Id)
            return Result<ApiTypes.Work, string>.Err("Id Miss Match");

        if (File.Exists($"{Env.ReplayPath}/{oldesMap.Id}.odr"))
            return Result<ApiTypes.Work, string>.Err("Not Allowed");

        using var stream = odrApiStream.OpenReadStream();
        using var file = File.Create($"{Env.ReplayPath}/{oldesMap.Id}.odr");

        file.Position = 0;
        CopyStream.Move(stream, file);
        file.Flush();

        return Result<ApiTypes.Work, string>.Ok(ApiTypes.Work.True);
    }
}