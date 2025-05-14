namespace Rimu.Repository.Postgres.Adapter.Interface;

public interface IPp {
    public double Pp { get; }
    
    public bool IsBetterThen(IPp other);
}