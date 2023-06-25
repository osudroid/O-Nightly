using OsuDroid.Model;
using OsuDroid.Utils;

namespace OsuDroid.Class.Dto;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class PushPlayDto : ModelApi2Submit.IScoreProp {
    public required long Id { get; init; }
    public required long Uid { get; init; }
    public required string Filename { get; init; }
    public required string Hash { get; init; }
    public required string Mode { get; init; }
    public required long Score { get; init; }
    public required long Combo { get; init; }
    public required string Mark { get; init; }
    public required long Geki { get; init; }
    public required long Perfect { get; init; }
    public required long Katu { get; init; }
    public required long Good { get; init; }
    public required long Bad { get; init; }
    public required long Miss { get; init; }
    public required long Accuracy { get; init; }
}