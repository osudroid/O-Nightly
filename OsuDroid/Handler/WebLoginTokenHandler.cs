using OsuDroid.Class;
using OsuDroid.View;
using OsuDroidAttachment.Class;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Interface;
using OsuDroidLib.Manager;

namespace OsuDroid.Handler;

public class WebLoginTokenHandler
    : IHandler<NpgsqlCreates.DbWrapper, LogWrapper, ControllerWrapper, OptionHandlerOutput<ViewWebLoginToken>> {
    public async ValueTask<Result<OptionHandlerOutput<ViewWebLoginToken>, string>> Handel(
        NpgsqlCreates.DbWrapper dbWrapper,
        LogWrapper logger,
        ControllerWrapper request) {
        var db = dbWrapper.Db;
        var controller = request.Controller;


        var now = DateTime.UtcNow;

        Random random = new((int)(now.ToBinary() % short.MaxValue));
        var res = new ViewWebLoginToken {
            Token = Guid.NewGuid(),
            MathValue1 = random.Next(1, 50),
            MathValue2 = random.Next(1, 50)
        };

        var tokenResult = await WebLoginMathResultManager.AddWebLoginTokenAsync(db, new Entities.WebLoginMathResult {
                CreateTime = now,
                MathResult = res.MathValue1 + res.MathValue2,
                WebLoginMathResultId = res.Token
            }
        );

        if (tokenResult == EResult.Err) return tokenResult.ConvertTo<OptionHandlerOutput<ViewWebLoginToken>>();

        return Result<OptionHandlerOutput<ViewWebLoginToken>, string>
            .Ok(OptionHandlerOutput<ViewWebLoginToken>.With(res));
    }
}