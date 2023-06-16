using OsuDroidLib.Dto;

namespace OsuDroid.View;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class ViewPlaysMarksLength {
    public long PlaysXSS { get; set; }
    public long PlaysSS { get; set; }
    public long PlaysXS { get; set; }
    public long PlaysS { get; set; }
    public long PlaysA { get; set; }
    public long PlaysB { get; set; }
    public long PlaysC { get; set; }
    public long PlaysD { get; set; }
    public long PlaysAll { get; set; }

    public static ViewPlaysMarksLength Factory(Dictionary<PlayScoreDto.EPlayScoreMark, long> dictionary) {
        return new ViewPlaysMarksLength() {
            PlaysXSS = dictionary.TryGetValue(PlayScoreDto.EPlayScoreMark.XSS, out var value)? value: 0,
            PlaysSS = dictionary.TryGetValue(PlayScoreDto.EPlayScoreMark.SS, out var value1)? value1: 0,
            PlaysXS = dictionary.TryGetValue(PlayScoreDto.EPlayScoreMark.XS, out var value2)? value2: 0,
            PlaysS = dictionary.TryGetValue(PlayScoreDto.EPlayScoreMark.S, out var value3)? value3: 0,
            PlaysA = dictionary.TryGetValue(PlayScoreDto.EPlayScoreMark.A, out var value4)? value4: 0,
            PlaysB = dictionary.TryGetValue(PlayScoreDto.EPlayScoreMark.B, out var value5)? value5: 0,
            PlaysC = dictionary.TryGetValue(PlayScoreDto.EPlayScoreMark.C, out var value6)? value6: 0,
            PlaysD = dictionary.TryGetValue(PlayScoreDto.EPlayScoreMark.D, out var value7)? value7: 0,
            PlaysAll = dictionary.Select(x => x.Value).Sum()
        };
    }
}