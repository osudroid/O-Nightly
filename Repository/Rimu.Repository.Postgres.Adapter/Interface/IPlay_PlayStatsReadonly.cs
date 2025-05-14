// ReSharper disable InconsistentNaming
namespace Rimu.Repository.Postgres.Adapter.Interface;

public interface IPlay_PlayStatsReadonly: IPlayReadonly, IPlayStatsReadonly {
    public new long Id { get; }
}