using System.Text;

namespace Rimu.Repository.Postgres.Adapter.Interface;

public interface IPlayStatsReadonly: IPp {
    public long Id { get;}
    public string[] Mode { get;}
    public long Score { get;}
    public long Combo { get;}
    public string Mark { get;}
    public long Geki { get;}
    public long Perfect { get;}
    public long Katu { get;}
    public long Good { get;}
    public long Bad { get;}
    public long Miss { get;}
    public DateTime Date { get;}
    public double Accuracy { get;}
    public new double Pp { get;}
    public long? ReplayFileId { get;}
    
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