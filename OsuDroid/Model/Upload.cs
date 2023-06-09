using Npgsql;
using OsuDroidLib.Query;

namespace OsuDroid.Model;

public static class Upload {
    public static async Task<Result<ApiTypes.Work, string>> UploadReplay(
        NpgsqlConnection db,
        string mapHash,
        long replayId,
        long userId,
        IFormFile odrApiStream
    ) {
        
        var resultMap = await QueryPlayScore.GetPlayScoreByIdAndUserIdAsync(db, replayId, userId);

        if (resultMap == EResult.Err)
            return Result<ApiTypes.Work, string>.Err(resultMap.Err());

        var optionMap = resultMap.Ok();
        
        if (optionMap.IsSet() == false)
            return Result<ApiTypes.Work, string>.Err(TraceMsg.WithMessage("Map Not Found"));

        var map = optionMap.Unwrap();
        
        
        var resultOldesMap = await QueryPlayScore.GetPlayScoreOldesByUserIdAndHashAsync(db, userId, mapHash);

        if (resultOldesMap == EResult.Err)
            return Result<ApiTypes.Work, string>.Err(resultOldesMap.Err());

        var optionOldesMap = resultOldesMap.Ok();
        
        if (optionOldesMap.IsSet() == false)
            return Result<ApiTypes.Work, string>.Err(TraceMsg.WithMessage("Map Not Found"));

        var oldesMap = optionOldesMap.Unwrap();
        
        if (oldesMap.PlayScoreId != map.PlayScoreId)
            return Result<ApiTypes.Work, string>.Err(TraceMsg.WithMessage("Id Miss Match"));

        if (File.Exists($"{Env.ReplayPath}/{oldesMap.PlayScoreId}.odr"))
            return Result<ApiTypes.Work, string>.Err("Not Allowed");

        await using var stream = odrApiStream.OpenReadStream();
        await using var file = File.Create($"{Env.ReplayPath}/{oldesMap.PlayScoreId}.odr");

        file.Position = 0;
        CopyStream.Move(stream, file);
        file.Flush();

        return Result<ApiTypes.Work, string>.Ok(ApiTypes.Work.True);
    }
}