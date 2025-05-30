using System.IO.Compression;
using System.Text;
using LamLibAllOver.ErrorHandling;
using Newtonsoft.Json;
using Rimu.Repository.OdrZip.Adapter.Class;
using Rimu.Repository.OdrZip.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;

namespace Rimu.Repository.OdrZip.Domain;

/// <summary>
/// Represents the OdrZip class, responsible for creating and managing OdrZip files.
/// </summary>
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

    /// <summary>
    /// Creates an OdrZip file from the given replay data and entry information.
    /// </summary>
    /// <param name="odrBytes">The replay data as a byte array.</param>
    /// <param name="entry">The entry information for the replay.</param>
    /// <returns>A result containing the created OdrZip file as a byte array, or an error.</returns>
    private ResultOk<byte[]> CreateOdrZip(byte[] odrBytes, OdrEntry entry) {
        try
        {
            using var file = new MemoryStream();

            // Create a new zip archive and add the replay file
            using var archive = new ZipArchive(file, ZipArchiveMode.Create, true);
            {
                var odrEntry = archive.CreateEntry(entry.Replay!.Replayfile!, CompressionLevel.SmallestSize);
                using (var odrEntryStream = odrEntry.Open()) {
                    odrEntryStream.Write(odrBytes, 0, odrBytes.Length);
                }
            }

            // Add the entry metadata as a JSON file
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

    /// <summary>
    /// Asynchronously generates an OdrZip file for the given Odr number.
    /// </summary>
    /// <param name="odrNumber">The unique identifier for the Odr.</param>
    /// <returns>
    /// A result containing an optional tuple with the OdrZip file as a byte array and its name, or an error.
    /// </returns>
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