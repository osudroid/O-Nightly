using System.IO.Compression;
using System.Text;
using LamLibAllOver.ErrorHandling;
using Newtonsoft.Json;
using Rimu.Repository.OdrZip.Adapter.Class;
using Rimu.Repository.OdrZip.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;

namespace Rimu.Repository.OdrZip.Domain;

public sealed class OdrZip: IOdrZip {
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    
    private readonly IQueryView_Play_PlayStats _queryView_Play_PlayStats;
    private readonly IQueryUserInfo _queryUserInfo;
    private readonly IQueryReplayFile _queryReplayFile;

    public OdrZip(
        IQueryView_Play_PlayStats queryViewPlayPlayStats, 
        IQueryUserInfo queryUserInfo, 
        IQueryReplayFile queryReplayFile) {
        
        _queryView_Play_PlayStats = queryViewPlayPlayStats;
        _queryUserInfo = queryUserInfo;
        _queryReplayFile = queryReplayFile;
    }

    private ResultOk<byte[]> CreateOdrZip(byte[] odrBytes, OdrEntry entry) {
        try
        {
            using var file = new MemoryStream();

            using var archive = new ZipArchive(file, ZipArchiveMode.Create, true);
            {
                var odrEntry = archive.CreateEntry(entry.Replay!.Replayfile!, CompressionLevel.SmallestSize);
                using (var odrEntryStream = odrEntry.Open()) {
                    odrEntryStream.Write(odrBytes, 0, odrBytes.Length);
                }
            }

            {
                var entryJson = archive.CreateEntry("entry.json", CompressionLevel.SmallestSize);
                using (var odrEntryStream = entryJson.Open()) {
                    var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(entry, Formatting.None));
                    odrEntryStream.Write(new Span<byte>(bytes));
                }
            }

            return ResultOk<byte[]>.Ok(file.ToArray());
        }
        catch (Exception e) {
            Logger.Error(e);
            return ResultOk<byte[]>.Err();
        }
    }


    public async Task<ResultOk<Option<(byte[] bytes, string name)>>> FactoryAsync(long odrNumber) {
        var resultPlay_PlayStats = await _queryView_Play_PlayStats.GetByIdAsync(odrNumber);

        if (resultPlay_PlayStats == EResult.Err) {
            return ResultOk<Option<(byte[] bytes, string name)>>.Err();
        }

        var optionPlay_PlayStats = resultPlay_PlayStats.Ok();
        if (optionPlay_PlayStats.IsSet() == false) {
            return SResult<Option<(byte[] bytes, string name)>>.Ok(Option<(byte[] bytes, string name)>
                .Empty
            );
        }

        var play_PlayStats = optionPlay_PlayStats.Unwrap();
        
        {
            var resultUserInfo = await _queryUserInfo.GetUsernameByUserIdAsync(play_PlayStats.UserId);

            if (resultUserInfo == EResult.Err) {
                return ResultOk<Option<(byte[] bytes, string name)>>.Err();
            }

            var userInfoOption = resultUserInfo.Ok();
            if (userInfoOption.IsSet() == false) {
                return SResult<Option<(byte[] bytes, string name)>>.Ok(
                    Option<(byte[] bytes, string name)>.Empty
                );
            }

            var replayFile = await _queryReplayFile.GetByIdAsync(odrNumber);
            if (replayFile == EResult.Err) {
                return ResultOk<Option<(byte[] bytes, string name)>>.Err();
            }

            if (replayFile.Ok().IsNotSet()) {
                return ResultOk<Option<(byte[] bytes, string name)>>.Ok(default);
            }
            
            return CreateOdrZip(
                replayFile.Ok().Unwrap().Odr, 
                OdrEntry.Factory(play_PlayStats, userInfoOption.Unwrap().Username ?? "")
            ).Map(x => Option<(byte[] stream, string name)>
                .With((x, $"{play_PlayStats.Filename}_{odrNumber}.zip"))
            ); 
        }
    }
}