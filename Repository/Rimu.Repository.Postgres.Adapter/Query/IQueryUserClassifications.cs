using Rimu.Repository.Postgres.Adapter.Dto;
using Rimu.Repository.Postgres.Adapter.Entities;
using Rimu.Repository.Postgres.Adapter.Interface;
using UserClassificationsDto = Rimu.Repository.Postgres.Adapter.Dto.UserClassificationsDto;

namespace Rimu.Repository.Postgres.Adapter.Query;

public interface IQueryUserClassifications {
    public Task<ResultNone> InsertAsync(UserClassifications iUserClassificationsReadonly);
    public Task<ResultNone> InsertBulkAsync(UserClassifications[] userClassificationsArray);
    public Task<ResultOk<List<UserClassifications>>> GetAllCoreDeveloperOrDeveloperOrContributorOrSupporterAsync();
    public Task<ResultNone> UpdateAsync(UserClassifications iUserClassificationsReadonly);
    public Task<ResultNone> DeleteAsync(long id);
    public Task<ResultOk<Option<UserClassifications>>> GetByUserIdAsync(long id);
}