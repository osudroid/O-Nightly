using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using NetEscapades.EnumGenerators;
using OsuDroidLib.Database.Entities;

// ReSharper disable All

namespace OsuDroidLib.Dto;

public record A(bool B);


public record PlayScoreDtoE(
    long PlayScoreId,
    long UserId,
    string Filename,
    string Hash,
    string[] Mode,
    long Score,
    long Combo,
    string Mark,
    long Geki,
    long Perfect,
    long Katu,
    long Good,
    long Bad,
    long Miss,
    DateTime Date,
    long Accuracy
);

public class PlayScoreDto {
    public required long PlayScoreId { get; init; }
    public required long UserId { get; init; }
    public required string Filename { get; init; }
    public required string Hash { get; init; }
    public required string[] Mode { get; init; }
    public required long Score { get; init; }
    public required long Combo { get; init; }
    public required EPlayScoreMark Mark { get; init; }
    public required long Geki { get; init; }
    public required long Perfect { get; init; }
    public required long Katu { get; init; }
    public required long Good { get; init; }
    public required long Bad { get; init; }
    public required long Miss { get; init; }
    public required DateTime Date { get; init; }
    public required long Accuracy { get; init; }

    [EnumExtensions]
    public enum EPlayScore {
        Geki,
        Perfect,
        Katu,
        Good,
        Bad,
        Miss,
        Accuracy,
        Hits,
        N300,
        N100,
        N50
    }

    [EnumExtensions]
    public enum EPlayScoreMark {
        XSS,
        SS,
        XS,
        S,
        A,
        B,
        C,
        D
    }


    public long GetValue(EPlayScore ePlayScore) {
        return ePlayScore switch {
            EPlayScore.Geki => Geki,
            EPlayScore.Perfect => Perfect,
            EPlayScore.Katu => Katu,
            EPlayScore.Good => Good,
            EPlayScore.Bad => Bad,
            EPlayScore.Miss => Miss,
            EPlayScore.Accuracy => Accuracy,
            EPlayScore.Hits => Perfect + Perfect + Good + Bad + Geki + Katu,
            EPlayScore.N300 => Perfect,
            EPlayScore.N100 => Good,
            EPlayScore.N50 => Bad,
            _ => throw new ArgumentOutOfRangeException(nameof(ePlayScore), ePlayScore, null)
        };
    }


    public static PlayScoreDto operator -(PlayScoreDto newVal, PlayScoreDto oldVal) {
        return new PlayScoreDto {
            PlayScoreId = newVal.PlayScoreId,
            UserId = newVal.UserId,
            Filename = newVal.Filename,
            Hash = newVal.Hash,
            Mode = newVal.Mode,
            Score = newVal.Score - oldVal.Score,
            Combo = newVal.Combo - oldVal.Combo,
            Mark = newVal.Mark,
            Geki = newVal.Geki - oldVal.Geki,
            Perfect = newVal.Perfect - oldVal.Perfect,
            Katu = newVal.Katu - oldVal.Katu,
            Good = newVal.Good - oldVal.Good,
            Bad = newVal.Bad - oldVal.Bad,
            Miss = newVal.Miss - oldVal.Miss,
            Date = newVal.Date,
            Accuracy = newVal.Accuracy - oldVal.Accuracy
        };
    }

    public int EqAsInt(EPlayScoreMark playScoreMark) => Eq(playScoreMark) ? 1 : 0;

    public bool Eq(EPlayScoreMark playScoreMark) => Mark == playScoreMark;

    public static bool operator ==(PlayScoreDto score, EPlayScoreMark playScoreMark) {
        return !score.Eq(playScoreMark);
    }

    public static bool operator !=(PlayScoreDto score, EPlayScoreMark playScoreMark) {
        return !score.Eq(playScoreMark);
    }


    public static string? OldMarkOrMarkToMark(string mark) {
        mark = mark.ToUpper();
        return mark switch {
            // OLD TO NEW
            "XH" => "XSS",
            "SH" => "XS",
            "X" => "SS",
            // NEW
            "XSS" => mark,
            "SS" => mark,
            "XS" => mark,
            "S" => mark,
            "A" => mark,
            "B" => mark,
            "C" => mark,
            "D" => mark,
            _ => null
        };
    }

    public static Option<PlayScoreDto> ToPlayScoreDto(PlayScore playScore) {
        if (EPlayScoreMarkExtensions.TryParse(OldMarkOrMarkToMark(playScore.Mark ?? ""), out var mark) == false)
            return Option<PlayScoreDto>.Empty;

        return Option<PlayScoreDto>.With(new() {
            PlayScoreId = playScore.PlayScoreId,
            UserId = playScore.UserId,
            Filename = playScore.Filename ?? "",
            Hash = playScore.Hash ?? "",
            Mode = playScore.Mode ?? Array.Empty<string>(),
            Score = playScore.Score,
            Combo = playScore.Combo,
            Mark = mark,
            Geki = playScore.Geki,
            Perfect = playScore.Perfect,
            Katu = playScore.Katu,
            Good = playScore.Good,
            Bad = playScore.Bad,
            Miss = playScore.Miss,
            Date = playScore.Date,
            Accuracy = playScore.Accuracy,
        });
    }
    
    public override string ToString()
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append("PlayScoreDto");
        stringBuilder.Append(" { ");
        if (PrintMembers(stringBuilder))
        {
            stringBuilder.Append(' ');
        }
        stringBuilder.Append('}');
        return stringBuilder.ToString();
    }

    protected virtual bool PrintMembers(StringBuilder builder)
    {
        RuntimeHelpers.EnsureSufficientExecutionStack();
        builder.Append("PlayScoreId = ");
        builder.Append(PlayScoreId.ToString());
        builder.Append(", UserId = ");
        builder.Append(UserId.ToString());
        builder.Append(", Filename = ");
        builder.Append((object)Filename);
        builder.Append(", Hash = ");
        builder.Append((object)Hash);
        builder.Append(", Mode = ");
        builder.Append(Mode);
        builder.Append(", Score = ");
        builder.Append(Score.ToString());
        builder.Append(", Combo = ");
        builder.Append(Combo.ToString());
        builder.Append(", Mark = ");
        builder.Append(Mark.ToStringFast());
        builder.Append(", Geki = ");
        builder.Append(Geki.ToString());
        builder.Append(", Perfect = ");
        builder.Append(Perfect.ToString());
        builder.Append(", Katu = ");
        builder.Append(Katu.ToString());
        builder.Append(", Good = ");
        builder.Append(Good.ToString());
        builder.Append(", Bad = ");
        builder.Append(Bad.ToString());
        builder.Append(", Miss = ");
        builder.Append(Miss.ToString());
        builder.Append(", Date = ");
        builder.Append(Date.ToString());
        builder.Append(", Accuracy = ");
        builder.Append(Accuracy.ToString());
        return true;
    }
}