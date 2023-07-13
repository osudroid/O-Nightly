using NetEscapades.EnumGenerators;

namespace OsuDroid.View;

[EnumExtensions]
public enum EModelResult {
    Ok,
    BadRequest,
    InternalServerError
}