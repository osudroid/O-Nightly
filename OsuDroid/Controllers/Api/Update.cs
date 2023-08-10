using System.Data;
using Microsoft.AspNetCore.Mvc;
using OsuDroid.Class;
using OsuDroid.Extensions;
using OsuDroid.Lib;
using OsuDroid.Model;
using OsuDroid.View;

namespace OsuDroid.Controllers.Api;

public class Update : ControllerExtensions {
    [HttpGet("/api/update")]
    [PrivilegeRoute("/api/update")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiTypes.ViewExistOrFoundInfo<ViewApiUpdateInfo>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUpdateInfoAsync([FromQuery(Name = "lang")] string lang = "en") {
        await using var dbN = await DbBuilder.BuildNpgsqlConnection();
        await using var dbT = await dbN.BeginTransactionAsync(IsolationLevel.Serializable);
        await using var db = dbT.Connection!;
        using var log = Log.GetLog(db);
        var isComplete = false;

        try {
            var result = await log.AddResultAndTransformAsync(
                await ModelApiUpdate.GetUpdateInfoAsync(this, db, lang)
            );

            if (result == EResult.Err) {
                if (!isComplete) {
                    isComplete = true;
                    await dbT.RollbackAsync();
                }

                return InternalServerError();
            }

            var modelResult = result.Ok();

            switch (result.Ok().Mode) {
                case EModelResult.Ok:
                    return Ok(result.Ok().Result.Unwrap());
                case EModelResult.BadRequest:
                    if (!isComplete) {
                        isComplete = true;
                        await dbT.RollbackAsync();
                    }

                    return InternalServerError();
                case EModelResult.InternalServerError:
                    if (!isComplete) {
                        isComplete = true;
                        await dbT.RollbackAsync();
                    }

                    return BadRequest();
                default:
                    if (!isComplete) {
                        isComplete = true;
                        await dbT.RollbackAsync();
                    }

                    return InternalServerError();
            }
        }
        catch (Exception e) {
            await log.AddLogErrorAsync(e.ToString());
            if (!isComplete) {
                isComplete = true;
                await dbT.RollbackAsync();
            }

            return InternalServerError();
        }
        finally {
            if (!isComplete) await dbT.CommitAsync();
        }
    }
}