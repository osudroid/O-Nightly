using NetEscapades.EnumGenerators;

namespace OsuDroidMediator.Domain.Model; 

[EnumExtensions]
public enum EModelResult {
    Ok,
    BadRequest,
    InternalServerError,
    BadRequestWithMessage,
}