using NetEscapades.EnumGenerators;

namespace OsuDroidAttachment.Class;

[EnumExtensions]
public enum EModelResult {
    Ok,
    BadRequest,
    InternalServerError,
    BadRequestWithMessage
}