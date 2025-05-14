using System.Runtime.CompilerServices;
using System.Text;
using LamLibAllOver;
using NetEscapades.EnumGenerators;
using Rimu.Repository.Postgres.Adapter.Interface;

// ReSharper disable All

namespace Rimu.Repository.Postgres.Adapter.Dto;

public record struct PlayPlayStatsDtoE(
    long Id,
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
    double Accuracy,
    double Pp,
    long? ReplayFileId
) : IPlayStatsReadonly {
    public bool IsBetterThen(IPp other) {
        return this.Pp < other.Pp;
    }
}

public struct PlayStatsDto: IPlayStatsReadonly {
    private static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    
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
        N50,
        Pp
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

    
    
    
    public required long Id { get; init; }
    public required string[] Mode { get; init; }
    public required long Score { get; init; }
    public required long Combo { get; init; }

    string IPlayStatsReadonly.Mark => this.Mark.ToString();

    public required EPlayScoreMark Mark { get; init; }
    public required long Geki { get; init; }
    public required long Perfect { get; init; }
    public required long Katu { get; init; }
    public required long Good { get; init; }
    public required long Bad { get; init; }
    public required long Miss { get; init; }
    public required DateTime Date { get; init; }
    public required double Accuracy { get; init; }
    public required double Pp { get; init; }
    public required long? ReplayFileId { get; init; }
    
    
    public decimal GetValue(EPlayScore ePlayScore) {
        return ePlayScore switch {
            EPlayScore.Geki => Geki,
            EPlayScore.Perfect => Perfect,
            EPlayScore.Katu => Katu,
            EPlayScore.Good => Good,
            EPlayScore.Bad => Bad,
            EPlayScore.Miss => Miss,
            EPlayScore.Accuracy => (decimal)Accuracy,
            EPlayScore.Hits => Perfect + Perfect + Good + Bad + Geki + Katu,
            EPlayScore.N300 => Perfect,
            EPlayScore.N100 => Good,
            EPlayScore.N50 => Bad,
            EPlayScore.Pp => (decimal)Pp,
            _ => throw new ArgumentOutOfRangeException(nameof(ePlayScore), ePlayScore, null)
        };
    }
    public bool IsBetterThen(IPp other) => this.Pp < other.Pp;

    public static PlayStatsDto operator -(PlayStatsDto newVal, PlayStatsDto oldVal) {
        return new PlayStatsDto {
            Id           = newVal.Id,
            Mode         = newVal.Mode,
            Mark         = newVal.Mark,
            Date         = newVal.Date,
            ReplayFileId = newVal.ReplayFileId,
            Score        = newVal.Score    - oldVal.Score,
            Combo        = newVal.Combo    - oldVal.Combo,
            Geki         = newVal.Geki     - oldVal.Geki,
            Perfect      = newVal.Perfect  - oldVal.Perfect,
            Katu         = newVal.Katu     - oldVal.Katu,
            Good         = newVal.Good     - oldVal.Good,
            Bad          = newVal.Bad      - oldVal.Bad,
            Miss         = newVal.Miss     - oldVal.Miss,
            Accuracy     = newVal.Accuracy - oldVal.Accuracy,
            Pp           = newVal.Pp       - oldVal.Pp,
        };
    }

    public int MarkEqualAsInt(EPlayScoreMark playScoreMark) => MarkEqual(playScoreMark) ? 1 : 0;

    public bool MarkEqual(EPlayScoreMark playScoreMark) => Mark == playScoreMark;
    public bool MarkNotEqual(EPlayScoreMark playScoreMark) => Mark == playScoreMark;

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
    
    
    /// <exception cref="Exception"></exception>
    public static PlayStatsDto Create(IPlayStatsReadonly playPlayStats) {
        var work = EPlayScoreMark.TryParse(OldMarkOrMarkToMark(playPlayStats.Mark), out EPlayScoreMark playScoreMark);
        if (work == false) {
            Logger.Error("Can Not Convert Mark With String: {}", playPlayStats.Mark);
            throw new Exception("Can Not Convert Mark");
        }
        
        return new () {
            Id = playPlayStats.Id,
            Mode = playPlayStats.Mode,
            Score = playPlayStats.Score,
            Combo = playPlayStats.Combo,
            Mark = playScoreMark,
            Geki = playPlayStats.Geki,
            Perfect = playPlayStats.Perfect,
            Katu = playPlayStats.Katu,
            Good = playPlayStats.Good,
            Bad = playPlayStats.Bad,
            Miss = playPlayStats.Miss,
            Date = playPlayStats.Date,
            Accuracy = playPlayStats.Accuracy,
            Pp = playPlayStats.Pp,
            ReplayFileId = playPlayStats.ReplayFileId,
        };
    }
}