using System.Text;
using Rimu.Repository.Postgres.Adapter.Entities;
using Rimu.Repository.Postgres.Adapter.Interface;

namespace Rimu.Repository.Postgres.Adapter.Dto;

public record PlayPlayStatsDto(
    long Id,
    long UserId,
    string Filename,
    string FileHash,
    string[]? Mode,
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
    ) {

    public static PlayPlayStatsDto CreateWith(IPlay_PlayStatsReadonly value) {
        return new PlayPlayStatsDto(
                Id: value.Id,
                UserId: value.UserId,
                Filename: value.Filename,
                FileHash: value.FileHash,
                Mode: value.Mode,
                Score: value.Score,
                Combo: value.Combo,
                Mark: value.Mark,
                Geki: value.Geki,
                Perfect: value.Perfect,
                Katu: value.Katu,
                Good: value.Good,
                Bad: value.Bad,
                Miss: value.Miss,
                Date: value.Date,
                Accuracy: value.Accuracy,
                Pp: value.Pp,
                ReplayFileId: value.ReplayFileId
            );
    }
    
    public string ModeAsSingleString() {
        if (Mode is null) return "|";
        if (Mode.Length == 0) return "|";
        
        var builder = new StringBuilder();
        foreach (var m in Mode) {
            if (m.Length > 1) {
                builder.Append('|');
            }
            builder.Append(m);
        }
    
        return builder.ToString();
    }
}