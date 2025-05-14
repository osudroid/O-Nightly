// ReSharper disable InconsistentNaming
namespace Rimu.Repository.Postgres.Adapter.Interface;

public interface IPlay_PlayStatsHistoryReadonly: IPlayReadonly, IPlayStatsHistoryReadonly {
    public new long Id { get; }
}