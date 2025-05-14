using System.Text;
using Rimu.Repository.Postgres.Adapter.Dto;
using Rimu.Repository.Postgres.Adapter.Interface;

// ReSharper disable InconsistentNaming

namespace Rimu.Repository.Postgres.Adapter.Entities;

public class View_Play_PlayStats: IPlay_PlayStatsReadonly {
    public required long Id { get; set; }
    public required long UserId { get; set; }
    public required string Filename { get; set; } = "";
    public required string FileHash { get; set; } = "";
    public required string[] Mode { get; set; } = [];
    public required long Score { get; set; }
    public required long Combo { get; set; }
    public required string Mark { get; set; } = "";
    public required long Geki { get; set; }
    public required long Perfect { get; set; }
    public required long Katu { get; set; }
    public required long Good { get; set; }
    public required long Bad { get; set; }
    public required long Miss { get; set; }
    public required DateTime Date { get; set; }
    public required double Accuracy { get; set; }
    public required double Pp { get; set; }
    public required long? ReplayFileId { get; set; }

    public View_Play_PlayStats() {
    }

    public PlayPlayStatsDto ToDto() => PlayPlayStatsDto.CreateWith(this);

    public PlayStats ToPlayStats() {
        return new PlayStats {
            Id = this.Id,
            Mode = this.Mode,
            Score = this.Score,
            Combo = this.Combo,
            Mark = this.Mark,
            Geki = this.Geki,
            Perfect = this.Perfect,
            Katu = this.Katu,
            Good = this.Good,
            Bad = this.Bad,
            Miss = this.Miss,
            Date = this.Date,
            Accuracy = this.Accuracy,
            ReplayFileId = this.ReplayFileId,
            Pp = this.Pp
        };
    }
    
    public static PlayStats[] ToPlayStatsArray<T>(T view_Play_PlayStats) where T: IReadOnlyList<View_Play_PlayStats> {
        var playStatsArr = new PlayStats[view_Play_PlayStats.Count];
        for (int i = 0; i < view_Play_PlayStats.Count; i++) {
            playStatsArr[i] = view_Play_PlayStats[i].ToPlayStats();
        }
        
        return playStatsArr;
    }
    
    public PlayStatsHistory ToPlayStatsHistory() {
        return new PlayStatsHistory {
            Id = this.Id,
            Score = this.Score,
            Date = this.Date,
            ReplayFileId = this.ReplayFileId,
            Pp = this.Pp,
            PlayId = this.Id 
        };
    }
    
    
    public bool IsBetterThen(IPp other) => Pp > other.Pp;

    public Play ToPlay() {
        return new Play() {
            FileHash = this.FileHash,
            Filename = this.Filename,
            Id = this.Id,
            UserId = this.UserId,
        };
    }
    
    public static Play[] ToPlays<T>(T view_Play_PlayStats) where T: IReadOnlyList<View_Play_PlayStats> {
        var playArr = new Play[view_Play_PlayStats.Count];
        for (int i = 0; i < view_Play_PlayStats.Count; i++) {
            playArr[i] = view_Play_PlayStats[i].ToPlay();
        }
        
        return playArr;
    }
}