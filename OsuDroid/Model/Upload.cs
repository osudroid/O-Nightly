namespace OsuDroid.Model;

public static class Upload {
    public static Response<ApiTypes.Work, string> UploadReplay(
        string mapHash,
        long replayId,
        long userId,
        IFormFile odrApiStream
    ) {
        using var db = DbBuilder.BuildPostSqlAndOpen();

        var map = SqlFunc.GetBblScoreByIdAndUserId(db, replayId, userId).OkOrDefault();
        if (map is null)
            return Response<ApiTypes.Work, string>.Err("Map Not Found");

        var oldesMap = SqlFunc.GetBblScoreOldesByUserIdAndHash(db, userId, mapHash).OkOrDefault();

        if (oldesMap is null)
            return Response<ApiTypes.Work, string>.Err("Map Not Found");

        if (oldesMap.Id != map.Id)
            return Response<ApiTypes.Work, string>.Err("Id Miss Match");

        if (File.Exists($"{Env.ReplayPath}/{oldesMap.Id}.odr"))
            return Response<ApiTypes.Work, string>.Err("Not Allowed");

        using var stream = odrApiStream.OpenReadStream();
        using var file = File.Create($"{Env.ReplayPath}/{oldesMap.Id}.odr");

        file.Position = 0;
        CopyStream.Move(stream, file);
        file.Flush();

        return Response<ApiTypes.Work, string>.Ok(ApiTypes.Work.True);
    }
}